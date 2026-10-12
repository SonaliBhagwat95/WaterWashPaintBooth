using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PaintBooth.Database
{
    public class PanelDetails
    {
        [Key]
        public int PanelID { get; set; }
        [ForeignKey("EnquiryId")]
        public int EnquiryId { get; set; }

        public string PanelPosition { get; set; }
        public double StandardPanelWidth { get; set; }
        //public double EqualPanelWidth { get; set; }
        public double StandardPanelDepth { get; set; }
        //public double EqualPanelDepth { get; set; }
        //public double RemainingPanelDepth { get; set; }
        public double StandardPanelHeight { get; set; }
        //public double EqualPanelHeight { get; set; }
        //public double RemainingPanelHeight { get; set; }
        public string SalesNo { get; set; }
        public string SlotDimention { get; set; }
        public double FrameWidth { get; set; }
        public double FrameHeight { get; set; }
        public int NoOfPanels { get; set; }
        public decimal StandardBend1 { get; set; }
        public decimal StandardBend2 { get; set; }
        public decimal SheetThickness { get; set; }
        public decimal PitchDistance { get; set; }
        public double PanelWeight { get; set; }

        public decimal CostOfFrame { get; set; }
        public decimal CostOfPanels {get;set;}
        public int? CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public int? ModifiedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public bool IsActive { get; set; }
        public bool CostingStatus { get; set; }
    }
}
