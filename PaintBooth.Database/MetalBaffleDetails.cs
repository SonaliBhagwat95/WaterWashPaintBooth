using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PaintBooth.Database
{
    public class MetalBaffleDetails
    {
        [Key]
        public int MID { get; set; }                // Primary Key
        public int EnquiryID { get; set; }          // Foreign Key to EnquiryMasters
        public decimal BaffleWidth { get; set; }    // Width of the baffle
        public decimal BaffleHeight { get; set; }   // Height of the baffle
        public int Quantity { get; set; }           // Quantity of baffles
        public string SalesNo { get; set; }         // Sales number (nullable)
        public bool IsActive { get; set; }         // Soft delete flag
        public decimal? BaffleWeight { get; set; }  // Weight of the baffle (nullable)
    }
}
