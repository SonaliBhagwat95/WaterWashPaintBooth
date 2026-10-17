using devDept.Eyeshot;
using devDept.Eyeshot.Translators;
using devDept.Geometry;
using Microsoft.Extensions.Configuration;
using PaintBooth.Models;
using static PaintBooth.Business.SheetProfile;

namespace PaintBooth.Business.Water_Wash_Parts.Water_Trough
{
    public class Plate3
    {
        private readonly IConfiguration _configuration;
        public Plate3(IConfiguration configuration) { _configuration = configuration; }

        public DesignDocument DrawPlate3(PaintBoothModel model, double length = 3992)
        {
            DesignDocument design = new DesignDocument();
            design.Units = linearUnitsType.Millimeters;

            // ---- Inputs (mm) - outside dimensions ----
            double T = 2;        // 2 THK SS304
            double L = length;   // along Y (3992)
            double Lip = 15;       // left lip height
            double Bottom = 222;      // overall width
            double Side = 200;      // right side height (to bend)
            double TopLip = 45;       // top lip length
            double BendDeg = 135;      // angle between side and top lip

            // ---- Centreline (X, Z) - origin = bottom-left outer corner ----
            double h = T / 2;
            double a = (180 - BendDeg) * Math.PI / 180.0;         // 45° : lip leans inward

            var c = new List<(double X, double Z)>
            {
                (h,          Lip),                                // top of left lip
                (h,          h),                                  // bottom-left bend
                (Bottom - h, h),                                  // bottom-right bend
                (Bottom - h, Side),                               // top of side (bend)
            };
            c.Add((c[3].X - TopLip * Math.Sin(a), c[3].Z + TopLip * Math.Cos(a)));   // top lip, up and in

            Thicken(c, h, out var left, out var right);
            AddPiece(design, left, right, 0, c.Count - 1, 0, L);

            //// ---- Export DWG ----
            //var path = _configuration["FolderPathConfig:DrawingPath"].ToString();
            //if (!Directory.Exists(path + "/PaintBooth drawing"))
            //    Directory.CreateDirectory(path + "/PaintBooth drawing");
            //string dwgFilePath = $"{path}/PaintBooth drawing/Plate3 {DateTime.Now:hh - mm}.dwg";
            //new WriteAutodesk(new WriteAutodeskParams(design), dwgFilePath).DoWork();

            return design;
        }
    }
}