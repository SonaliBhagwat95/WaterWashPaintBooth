using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PaintBooth.Model
{
    public enum DoorSide
    {
        Left,
        Right
    }
    public class DoorDimensionsModel
    {
        public string doorType { get; set; }
        public string doorSubType { get; set; }
        public string sideDoorLocation { get; set; }
        public double doorWidth { get; set; }
        public double doorHeight { get; set; }
        public double xOffeset { get; set; }
        public double xOffesetForFrontDoor { get; set; }
        public double yOffeset { get; set; }
        //public string Side { get; set; }
        public DoorSide Side { get; set; }
        public string TypeOfHinges { get; set; }
        public double panelHeightforAboveCompEntryPanels { get; set; }
        public double panelWidthforAboveCompEntryPanels { get; set; }
        public double yOffsetForFrontDoors { get; set; }
        public double RemainingSpanceinDepth { get; set; }
        public double PHforAboveCompEntryPanels { get; set; }
        public double PWforAboveCompEntryPanels { get; set; }
        public double panelZOffsetAboveDoor { get; set; }

    }
}
