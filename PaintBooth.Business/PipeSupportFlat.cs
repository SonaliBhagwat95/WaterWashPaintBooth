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

namespace PaintBooth.Business
{
    public class PipeSupportFlat
    {
        private readonly IConfiguration _configuration;
        public PipeSupportFlat(IConfiguration configuration) { _configuration = configuration; }

        public DesignDocument DrawTieBar(PaintBoothModel model)
        {
            DesignDocument design = new DesignDocument();
            design.Units = linearUnitsType.Millimeters;

            // ---- Inputs (mm) ----
            // Flat bar
            double BarT = 5;        // 5T
            double BarL = 300;      // along Y
            double BarH = 32;       // height (13 + 19)
            double BarHoleD = 8;
            double BarHoleE = 28.5;     // 243 pitch
            double BarHoleZ = -13;      // hole centre below top
            double PipeAxisZ = -19;     // pipe centre 19 below bar top (elevation "19" / "32")
            // Pipe 3/4" NB SCH 10
            double PipeOD = 26.67;
            double PipeWall = 2.11;
            double PipeY = 147.5;    // pipe axis from bar end (plan)

            // Flattened end
            double HoleX = 1004;     // Ø8 hole centre from bar outer face (horizontal)
            double HoleZ = -259;     // Ø8 hole centre below bar top
            double TipBeyond = 13;       // tip beyond hole (horizontal)
            double FlatLen = 47.9;     // flat length to tip (along axis)
            double FlatW = 36;       // flat width (in elevation)
            double FlatT = 5;        // flat thickness (plan)
            double FlatHoleD = 8;
            double TransL = 10;       // round -> flat transition             (CHECK)

            double r = PipeOD / 2;

            // ---- Pipe axis: from bar face (top flush) to the Ø8 hole ----
            double x0 = BarT, z0 = PipeAxisZ;                        // axis start on bar face                              // axis start on bar face
            double a = Math.Atan2(z0 - HoleZ, HoleX - x0);          // slope below horizontal (~13.8°)
            double ux = Math.Cos(a), uz = -Math.Sin(a);              // down the axis
            double nx = Math.Sin(a), nz = Math.Cos(a);               // normal in XZ (upper side)
            Vector3D d = new Vector3D(ux, 0, uz);
            Vector3D n = new Vector3D(nx, 0, nz);

            double sHole = (HoleX - x0) / ux;                       // along axis to hole
            double sTip = sHole + TipBeyond / ux;
            double sFlat = sTip - FlatLen;                          // flat starts here
            double sRound = sFlat - TransL;                          // round pipe ends here

            Point3D P(double s, double y) => new Point3D(x0 + ux * s, y, z0 + uz * s);

            // ================= 1. FLAT BAR 5T with 2 x Ø8 (YZ, through X) =================
            var bar = new List<ICurve>
            {
                RectYZ(0, 0, BarL, -BarH, 0),
                new Circle(Plane.YZ, new Point3D(0, BarHoleE,        BarHoleZ), BarHoleD / 2),
                new Circle(Plane.YZ, new Point3D(0, BarL - BarHoleE, BarHoleZ), BarHoleD / 2)
            };
            design.Entities.Add(new devDept.Eyeshot.Entities.Region(bar.ToArray())
                                .ExtrudeAsBrep(new Vector3D(BarT, 0, 0)), Color.BurlyWood);

            // ================= 2. ROUND PIPE (ring square to axis) =================
            Point3D p0 = P(0, PipeY);
            Plane pp = new Plane(p0, Vector3D.AxisY, n);
            var ring = new devDept.Eyeshot.Entities.Region(
                new Circle(pp, p0, r), new Circle(pp, p0, r - PipeWall));
            design.Entities.Add(ring.ExtrudeAsBrep(new Vector3D(d.X * sRound, 0, d.Z * sRound)), Color.Silver);

            // ================= 3. TRANSITION round -> vertical flat (mesh skin) =================
            Point3D cR = P(sRound, PipeY);
            Point3D cF = P(sFlat, PipeY);
            int N = 32;
            var verts = new List<Point3D>();
            for (int i = 0; i < N; i++)                               // round section
            {
                double t = 2 * Math.PI * i / N;
                verts.Add(new Point3D(cR.X + n.X * r * Math.Sin(t), cR.Y + r * Math.Cos(t), cR.Z + n.Z * r * Math.Sin(t)));
            }
            for (int i = 0; i < N; i++)                               // flat: wide in XZ, thin in Y
            {
                double t = 2 * Math.PI * i / N;
                verts.Add(new Point3D(cF.X + n.X * (FlatW / 2) * Math.Sin(t), cF.Y + (FlatT / 2) * Math.Cos(t),
                                      cF.Z + n.Z * (FlatW / 2) * Math.Sin(t)));
            }
            var tris = new List<IndexTriangle>();
            for (int i = 0; i < N; i++)
            {
                int a0 = i, a1 = (i + 1) % N, b0 = N + i, b1 = N + (i + 1) % N;
                tris.Add(new IndexTriangle(a0, a1, b1));
                tris.Add(new IndexTriangle(a0, b1, b0));
            }
            design.Entities.Add(new Mesh(verts.ToArray(), tris.ToArray()), Color.Silver);

            // ================= 4. FLAT END 47.9 x 36 x 5 with Ø8 (XZ, through Y) =================
            double hw = FlatW / 2;
            Point3D Q(double s, double w) => new Point3D(x0 + ux * s + nx * w, 0, z0 + uz * s + nz * w);

            LinearPath flatPath = new LinearPath(new Point3D[]
            {
                Q(sFlat, -hw), Q(sTip, -hw), Q(sTip, hw), Q(sFlat, hw), Q(sFlat, -hw)
            });
            Point3D hc = Q(sHole, 0);
            var flatRegion = new devDept.Eyeshot.Entities.Region(
                flatPath, new Circle(Plane.XZ, hc, FlatHoleD / 2));
            Brep flat = flatRegion.ExtrudeAsBrep(new Vector3D(0, FlatT, 0));
            flat.Translate(0, PipeY - FlatT / 2, 0);
            design.Entities.Add(flat, Color.Silver);

            // ================= Export DWG =================
            var path = _configuration["FolderPathConfig:DrawingPath"].ToString();
            if (!Directory.Exists(path + "/PaintBooth drawing"))
                Directory.CreateDirectory(path + "/PaintBooth drawing");
            string dwgFilePath = $"{path}/PaintBooth drawing/TieBar {DateTime.Now:hh - mm}.dwg";
            new WriteAutodesk(new WriteAutodeskParams(design), dwgFilePath).DoWork();

            return design;
        }

        private static LinearPath RectYZ(double x, double y0, double y1, double z0, double z1)
        {
            return new LinearPath(new Point3D[]
            {
                new Point3D(x, y0, z0), new Point3D(x, y1, z0),
                new Point3D(x, y1, z1), new Point3D(x, y0, z1),
                new Point3D(x, y0, z0)
            });
        }
    }
}