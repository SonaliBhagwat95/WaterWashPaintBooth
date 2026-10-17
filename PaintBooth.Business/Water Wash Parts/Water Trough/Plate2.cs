using devDept.Eyeshot;
using devDept.Eyeshot.Translators;
using devDept.Geometry;
using Microsoft.Extensions.Configuration;
using PaintBooth.Models;
using static PaintBooth.Business.SheetProfile;

namespace PaintBooth.Business.Water_Wash_Parts.Water_Trough
{
    public class Plate2
    {
        private readonly IConfiguration _configuration;
        public Plate2(IConfiguration configuration) { _configuration = configuration; }

        public DesignDocument DrawPlate2(PaintBoothModel model, double length = 3992)
        {
            DesignDocument design = new DesignDocument();
            design.Units = linearUnitsType.Millimeters;

            // ---- Inputs (mm) - outside dimensions ----
            double T = 2;        // 2 THK SS304
            double L = length;   // along Y (3992)
            double Web = 248;      // overall height
            double Lip = 15;       // top and bottom lips

            // ---- Centreline (X, Z) - origin = bottom-left outer corner, lips point +X ----
            double h = T / 2;
            var c = new List<(double X, double Z)>
            {
                (Lip,  h),            // end of bottom lip
                (h,    h),            // bottom bend
                (h,    Web - h),      // top bend
                (Lip,  Web - h)       // end of top lip
            };

            Thicken(c, h, out var left, out var right);
            AddPiece(design, left, right, 0, c.Count - 1, 0, L);

            //// ---- Export DWG ----
            //var path = _configuration["FolderPathConfig:DrawingPath"].ToString();
            //if (!Directory.Exists(path + "/PaintBooth drawing"))
            //    Directory.CreateDirectory(path + "/PaintBooth drawing");
            //string dwgFilePath = $"{path}/PaintBooth drawing/Plate2 {DateTime.Now:hh - mm}.dwg";
            //new WriteAutodesk(new WriteAutodeskParams(design), dwgFilePath).DoWork();

            return design;
        }
    }
}