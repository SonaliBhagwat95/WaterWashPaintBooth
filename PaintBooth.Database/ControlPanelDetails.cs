using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PaintBooth.Database
{
    public class ControlPanelDetails
    {
        [Key]
        public int ControlPanelID { get; set; }
        public int EnquiryID { get; set; }

        public bool IsRequired { get; set; } = true;
        public string? PanelType { get; set; }
        public string? Enclosure { get; set; }
        public string? IPRating { get; set; }
        public string? StarterType { get; set; }

        public string? Side { get; set; }
        public decimal? OffsetAlong { get; set; }
        public decimal? DistanceFromBooth { get; set; }
        public decimal? MountHeight { get; set; }

        public decimal? Width { get; set; }
        public decimal? Height { get; set; }
        public decimal? Depth { get; set; }
        public string? SheetThickness { get; set; }
        public string? PanelShade { get; set; }

        public string? IncomerType { get; set; }
        public decimal? IncomerRating { get; set; }
        public string? ControlVoltage { get; set; }
        public decimal? ConnectedLoadKW { get; set; }

        public bool Ammeter { get; set; }
        public bool Voltmeter { get; set; }
        public bool IndicationLamps { get; set; }
        public bool HourMeter { get; set; }
        public bool EmergencyStop { get; set; }
        public bool BlowerGunInterlock { get; set; }
        public bool DPSwitch { get; set; }
        public bool DoorLimitSwitch { get; set; }
        public bool FireAlarmInterface { get; set; }
        public bool PLCWithHMI { get; set; }

        public string? TrayWidth { get; set; }
        public string? TrayType { get; set; }
        public decimal? TrayLength { get; set; }

        public string? ItemCode { get; set; }

        public bool IsActive { get; set; } = true;
        public int? CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; } = DateTime.Now;
        public DateTime? ModifiedDate { get; set; }
    }
}
