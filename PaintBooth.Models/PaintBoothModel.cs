using System.ComponentModel.DataAnnotations;

namespace PaintBooth.Models
{
    public class PaintBoothModel
    {
        public int PaintBoothID { get; set; }
        public List<SettingItemViewModel> Settings { get; set; }
        public double ExtractionChamberHeight { get; set; }
        public double PanelHeightAboveFilterFrame { get; set; }
        public string BendType { get; set; }
        public int EnquiryId { get; set; }
        public string SalesNO { get; set; }
        public int? MotorCatalogID { get; set; }
        public bool chkServiceDoor { get; set; }
        public bool chkWatchDoor { get; set; }
        public double JobSize { get; set; }
        public string PanelPosition { get; set; }
        public string PanelsizeforD { get; set; }
        public double StandardPanelWidthForD { get; set; }
        public decimal RatedOutputHP { get; set; }
        public double HalfPanelswidthforD { get; set; }
        public string PanelsizeforW { get; set; }
        public double PanelHeightforW { get; set; }
        public double HalfPanelsHeightforW { get; set; }
        public int Lights { get; set; }
        public string PanelsizeforH { get; set; }
        public double PanelHeightforH { get; set; }
        public double HalfPanelsHeightforH { get; set; }
        public int NoOfBlowers { get; set; }
        public int CapacityofEachBlower { get; set; }
        public double PreviousPanelWidth { get; set; }
        public double PreviousPanelHeight { get; set; }
        public bool RightSideProcessed { get; set; }
        public bool LeftSideProcessed { get; set; }
        public string ServiceDoorLocation { get; set; }
        public string PanelTypes { get; set; }
        public double D1 { get; set; }
        public double D2 { get; set; }
        public string PanelTypesforW { get; set; }
        public string PanelTypesforH { get; set; }
        public double DoorHeight { get; set; }
        public double DoorWidth { get; set; }
        public double WatchglassHeight { get; set; }
        public double WatchglassWidth { get; set; }
        public double WatchGlassBottomDistance { get; set; }
        public double D3 { get; set; }
        public double W1 { get; set; }
        public double W2 { get; set; }
        public double W3 { get; set; }
        public double D { get; set; }

        public double H1 { get; set; }
        public double H2 { get; set; }

        public double W { get; set; }
        public double H { get; set; }
        public double Depth { get; set; }
        public double Width { get; set; }
        public double Height { get; set; }
        public double CrossSectionalArea { get; set; }
        public double VelocityofBlower { get; set; }
        public decimal SheetThickness { get; set; }
        public double CapacityofBlower { get; set; }
        public double VelocityofFilterFrame { get; set; }
        public double FilterArea { get; set; }
        public double CapacityofBlowerreadonly { get; set; }
        public double CapacityofBlowerRoundOf { get; set; }
        public double CapacityofBlowerAfterRoundOup { get; set; }
        public int StandardPanels { get; set; }
        public int RemainingPanels { get; set; }


        [Required(ErrorMessage = "*")]
        [Range(0.1, double.MaxValue, ErrorMessage = "Water Column must be greater than zero")]
        public double WaterColumn { get; set; }
        public double BlowerHpCalculation { get; set; }
        public string? WaterColumnRowsJson { get; set; }
        public int TotalPanels { get; set; }
        public int StandardPanelsByW { get; set; }
        public int RemainingPanelsByW { get; set; }
        public bool MakeItEqualByW { get; set; }
        public bool designStatus { get; set; }
        public double EqualPanelWidthByW { get; set; }
        public int TotalPanelsByW { get; set; }
        public double CapacityofBlowerinH { get; set; }
        public double StandardPanelsByH { get; set; }
        public int NoofPanels { get; set; }
        public double PanelWeight { get; set; }
        public int RemainingPanelsByH { get; set; }
        public bool MakeItEqualByH { get; set; }
        public double EqualPanelWidthByH { get; set; }
        public int TotalPanelsByH { get; set; }
        public double FrameWidth { get; set; }
        public double FrameHeight { get; set; }
        public string PaintBoothType { get; set; }
        public decimal standardbend1 { get; set; }
        public decimal standardbend2 { get; set; }
        public decimal PitchDistance { get; set; }
        public string SlotDimention { get; set; }
        public string Section { get; set; }
        public string BlowerOrientation { get; set; }
        public List<PaintBoothModel> DetailsList { get; set; } = new List<PaintBoothModel>();
        public int FilterHeight { get; set; }
        public double ExhaustDuctHeight { get; set; }
        public double ExhaustWidth { get; set; }
        [Required(ErrorMessage = "*")]
        public double ExhaustThickness { get; set; } = 2;
        public double PanelWidth { get; set; }
        public double PanelHeight { get; set; }
        public double PanelLength { get; set; }
        public string ExhaustDucting { get; set; }
        public string BendDucting { get; set; }
        public double DuctLength { get; set; }
        public double DuctWeight { get; set; }
        public double BendWeight { get; set; }
        public string Mountingtype { get; set; }
        public string Drivetype { get; set; }
        public string BlowerItemCode { get; set; }
        public IEnumerable<PressureDrop> pressureDropDetails { get; set; }
        public List<ExhaustDuctingModel> ExhaustDuctingList { get; set; }

        public string ExhaustDuctingListJson { get; set; } // Serialized JSON list

        public double CChannelHeight { get; set; }
        public string MotorTypes { get; set; }
        public string MotorItemCode { get; set; }
        public decimal HingedDoorWidth { get; set; }
        public decimal HingedDoorHeight { get; set; }
        public string DisplayPanelPosition { get; set; }
        public double BetweenCenterDistance { get; set; }
        public int PlenumHeight { get; set; }

        // Newly added fields  by Ajay
        public string ItemCode { get; set; }   // e.g. "512030056"
        public decimal Rate { get; set; }      // will be auto-filled
        public string BendDirection { get; set; }
        public string ItemCodeForStructuralM { get; set; }   // e.g. "512030056"
        public decimal RateForStructuralM { get; set; }      // will be auto-filled

    }

    public class PressureDrop
    {
        public int ItemNumber { get; set; }
        public string Description { get; set; }
        public decimal PressureDrop_mm { get; set; }
    }

    public class ExhaustDuctingModel
    {
        //public string BendType { get; set; }
        //public int? DuctLength { get; set; } // Nullable because not all bend types need a length
        //public decimal DuctWidth { get; set; }
        //public decimal DuctHeight { get; set; }
        //public decimal DuctThickness { get; set; }
        //public decimal DuctWeight { get; set; }
        //// NEW — needed for item rate auto-fetch Code added by Ajay
        //public string ItemCode { get; set; }
        //public decimal Rate { get; set; }
        public string? BendType { get; set; }
        public string? BendDirection { get; set; }   // NEW — Up/Down/Left/Right for bends
        public int? DuctLength { get; set; }          // nullable — only ducts have a length
        public decimal DuctWidth { get; set; }
        public decimal DuctHeight { get; set; }
        public decimal DuctThickness { get; set; }
        public decimal DuctNetWeight { get; set; }    // NEW — replaces DuctWeight
        public decimal DuctGrossWeight { get; set; }  // NEW — net + wastage

        // item rate auto-fetch — added by Ajay
        public string ItemCode { get; set; }
        public decimal Rate { get; set; }
    }
}
