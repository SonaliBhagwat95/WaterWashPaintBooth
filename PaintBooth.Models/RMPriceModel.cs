using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PaintBooth.Models
{
    public class RMPriceModel
    {
        public int EnquiryId { get; set; }
        public decimal TotalCostOfPanels { get; set; }
        public decimal TotalCostOfFrames { get; set; }
        public decimal TotalCostOfRM { get; set; }
       // public decimal EnquiryId { get; set; }
    }
}
