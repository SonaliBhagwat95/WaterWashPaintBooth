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

namespace PaintBooth.Business.Water_Wash_Parts
{
    public class DistributionPipe
    {
        private readonly IConfiguration _configuration;
        public DistributionPipe(IConfiguration configuration) { _configuration = configuration; }

        /// <summary>Z of socket bottom relative to header axis (set by DrawDistributionPipe).</summary>
        public double SocketBottom { get; private set; }

        // Header along +Y, slip-on flange both ends, 3/4" BSP sockets underneath (-Z).
        // Origin = axis at outer face of first flange.
        // Distribution Pipe: DrawDistributionPipe(model, "DistributionPipe", 3800, { 251, 1051, 1851, 2651, 3451 })
        public DesignDocument DrawDistributionPipe(PaintBoothModel model, string name, double L, double[] socketY)
        {
            DesignDocument design = new DesignDocument();
            design.Units = linearUnitsType.Millimeters;

            // ---- Header pipe, 65NB Sch 10S ----                              (CHECK size on drawing)
            double OD = 73.0, W = 3.05;

            // ---- BS10 Table D slip-on flange, 65NB ----                      (CHECK)
            double FlOD = 165.1, FlPCD = 127.0, FlT = 12.7, BoltHole = 17.5;
            int NBolts = 4;

            // ---- 3/4" BSP socket (half coupling) ----                        (CHECK)
            double sockOD = 35, sockID = 26.4, sockH = 25, sockIn = 3;          // sockIn = sunk into pipe wall
            SocketBottom = -OD / 2 + sockIn - sockH;

            // ================= 1. PIPE =================
            TubeY(design, 0, L, OD, W);

            // ================= 2. FLANGES - both ends =================
            FlangeY(design, 0, +FlT, OD, FlOD, FlPCD, BoltHole, NBolts);      // first end
            FlangeY(design, L, -FlT, OD, FlOD, FlPCD, BoltHole, NBolts);      // far end

            // ================= 3. SOCKETS (underside) =================
            foreach (double y in socketY)
            {
                Point3D c = new Point3D(0, y, -OD / 2 + sockIn);
                var ring = new devDept.Eyeshot.Entities.Region(
                    new Circle(Plane.XY, c, sockOD / 2), new Circle(Plane.XY, c, sockID / 2));
                design.Entities.Add(ring.ExtrudeAsBrep(new Vector3D(0, 0, -sockH)), Color.DimGray);
            }

            // ================= Export DWG =================
            var path = _configuration["FolderPathConfig:DrawingPath"].ToString();
            if (!Directory.Exists(path + "/PaintBooth drawing"))
                Directory.CreateDirectory(path + "/PaintBooth drawing");
            string dwgFilePath = $"{path}/PaintBooth drawing/{name} {DateTime.Now:hh - mm}.dwg";
            new WriteAutodesk(new WriteAutodeskParams(design), dwgFilePath).DoWork();

            return design;
        }

        // tube along Y from y0 to y1
        private static void TubeY(DesignDocument design, double y0, double y1, double od, double wall)
        {
            var pl = new Plane(new Point3D(0, y0, 0), Vector3D.AxisY);
            var ring = new devDept.Eyeshot.Entities.Region(
                new Circle(pl, od / 2), new Circle(pl, od / 2 - wall));
            design.Entities.Add(ring.ExtrudeAsBrep(new Vector3D(0, y1 - y0, 0)), Color.Silver);
        }

        // slip-on flange on Y axis: face at yFace, thickness dy (sign = direction)
        private static void FlangeY(DesignDocument design, double yFace, double dy, double pipeOD,
                                    double flOD, double pcd, double hole, int n)
        {
            var pl = new Plane(new Point3D(0, yFace, 0), Vector3D.AxisY);
            var curves = new List<ICurve>
            {
                new Circle(pl, flOD / 2),
                new Circle(pl, pipeOD / 2)
            };
            for (int i = 0; i < n; i++)
            {
                double a = Math.PI / n + 2 * Math.PI * i / n;
                var hp = new Plane(new Point3D(pcd / 2 * Math.Cos(a), yFace, pcd / 2 * Math.Sin(a)), Vector3D.AxisY);
                curves.Add(new Circle(hp, hole / 2));
            }
            var r = new devDept.Eyeshot.Entities.Region(curves.ToArray());
            design.Entities.Add(r.ExtrudeAsBrep(new Vector3D(0, dy, 0)), Color.Silver);
        }
    }
}