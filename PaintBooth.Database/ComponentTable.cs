using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PaintBooth.Database
{
    public class ComponentTable
    {
        [Key]
        public int ComponentID { get; set; }
        [Required(ErrorMessage = "*")]
        public string? Category { get; set; }
        public string? Component { get; set; }
        public string? Shape { get; set; }
        public string? Orientation { get; set; }
        // public decimal Diameter { get; set; }
        [NotMapped]
        public double? Diameter { get; set; }

        [NotMapped]
        public double? CircularHeightSize { get; set; }
        public double? Length { get; set; }
        public double? WidthSize { get; set; }
        public double? HeightSize { get; set; }
        public double? Weight { get; set; }
        public decimal? CarringCapacity { get; set; }

        public string? MaterialofConstruction { get; set; }
        public string? OverheadConveyorSubTypes { get; set; }
        public string? ConveyorNumber { get; set; }

        public double? SurfaceArea { get; set; }

        public double? WallThickness { get; set; }

        public int? ProductionRequirement { get; set; }

        public int? Workingdays { get; set; }

        public int? NumberofShifts { get; set; }
        [DisplayName("Choose File")]
        public string? Image_Path { get; set; }
        public double? EffectiveWorking { get; set; }
        public bool IsActive { get; set; }
        public int? CreatedBy { get; set; }
        public DateTime? CreatedDate { get; set; }
        public int? ModifiedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }

        public string? Conveyor { get; set; }
        public double? Pitch { get; set; }
        public double? Speed { get; set; }
        public string? LoadingUnloading { get; set; }
        public string? ComponentHandling { get; set; }
        public int? NoOfColors { get; set; }
        public int? NoOfCoats { get; set; }
        public double? Viscosity { get; set; }
        public double? Paint { get; set; }
        public double? Powder { get; set; }
        public string? TypeOfPaint { get; set; }
        public string? TypeOfPowder { get; set; }
        public double? DFT { get; set; }
        public double? SpecificHeat { get; set; }
        public string? ConsumptionPerDay { get; set; }
        public string? ExtractionChamberHeight { get; set; }
        public string? ComponentEntry { get; set; }
        public string? DoorType { get; set; }
        public string? DoorSubType { get; set; }
        public string? SideDoorLOcation { get; set; }
        public string? TypeOfHingedDoor { get; set; }
        public decimal? DoorWidth { get; set; }
        public decimal? DoorHeight { get; set; }
        public string? TypeOfHinges { get; set; }
        public bool? chkWatchDoor { get; set; }
        public double? WatchGlassBottomDistance { get; set; }
        public double? WatchglassHeight { get; set; }
        public double? WatchglassWidth { get; set; }
        public string? WashingChamberTank { get; set; }
        public string? MOC { get; set; }
    }
}
