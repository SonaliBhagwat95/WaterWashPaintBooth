using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PaintBooth.Models
{
    public class EnquiryModel
    {
        public int EnquiryID { get; set; }
        public DateTime EnquiryDate { get; set; }
        public string Shape { get; set; }
        public decimal CarringCapacity { get; set; }
        public string DraftType { get; set; }
        public string SubTypeOfDraft { get; set; }
        public string ConveyorNumber { get; set; }
        public string OverheadConveyorSubTypes { get; set; }
        public string Orientation { get; set; }
        public int EnquiryId { get; set; }
        public bool settingStatus { get; set; }
        public string MotorType { get; set; }
        public string LightType { get; set; }
        public string SalesNO { get; set; }
        public decimal Diameter { get; set; }
        public int CustomerID { get; set; }
        public int ComponentID { get; set; }
        public string ComponentHandling { get; set; }
        public int NoOfColors { get; set; }
        public int NoOfCoats { get; set; }
        public double SpecificHeat { get; set; }
        public string CompanyName { get; set; }
        public string CustomerAddress { get; set; }
        public string Designation { get; set; }
        public int Category { get; set; }
        public string Component { get; set; }
        public double LengthSize { get; set; }
        public double WidthSize { get; set; }
        public double HeightSize { get; set; }
        public double CircularHeightSize { get; set; }
        public double Weight { get; set; }
        public int QtyperAssembly { get; set; }
        public string MaterialofConstruction { get; set; }
        public string PaintBoothType { get; set; }
        public double SurfaceArea { get; set; }
        public double WallThickness { get; set; }
        public string Conveyor { get; set; }
        public double Pitch { get; set; }
        public double Speed { get; set; }
        public string LoadingUnloading { get; set; }
        public int ProductionRequirement { get; set; }
        public double Viscosity { get; set; }
        public double Paint { get; set; }
        public double Powder { get; set; }
        public double DFT { get; set; }
        public int Workingdays { get; set; }
        public int NumberofShifts { get; set; }
        public double EffectiveWorking { get; set; }
        public string Image_Path { get; set; }
        public IFormFile imageFile { get; set; }
        public int CreatedBy { get; set; }
        public DateTime? CreatedDate { get; set; }
        public int StateId { get; set; }
        public int CityId { get; set; }
        public string State { get; set; }
        public int Pin { get; set; }
        public string PAN { get; set; }
        public string Contactperson { get; set; }
        public string MobileNo { get; set; }
        public string EmailId { get; set; }
        public string Consumption { get; set; }
        public string Description { get; set; }
        public int MotorID { get; set; }
        public string HeightOfPlenum { get; set; }
        public string ExtractionChamberHeight { get; set; }
        public string ComponentEntry { get; set; }
        public string DoorType { get; set; }
        public string SideDoorLOcation { get; set; }
        public string TypeOfOperationForManual { get; set; }
        public string TypeOfOperationForMotorised { get; set; }
        public string TypeOfPaint { get; set; }
        public string TypeOfPowder { get; set; }
        public string TypeOfHingedDoor { get; set; }
        public decimal SplitDoorWidth { get; set; }
        public decimal SplitDoorHeight { get; set; }
        public string TypeOfHinges { get; set; }

        //Maintenance door 
        public string SideLocationDoorFAS { get; set; }
        public double MaintainanceDoorWidth { get; set; }
        public double MaintainanceDoorHeight { get; set; }
        public bool chkmaintenanceDoor { get; set; }
    }
}
