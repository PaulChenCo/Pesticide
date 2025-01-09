using Newtonsoft.Json;
using PesticideContext;
using HtmlAgilityPack;

namespace thirdpartyAPI;
public class PesticideDataFetcher
{
    private readonly DatabaseContext _context;
    private readonly HttpClient _httpClient;

    public PesticideDataFetcher(HttpClient httpClient, DatabaseContext context)
    {
        _httpClient = httpClient;
        _context = context;
    }

    // 抓取農藥資料
    public async Task<List<Dictionary<string, object>>> FetchPesticideDataAsync()
    {
        string url = "https://data.moa.gov.tw/Service/OpenData/FromM/PesticideData.aspx";
        var allData = new List<Dictionary<string, object>>();
        int skip = 0;
        int top = 1000;

        while (true)
        {
            var requestUrl = $"{url}?$top={top}&$skip={skip}";
            var response = await _httpClient.GetAsync(requestUrl);
            if (response.IsSuccessStatusCode)
            {
                var jsonResponse = await response.Content.ReadAsStringAsync();
                var data = JsonConvert.DeserializeObject<List<Dictionary<string, object>>>(jsonResponse);

                if (data != null && data.Count > 0)
                {
                    allData.AddRange(data);
                    skip += top;
                }
                else
                {
                    break; // 如果沒有資料，跳出迴圈
                }
            }
            else
            {
                break; // 如果請求失敗，跳出迴圈
            }
        }
        return ConvertPesticideData(allData);
    }
    
    public async Task<object> FetchPesticideBarcodeAsync(List<Dictionary<string, object>> originalData)
    {
        var allJsonData = new List<Dictionary<string, object>>();  // 用來儲存所有請求的結果
        var maxDegreeOfParallelism = 1;  // 控制並發數量，根據需求調整
        var semaphore = new SemaphoreSlim(maxDegreeOfParallelism);  // 控制並發數量

        // 創建所有並行請求的任務
        var tasks = originalData.Select(async data =>
        {
            await semaphore.WaitAsync();  // 確保並行數量不超過限制
            var brcodeXMLUrl = data["GetBrcodeXML"].ToString();

            var retries = 3;  // 設置最多重試 3 次
            var success = false;
            HttpResponseMessage response = null;

            var cts = new CancellationTokenSource(TimeSpan.FromSeconds(120));  // 每個請求有 120 秒的超時
            var token = cts.Token;

            while (retries > 0 && !success)
            {
                try
                {
                    // 進行 HTTP 請求，傳遞 CancellationToken
                    response = await _httpClient.GetAsync(brcodeXMLUrl, token).ConfigureAwait(false);

                    if (response.IsSuccessStatusCode)
                    {
                        var jsonResponse = await response.Content.ReadAsStringAsync();
                        var jsonData = JsonConvert.DeserializeObject<List<Dictionary<string, object>>>(jsonResponse);

                        if (jsonData != null && jsonData.Count > 0)
                        {
                            lock (allJsonData)  // 使用鎖來保護並行寫入 allJsonData
                            {
                                allJsonData.AddRange(jsonData);  // 將 jsonData 加入到 allJsonData
                            }
                        }

                        success = true;  // 請求成功，退出重試循環
                    }
                    else
                    {
                        retries--;  // 如果不是 200 OK，減少重試次數
                        if (retries > 0)
                        {
                            await Task.Delay(1000);  // 重試前的延遲 1 秒
                        }
                    }
                }
                catch (TaskCanceledException ex)
                {
                    // 如果請求超時或被取消，減少重試次數
                    retries--;
                    if (retries > 0)
                    {
                        Console.WriteLine($"Request to {brcodeXMLUrl} was canceled. Retrying... {retries} attempts left.");
                        await Task.Delay(1000);  // 重試前的延遲 1 秒
                    }
                    else
                    {
                        // 超過最大重試次數後，記錄錯誤並退出
                        Console.WriteLine($"Request to {brcodeXMLUrl} was canceled after multiple retries: {ex.Message}");
                        return new { success = false, message = "Request timed out after multiple retries." };
                    }
                }
                catch (Exception ex)
                {
                    retries--;  // 如果發生其他錯誤，減少重試次數
                    if (retries > 0)
                    {
                        await Task.Delay(1000);  // 重試前的延遲 1 秒
                    }
                    else
                    {
                        // 發生未知錯誤，記錄錯誤並退出
                        Console.WriteLine($"Error fetching data from {brcodeXMLUrl}: {ex.Message}");
                        return new { success = false, message = $"Request failed: {ex.Message}" };
                    }
                }
            }

            semaphore.Release();  // 完成請求後釋放鎖定
            return new { success = success, message = success ? "Request succeeded." : "Request failed." };
        }).ToList();

        // 等待所有請求完成
        var results = await Task.WhenAll(tasks);

        // 最後將所有結果傳遞給 InsertProduct
        if (allJsonData.Count > 0)
        {
            InsertProduct(allJsonData);  // 傳遞所有資料的合集
        }

        return results.ToList();
    }

