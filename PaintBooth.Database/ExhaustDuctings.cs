using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace PaintBooth.Database
{
    public class ExhaustDuctings
    {
        [Key]
        public int DuctId { get; set; }
        public string? BendType { get; set;}
        public string? BendDirection { get; set; }
        public decimal DuctWidth { get; set; }
        public decimal DuctHeight { get; set; }
        public decimal DuctThickness { get; set; }
        public string SalesNo { get; set; }
        public int? DuctLength { get; set; }
        public decimal? DuctNetWeight { get; set; }
        public decimal? DuctGrossWeight { get; set; }
        public double? WallDistance { get; set; }
        public double? DuctHeightFromFFL { get; set; }
        [ForeignKey("EnquiryID")]
        public int EnquiryID { get; set; }
        public int? CreatedBy { get; set; }
        public DateTime? CreatedDate { get; set; }
        public int ModifiedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public bool IsActive { get; set; }
    }
}
