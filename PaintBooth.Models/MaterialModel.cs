using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PaintBooth.Models
{
    public class MaterialModel
    {
        public int MOCID { get; set; }
        public string MOCName { get; set; }
        public double Density { get; set; }
        public double Rate { get; set; }
        public int CreatedBy { get; set; }
        public DateTime? CreatedDate { get; set; }
        public int ModifiedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public bool IsActive { get; set; }
        // Add this property for BO Calculations
        public decimal Quantity { get; set; }
        public decimal Cost { get; set; }
        public int FID { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public double filterWeight { get; set; }
        public decimal AreaOfPaintArrestedFilter { get; set; }

        // Newly added field by Ajay 
        public string ItemCode { get; set; }   // e.g. "512030056"

    }
}
