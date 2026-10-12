using devDept.Eyeshot;
using devDept.Eyeshot.Translators;
using devDept.Geometry;
using Microsoft.Extensions.Configuration;
using PaintBooth.Models;
using System;
using System.Collections.Generic;
using System.IO;
using static PaintBooth.Business.SheetProfile;

namespace PaintBooth.Business
{
    public class PipeSupportBracket
    {
        private readonly IConfiguration _configuration;

        public PipeSupportBracket(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public DesignDocument DrawPipeSupportBracket(PaintBoothModel model, double length = 300)
        {
            DesignDocument design = new DesignDocument();
            design.Units = linearUnitsType.Millimeters;

            // ---- Inputs (mm) ----
            double T = 3;       // thickness
            double LegV = 70;      // vertical leg height
            double BendDeg = 126;     // angle between vertical (up) and angled leg
            double LegA = 70;      // angled leg length                    (CHECK)
            double Length = length;  // along Y (same as pipe support top bracket 300)  (CHECK)

            // ---- Centreline (X, Z) - origin = bend point ----
            //double legDeg = 90 - BendDeg;                 // -36° : down-right, same slope as pipe
            double legDeg = BendDeg - 90;
            var c = new List<(double X, double Z)>
            {
                (0, -LegV),                               // bottom of vertical leg
                (0, 0),                                   // bend
                Add((0, 0), LegA, legDeg)                 // end of angled leg
            };

            Thicken(c, T / 2.0, out var left, out var right);
            AddPiece(design, left, right, 0, c.Count - 1, 0, Length);

            // ---- Export DWG ----
            var path = _configuration["FolderPathConfig:DrawingPath"].ToString();

            if (!Directory.Exists(path + "/PaintBooth drawing"))
                Directory.CreateDirectory(path + "/PaintBooth drawing");

            string dwgFilePath = $"{path}/PaintBooth drawing/PipeSupportBracket {DateTime.Now:hh - mm}.dwg";

            WriteAutodeskParams auto = new WriteAutodeskParams(design);
            WriteAutodesk dwgg1 = new WriteAutodesk(auto, dwgFilePath);
            dwgg1.DoWork();

            return design;
        }
    }
}