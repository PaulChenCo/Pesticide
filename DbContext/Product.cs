using System.ComponentModel.DataAnnotations;

namespace PesticideContext;
    public class Product{
        [Key]
        public string? Barcode { get; set; }
        public string? LicType { get; set; }
        public string? LicNo { get; set; }
        public string? Seq { get; set; }
        public string? Spec { get; set; }
        public string? Unit { get; set; }
        public string? Volume { get; set; }
        public string? UpdateDT { get; set; }
    }