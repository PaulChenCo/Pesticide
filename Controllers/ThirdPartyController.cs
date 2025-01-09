using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using PesticideContext;
using thirdpartyAPI;

namespace Pesticide.Controllers;

[ApiController]
[Route("[controller]")]
public class ThirdPartyController : ControllerBase
{
    private readonly PesticideDataFetcher _dataFetcher;
    private readonly DatabaseContext _context;

    public ThirdPartyController(DatabaseContext context, PesticideDataFetcher dataFetcher)
    {
        _context = context;
        _dataFetcher = dataFetcher;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<ThirdParty>>> Get()
    {
        string sql = "";
        sql = "IF NOT EXISTS(SELECT * FROM ThirdParty)";
        sql += "BEGIN ";
        sql += "INSERT INTO ThirdParty(DataStatus,DataTime) VALUES(0,GETDATE());";
        sql += "END ";
        sql += "SELECT DataStatus,DataTime FROM ThirdParty;";
        
        return await _context.ThirdParty.FromSqlRaw(sql).ToListAsync();
    }

    [HttpGet]
    [Route("TestData")]
    public async Task<ActionResult<IEnumerable<Dictionary<string, object>>>> TestData()
    {   
        List<Dictionary<string, object>> list = await _dataFetcher.FetchPesticideDataAsync();
        list = list.Take(1).ToList();
        _dataFetcher.FetchPesticideBarcodeAsync(list);
        return Ok(list);
    }
    
    [HttpGet]
    [Route("RefreshData")]
    public async Task<ActionResult<IEnumerable<Dictionary<string, object>>>> RefreshData()
    {   
        List<Dictionary<string, object>> list = await _dataFetcher.FetchPesticideDataAsync();
        _dataFetcher.InsertPesticideData(list);
        await _dataFetcher.FetchPesticideRegister();
        return Ok(new { Status = true,message = "Done!" });
    }

    [HttpGet]
    [Route("GetRegister")]
    public async Task<ActionResult<IEnumerable<Register>>> GetRegister()
    {
        await _dataFetcher.FetchPesticideRegister();
        return Ok(new { Status = true,message = "Done!" });
    }
}

