using devDept.Eyeshot;
using devDept.Eyeshot.Entities;
using devDept.Eyeshot.Translators;
using devDept.Geometry;
using Microsoft.Extensions.Configuration;
using PaintBooth.Models;
using System;
using System.Drawing;
using System.IO;

namespace PaintBooth.Business.Water_Wash_Parts
{
    public class FlowPipe
    {
        private readonly IConfiguration _configuration;
        public FlowPipe(IConfiguration configuration) { _configuration = configuration; }

        // 20NB plain pipe, 3/4" BSP thread at top end.
        // Origin = axis at bottom (plain) end, pipe runs up +Z.
        // Flow Pipe: DrawFlowPipe(model, "FlowPipe", 330, 30)
        public DesignDocument DrawFlowPipe(PaintBoothModel model, string name, double L, double threadL)
        {
            DesignDocument design = new DesignDocument();
            design.Units = linearUnitsType.Millimeters;

            // ---- 20NB, Sch 10S ----                                         (CHECK wall)
            double OD = 26.7, W = 2.11;

            AddTube(design, 0, L - threadL, OD, W, Color.Silver);        // plain length 300
            AddTube(design, L - threadL, L, OD, W, Color.DimGray);       // 3/4" BSP thread zone 30

            // ================= Export DWG =================
            var path = _configuration["FolderPathConfig:DrawingPath"].ToString();
            if (!Directory.Exists(path + "/PaintBooth drawing"))
                Directory.CreateDirectory(path + "/PaintBooth drawing");
            string dwgFilePath = $"{path}/PaintBooth drawing/{name} {DateTime.Now:hh - mm}.dwg";
            new WriteAutodesk(new WriteAutodeskParams(design), dwgFilePath).DoWork();

            return design;
        }

        // tube from zFrom to zTo (either direction)
        private static void AddTube(DesignDocument design, double zFrom, double zTo,
                                    double od, double wall, Color color)
        {
            Point3D c = new Point3D(0, 0, zFrom);
            var ring = new devDept.Eyeshot.Entities.Region(
                new Circle(Plane.XY, c, od / 2), new Circle(Plane.XY, c, od / 2 - wall));
            design.Entities.Add(ring.ExtrudeAsBrep(new Vector3D(0, 0, zTo - zFrom)), color);
        }
    }
}