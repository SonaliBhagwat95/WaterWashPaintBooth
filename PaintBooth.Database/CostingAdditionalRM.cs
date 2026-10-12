using System.ComponentModel.DataAnnotations;

namespace PaintBooth.Database
{
    public class CostingAdditionalRM
    {
        [Key]
        public int Id { get; set; }
        public int EnquiryId { get; set; }
        public string Description { get; set; }
        public decimal Qty { get; set; }
        public decimal UnitWeight { get; set; }
        public decimal TotalWeight { get; set; }
        public decimal Rate { get; set; }
        public decimal Amount { get; set; }
        public DateTime CreatedAt { get; set; }

        public string? ItemCode { get; set; }
    }
}
