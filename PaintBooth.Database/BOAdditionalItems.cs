using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PaintBooth.Database
{
    public class BOAdditionalItems
    {
        public int Id { get; set; }
        public int EnquiryId { get; set; }
        public string Description { get; set; }
        public decimal Qty { get; set; }
        public decimal UnitWeight { get; set; }
        public decimal TotalWeight { get; set; }
        public decimal Rate { get; set; }

        public decimal Amount { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        // Added By Ajay
        public string? ItemCode { get; set; }   // ← NEW
    }
}
