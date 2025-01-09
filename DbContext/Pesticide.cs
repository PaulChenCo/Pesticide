using System.ComponentModel.DataAnnotations;

namespace PesticideContext;

public class Pesticide{
    [Key]
    public Guid Id { get; set; }                        //UID, PK
    public string? Permit { get; set; }                 //許可證字
    public string? PermitNumber { get; set; }           //許可證號
    public string? ChineseName { get; set; }            //中文名稱
    public string? PesticideCode { get; set; }          //農藥代號
    public string? EnName { get; set; }                 //英文名稱
    public string? BrandName { get; set; }              //廠牌名稱
    public string? ChemicalComposition { get; set; }    //化學成分
    public string? ForeignMaker { get; set; }           //國外原製造廠商
    public string? formCode { get; set; }               //劑型
    public string? contents { get; set; }               //含量
    public string? ExpireDate { get; set; }             //有效期限
    public string? Vendor { get; set; }                 //廠商名稱
    public string? FRAC { get; set; }                   //殺菌劑抗藥性
    public string? HRAC { get; set; }                   //除草劑抗藥性
    public string? IRAC { get; set; }                   //殺蟲劑抗藥性
    public string? RevocationType { get; set; }         //撤銷類別
    public string? RevocationDate { get; set; }         //撤銷日期
    public string? ScopeOfUse { get; set; }             //農藥使用範圍
    public string? GetFile { get; set; }                //許可證標示
    public string? GetBrcodeXML { get; set; }           //農藥條碼資料
    public string? GetGropty { get; set; }              //混合劑百分率資料
}