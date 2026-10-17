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
    public class DrainPipe
    {
        private readonly IConfiguration _configuration;
        public DrainPipe(IConfiguration configuration) { _configuration = configuration; }

        // Vertical pipe along -Z. Origin = axis at top face of top flange.
        // Drain Pipe: DrawDrainPipe(model, "DrainPipe", 1988, 1845, 100)
        public DesignDocument DrawDrainPipe(PaintBoothModel model, string name,
                                            double totalL, double valveTopL, double stubL)
        {
            DesignDocument design = new DesignDocument();
            design.Units = linearUnitsType.Millimeters;

            // ---- Pipe size (50NB, Sch 10S) ----                            (CHECK wall)
            double OD50 = 60.3, W50 = 2.77;

            // ---- BS10 Table D slip-on flange, 50NB ----                     (CHECK)
            double Fl50OD = 152.4, Fl50PCD = 114.3, Fl50T = 12.7;
            double BoltHole = 17.5;
            int NBolts = 4;

            // ---- 2" Ball valve (simplified) ----                            (CHECK)
            double valveL = totalL - valveTopL - stubL;   // 43 from drawing dims
            double valveBodyD = 90.0;
            double stemD = 14, stemH = 30;
            double leverL = 150, leverW = 20, leverT = 4;

            // ---- 2" 90° bend ----                                           (CHECK radius)
            double bendR = 76.2;                              // 1.5D long radius

            double z1 = -valveTopL;          // pipe bottom / valve top
            double z2 = z1 - valveL;         // valve bottom / stub top
            double z3 = z2 - stubL;          // stub bottom / bend start  (= -totalL)

            // ================= 1. PIPE + STUB =================
            AddTube(design, 0, z1, OD50, W50);                       // main pipe 1845
            AddTube(design, z2, z3, OD50, W50);                       // 100 stub

            // ================= 2. BALL VALVE =================
            double boreID = OD50 - 2 * W50;
            AddTube(design, z1, z2, valveBodyD, (valveBodyD - boreID) / 2);   // body

            double zMid = (z1 + z2) / 2;
            double rBody = valveBodyD / 2;

            // stem (along +Y)
            var stemPlane = new Plane(new Point3D(0, rBody, zMid), Vector3D.AxisY);
            var stem = new devDept.Eyeshot.Entities.Region(new Circle(stemPlane, stemD / 2));
            design.Entities.Add(stem.ExtrudeAsBrep(new Vector3D(0, stemH, 0)), Color.Silver);

            // lever (along +X, sits on top of stem)
            double yL = rBody + stemH;
            var lever = new devDept.Eyeshot.Entities.Region(new LinearPath(new[]
            {
                new Point3D(-stemD,  yL, zMid - leverW / 2),
                new Point3D(leverL,  yL, zMid - leverW / 2),
                new Point3D(leverL,  yL, zMid + leverW / 2),
                new Point3D(-stemD,  yL, zMid + leverW / 2),
                new Point3D(-stemD,  yL, zMid - leverW / 2)
            }));
            design.Entities.Add(lever.ExtrudeAsBrep(new Vector3D(0, leverT, 0)), Color.Red);

            // ================= 3. 90° BEND (turns to +X) =================
            AddBend(design, z3, OD50, W50, bendR);

            // ================= 4. TOP SLIP-ON FLANGE =================
            AddFlange(design, 0, -Fl50T, OD50, Fl50OD, Fl50PCD, BoltHole, NBolts);

            // ================= Export DWG =================
            var path = _configuration["FolderPathConfig:DrawingPath"].ToString();
            if (!Directory.Exists(path + "/PaintBooth drawing"))
                Directory.CreateDirectory(path + "/PaintBooth drawing");
            string dwgFilePath = $"{path}/PaintBooth drawing/{name} {DateTime.Now:hh - mm}.dwg";
            new WriteAutodesk(new WriteAutodeskParams(design), dwgFilePath).DoWork();

            return design;
        }

        // tube from zTop down to zBot
        private static void AddTube(DesignDocument design, double zTop, double zBot, double od, double wall)
        {
            Point3D c = new Point3D(0, 0, zTop);
            var ring = new devDept.Eyeshot.Entities.Region(
                new Circle(Plane.XY, c, od / 2), new Circle(Plane.XY, c, od / 2 - wall));
            design.Entities.Add(ring.ExtrudeAsBrep(new Vector3D(0, 0, zBot - zTop)), Color.Silver);
        }

        // 90° bend: starts at zStart going down (-Z), ends pointing +X
        private static void AddBend(DesignDocument design, double zStart, double od, double wall, double R)
        {
            Point3D c = new Point3D(0, 0, zStart);
            var ring = new devDept.Eyeshot.Entities.Region(
                new Circle(Plane.XY, c, od / 2), new Circle(Plane.XY, c, od / 2 - wall));

            // revolve about axis parallel to Y through (R, 0, zStart); -Y => sweeps downward
            var bend = ring.RevolveAsBrep(Math.PI / 2, new Vector3D(0, -1, 0),
                                          new Point3D(R, 0, zStart), 0.01);
            design.Entities.Add(bend, Color.Silver);
        }

        // slip-on flange: disc from zFace, thickness dz (sign = direction), bore = pipe OD
        private static void AddFlange(DesignDocument design, double zFace, double dz, double pipeOD,
                                      double flOD, double pcd, double hole, int n)
        {
            Point3D c = new Point3D(0, 0, zFace);
            var curves = new List<ICurve>
            {
                new Circle(Plane.XY, c, flOD / 2),
                new Circle(Plane.XY, c, pipeOD / 2)
            };
            for (int i = 0; i < n; i++)
            {
                double a = Math.PI / n + 2 * Math.PI * i / n;
                curves.Add(new Circle(Plane.XY, new Point3D(pcd / 2 * Math.Cos(a), pcd / 2 * Math.Sin(a), zFace), hole / 2));
            }
            var r = new devDept.Eyeshot.Entities.Region(curves.ToArray());
            design.Entities.Add(r.ExtrudeAsBrep(new Vector3D(0, 0, dz)), Color.Silver);
        }
    }
}