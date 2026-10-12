using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PaintBooth.Models
{
    public class BOItemRateDto
    {
        public string ItemType { get; set; }
        public int RowIndex { get; set; }
        public decimal Rate { get; set; }
        public string? ItemCode { get; set; }   // ← NEW
        public decimal Quantity { get; set; }
        public decimal Cost { get; set; }
    }
}
