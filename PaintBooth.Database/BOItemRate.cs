using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PaintBooth.Database
{
    public class BOItemRate
    {
        public int Id { get; set; }
        public string EnquiryCode { get; set; }
        public string ItemType { get; set; }
        public int RowIndex { get; set; }
        public decimal Rate { get; set; }
        public decimal Quantity { get; set; }
        public decimal Cost { get; set; }
        public int CreatedBy { get; set; }
        public int ModifiedBy { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime ModifiedDate { get; set; }

        // Added By Ajay 
        public string? ItemCode { get; set; }   // NEW
    }
}
