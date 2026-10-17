using devDept.Eyeshot;
using devDept.Eyeshot.Entities;
using devDept.Eyeshot.Translators;
using devDept.Geometry;
using Microsoft.Extensions.Configuration;
using PaintBooth.Models;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;

namespace PaintBooth.Business.Water_Wash_Parts.Water_Trough
{
    public class PipeWithFlange
    {
        private readonly IConfiguration _configuration;
        public PipeWithFlange(IConfiguration configuration) { _configuration = configuration; }

        // Vertical 2" pipe hanging down from Z = 0, flange at the bottom. Origin = pipe axis at top.
        public DesignDocument DrawPipeWithFlange(PaintBoothModel model, double length = 300)
        {
            DesignDocument design = new DesignDocument();
            design.Units = linearUnitsType.Millimeters;

            // ---- Inputs (mm) ----
            double L = length;   // 300 LG overall (top of pipe to underside of flange)
            double PipeOD = 60.3;     // 2" NB
            double PipeW = 2.77;     // Sch 10S                                   (CHECK)
            double FlOD = 152.4;    // 2" Class 150 flange                       (CHECK)
            double FlT = 19;       // flange thickness                          (CHECK)
            double PCD = 120.7;    // bolt circle                               (CHECK)
            int NBolt = 4;
            double BoltD = 19;       // bolt hole Ø                               (CHECK)

            // ---- pipe: ring in XY, extruded down ----
            Point3D c0 = Point3D.Origin;
            var ring = new devDept.Eyeshot.Entities.Region(
                new Circle(Plane.XY, c0, PipeOD / 2),
                new Circle(Plane.XY, c0, PipeOD / 2 - PipeW));
            design.Entities.Add(ring.ExtrudeAsBrep(new Vector3D(0, 0, -(L - FlT))), Color.Silver);

            // ---- flange at bottom: disc with bore and bolt holes ----
            double zF = -(L - FlT);
            Point3D cF = new Point3D(0, 0, zF);
            var fl = new List<ICurve>
            {
                new Circle(Plane.XY, cF, FlOD / 2),
                new Circle(Plane.XY, cF, PipeOD / 2 - PipeW)
            };
            for (int i = 0; i < NBolt; i++)
            {
                double a = Math.PI / NBolt + 2 * Math.PI * i / NBolt;     // straddle centrelines
                fl.Add(new Circle(Plane.XY, new Point3D(PCD / 2 * Math.Cos(a), PCD / 2 * Math.Sin(a), zF), BoltD / 2));
            }
            var flange = new devDept.Eyeshot.Entities.Region(fl.ToArray());
            design.Entities.Add(flange.ExtrudeAsBrep(new Vector3D(0, 0, -FlT)), Color.Silver);

            //// ---- Export DWG ----
            //var path = _configuration["FolderPathConfig:DrawingPath"].ToString();
            //if (!Directory.Exists(path + "/PaintBooth drawing"))
            //    Directory.CreateDirectory(path + "/PaintBooth drawing");
            //string dwgFilePath = $"{path}/PaintBooth drawing/PipeWithFlange {DateTime.Now:hh - mm}.dwg";
            //new WriteAutodesk(new WriteAutodeskParams(design), dwgFilePath).DoWork();

            return design;
        }
    }
}