    public async Task<Boolean> FetchPesticideRegister()
    {
        bool result = false;
        var request = new HttpRequestMessage(HttpMethod.Get, "https://pesticide.aphia.gov.tw/information/Query/Register");
        request.Headers.Referrer = new Uri("https://pesticide.aphia.gov.tw/information");

        var response = await _httpClient.SendAsync(request);
        response.EnsureSuccessStatusCode();
        var registerHtml = await response.Content.ReadAsStringAsync();
        var htmlDocument = new HtmlDocument();
        htmlDocument.LoadHtml(registerHtml);
        var selectNode = htmlDocument.DocumentNode.SelectSingleNode("//select[@id='selectRegtid']").SelectNodes(".//option");

        if (selectNode != null)
        {
            var registerList = selectNode.ToList().Select(option =>
            {
                var regtid = option.Attributes["value"].Value;
                var regtname = option.InnerText;
                return new Register { RegtID = regtid, Permit = regtname };
            }).Where(r => r.RegtID != string.Empty).ToList();

            if (registerList.Count > 0)
            {
                _context.Register.RemoveRange(_context.Register);
                _context.Register.AddRange(registerList);
                await _context.SaveChangesAsync();
                result = true;
            }
        }
        return result;
    }

    public List<Dictionary<string, object>> ConvertPesticideData(List<Dictionary<string, object>> originalData)
    {
        var convertedData = new List<Dictionary<string, object>>();
        foreach (var data in originalData)
        {
            var convertedItem = new Dictionary<string, object>();
            foreach (var item in data)
            {
                string key = item.Key switch
                {
                    "許可證字" => "Permit",
                    "許可證號" => "PermitNumber",
                    "中文名稱" => "ChineseName",
                    "農藥代號" => "PesticideCode",
                    "英文名稱" => "EnName",
                    "廠牌名稱" => "BrandName",
                    "化學成分" => "ChemicalComposition",
                    "國外原製造廠商" => "ForeignMaker",
                    "劑型" => "formCode",
                    "含量" => "contents",
                    "有效期限" => "ExpireDate",
                    "廠商名稱" => "Vendor",
                    "FRAC殺菌劑抗藥性" => "FRAC",
                    "HRAC除草劑抗藥性" => "HRAC",
                    "IRAC殺蟲劑抗藥性" => "IRAC",
                    "撤銷類別" => "RevocationType",
                    "撤銷日期" => "RevocationDate",
                    "農藥使用範圍" => "ScopeOfUse",
                    "許可證標示" => "GetFile",
                    "農藥條碼資料" => "GetBrcodeXML",
                    "混合劑百分率" => "GetGropty",
                    _ => item.Key,
                };
                convertedItem.Add(key, item.Value);
            }
            convertedData.Add(convertedItem);
        }
        return convertedData;
    }

