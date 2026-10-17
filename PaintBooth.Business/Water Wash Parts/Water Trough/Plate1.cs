using devDept.Eyeshot;
using devDept.Eyeshot.Translators;
using devDept.Geometry;
using Microsoft.Extensions.Configuration;
using PaintBooth.Models;
using System;
using System.Collections.Generic;
using System.IO;
using static PaintBooth.Business.SheetProfile;

namespace PaintBooth.Business.Water_Wash_Parts.Water_Trough
{
    public class Plate1
    {
        private readonly IConfiguration _configuration;
        public Plate1(IConfiguration configuration) { _configuration = configuration; }

        public DesignDocument DrawPlate1(PaintBoothModel model, double length = 3992)
        {
            DesignDocument design = new DesignDocument();
            design.Units = linearUnitsType.Millimeters;

            // ---- Inputs (mm) - outside dimensions ----
            double T = 2;        // 2 THK SS304
            double L = length;   // along Y (3992)
            double H = 300;      // overall height
            double Lip = 25;       // top lip

            // ---- Centreline (X, Z) - origin = bottom of vertical leg, outer face at X = 0 ----
            double h = T / 2;
            var c = new List<(double X, double Z)>
            {
                (h,   0),             // bottom of vertical leg
                (h,   H - h),         // top bend
                (Lip, H - h)          // end of top lip
            };

            Thicken(c, h, out var left, out var right);
            AddPiece(design, left, right, 0, c.Count - 1, 0, L);

            //// ---- Export DWG ----
            //var path = _configuration["FolderPathConfig:DrawingPath"].ToString();
            //if (!Directory.Exists(path + "/PaintBooth drawing"))
            //    Directory.CreateDirectory(path + "/PaintBooth drawing");
            //string dwgFilePath = $"{path}/PaintBooth drawing/Plate1 {DateTime.Now:hh - mm}.dwg";
            //new WriteAutodesk(new WriteAutodeskParams(design), dwgFilePath).DoWork();

            return design;
        }
    }
}