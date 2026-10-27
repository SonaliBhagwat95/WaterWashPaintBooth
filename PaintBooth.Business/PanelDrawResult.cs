using devDept.Eyeshot;

namespace PaintBooth.Business
{
    public class PanelDrawResult
    {
        public DesignDocument drawing { get; set; }
        public double Weight { get; set; }
        //public bool IsSmallPanel { get; set; }
        public string lstpath { get; set; }
        public int NoOfTransitionPieces { get; set; }
        public double DuctCutZ { get; set; }      // centre height of duct where it passes the wall
        public double DuctCutSize { get; set; }   // wall cut-out size (square)
        public double DuctLegX { get; set; }
    }


}
