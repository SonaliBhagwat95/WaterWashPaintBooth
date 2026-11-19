namespace PaintBooth.Models
{
    public class ControlPanelModel
    {
        public bool IsRequired { get; set; } = true;
        public string PanelType { get; set; } = "Wall";          // Wall / Floor
        public string Enclosure { get; set; } = "NFLP";          // NFLP / FLP
        public string IPRating { get; set; } = "IP54";

        public string Side { get; set; } = "Right";              // Left / Right / Front / Rear
        public double OffsetAlong { get; set; } = 500;
        public double DistanceFromBooth { get; set; } = 1000;
        public double MountHeight { get; set; } = 800;

        public double Width { get; set; } = 600;
        public double Height { get; set; } = 800;
        public double Depth { get; set; } = 300;
        public string SheetThickness { get; set; } = "1.6";
        public string PanelShade { get; set; } = "RAL 7032";

        public string IncomerType { get; set; } = "MCB";         // MCB / MCCB
        public double IncomerRating { get; set; }                 // A
        public string ControlVoltage { get; set; } = "230V AC";
        public string StarterType { get; set; } = "Star-Delta";  // DOL / Star-Delta / VFD / Soft Starter

        public bool Ammeter { get; set; } = true;
        public bool Voltmeter { get; set; } = true;
        public bool IndicationLamps { get; set; } = true;
        public bool HourMeter { get; set; }
        public bool EmergencyStop { get; set; } = true;
        public bool BlowerGunInterlock { get; set; } = true;
        public bool DPSwitch { get; set; }
        public bool DoorLimitSwitch { get; set; }
        public bool FireAlarmInterface { get; set; }
        public bool PLCWithHMI { get; set; }
        public string TrayWidth { get; set; } = "100";
        public string TrayType { get; set; } = "Perforated GI";
        public double TrayLength { get; set; }                   // auto
        public double ConnectedLoadKW { get; set; }              // auto
    }
}
