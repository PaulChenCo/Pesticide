using System.ComponentModel.DataAnnotations;

namespace PesticideContext;

public class Register{
    [Key]
    public required string RegtID { get; set; }     //許可證字號
    public string? Permit { get; set; }             //許可證字
}