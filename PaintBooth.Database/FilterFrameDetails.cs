using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PaintBooth.Database
{
    public class FilterFrameDetails
    {
        [Key]
        public int FID { get; set; }
        public decimal FrameWidth { get; set; }
        public decimal FrameHeight { get; set; }
        public int Quantity { get; set; }
        [ForeignKey("EnquiryID")]
        public int EnquiryID { get; set; }
        public string SalesNO { get; set; }
        public bool IsActive { get; set; }
        public decimal FrameWeight { get; set; }
        
    }
}
