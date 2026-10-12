using System.ComponentModel.DataAnnotations;

namespace PaintBooth.Database
{
    public class EnquiryMaster
    {
        [Key]
        public int EnquiryID { get; set; }
        public int CustomerID { get; set; }
        public int? CreatedBy { get; set; }
        public int ComponentID { get; set; }
        public DateTime EnquiryDate { get; set; }
        public string SalesNO { get; set; }
        public string DraftType { get; set; }
        public string SubTypeOfDraftType { get; set; }
        public DateTime? CreatedDate { get; set; }
        public int ModifiedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public bool IsActive { get; set; }
        public string PlenumHeight { get; set; }
        public decimal TotalCostOfPanels { get; set; }
        public decimal TotalCostOfFrames { get; set; }
        public decimal TotalCostOfRM { get; set; }
        public string SideLocationDoorFAS { get; set; }
        public decimal MaintainanceDoorWidth { get; set; }
        public decimal MaintainanceDoorHeight { get; set; }
        public bool IsMaintenanceDoorAdded { get; set; }
        public string FilterTypes { get; set; }
        public int? SalesEngineerId { get; set; }
      
    }

}