    public void InsertPesticideData(List<Dictionary<string, object>> pesticideData)
    {
        _context.Pesticide.AddRange(pesticideData.Select(data => new PesticideContext.Pesticide
        {
            Id = Guid.NewGuid(),
            Permit = data.TryGetValue("Permit", out var permit) ? permit?.ToString() : null,
            PermitNumber = data.TryGetValue("PermitNumber", out var permitNumber) ? permitNumber?.ToString() : null,
            ChineseName = data.TryGetValue("ChineseName", out var chineseName) ? chineseName?.ToString() : null,
            PesticideCode = data.TryGetValue("PesticideCode", out var pesticideCode) ? pesticideCode?.ToString() : null,
            EnName = data.TryGetValue("EnName", out var enName) ? enName?.ToString() : null,
            BrandName = data.TryGetValue("BrandName", out var brandName) ? brandName?.ToString() : null,
            ChemicalComposition = data.TryGetValue("ChemicalComposition", out var chemicalComposition) ? chemicalComposition?.ToString() : null,
            ForeignMaker = data.TryGetValue("ForeignMaker", out var foreignMaker) ? foreignMaker?.ToString() : null,
            formCode = data.TryGetValue("formCode", out var formCode) ? formCode?.ToString() : null,
            contents = data.TryGetValue("contents", out var contents) ? contents?.ToString() : null,
            ExpireDate = data.TryGetValue("ExpireDate", out var expireDate) ? expireDate?.ToString() : null,
            Vendor = data.TryGetValue("Vendor", out var vendor) ? vendor?.ToString() : null,
            FRAC = data.TryGetValue("FRAC", out var frac) ? frac?.ToString() : null,
            HRAC = data.TryGetValue("HRAC", out var hrac) ? hrac?.ToString() : null,
            IRAC = data.TryGetValue("IRAC", out var irac) ? irac?.ToString() : null,
            RevocationType = data.TryGetValue("RevocationType", out var revocationType) ? revocationType?.ToString() : null,
            RevocationDate = data.TryGetValue("RevocationDate", out var revocationDate) ? revocationDate?.ToString() : null,
            ScopeOfUse = data.TryGetValue("ScopeOfUse", out var scopeOfUse) ? scopeOfUse?.ToString() : null,
            GetFile = data.TryGetValue("GetFile", out var getFile) ? getFile?.ToString() : null,
            GetBrcodeXML = data.TryGetValue("GetBrcodeXML", out var getBrcodeXML) ? getBrcodeXML?.ToString() : null,
            GetGropty = data.TryGetValue("GetGropty", out var getGropty) ? getGropty?.ToString() : null
        }));
        _context.SaveChanges();
    }

    public void InsertProduct(List<Dictionary<string, object>> Product)
    {
        foreach (var data in Product)
        {
            var barcode = data.TryGetValue("Barcode", out var barcodeValue) ? barcodeValue?.ToString() : null;
            var product = _context.Product.Find(barcode);
            if (product == null)
            {
                product = new PesticideContext.Product
                {
                    Barcode = barcode
                };
                _context.Product.Add(product);
            }
            product.LicType = data.TryGetValue("LicType", out var licType) ? licType?.ToString() : null;
            product.LicNo = data.TryGetValue("LicNo", out var licNo) ? licNo?.ToString() : null;
            product.Seq = data.TryGetValue("Seq", out var seq) ? seq?.ToString() : null;
            product.Spec = data.TryGetValue("Spec", out var spec) ? spec?.ToString() : null;
            product.Unit = data.TryGetValue("Unit", out var unit) ? unit?.ToString() : null;
            product.Volume = data.TryGetValue("Volume", out var volume) ? volume?.ToString() : null;
            product.UpdateDT = data.TryGetValue("UpdateDT", out var updateDT) ? updateDT?.ToString() : null;
        }
        _context.SaveChanges();
    }
}
