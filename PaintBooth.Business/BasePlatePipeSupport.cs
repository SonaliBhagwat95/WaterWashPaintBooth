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
using static PaintBooth.Business.SheetProfile;

namespace PaintBooth.Business
{
    public class BasePlatePipeSupport
    {
        private readonly IConfiguration _configuration;
        public BasePlatePipeSupport(IConfiguration configuration) { _configuration = configuration; }

        // used for placing in assembly
        public double AxisX { get; private set; }          // a point on the round pipe axis (local)
        public double AxisZ { get; private set; }
        public double PipeDeg { get; private set; } = 36;    // slope below horizontal
        public double PipeOD { get; private set; } = 26.67; // 3/4" NB pipe (plan shows 26)

        public DesignDocument DrawBasePlatePipeSupport(PaintBoothModel model)
        {
            DesignDocument design = new DesignDocument();
            design.Units = linearUnitsType.Millimeters;

            // ---- Inputs (mm) ----
            // Pipe support top bracket
            double T = 3;            // bracket thickness
            double TopL = 300;          // along Y
            double LegV = 41;           // vertical leg (holes)
            double LegA = 33;           // short leg at 126°                        (CHECK)
            double TopHoleD = 8;
            double TopHoleE = 28.5;         // 243 pitch
            //double TopHoleZ = -LegV / 2;    // hole centre height                       (CHECK)
            double TopHoleZ = -13;          // Ø8 bolt hole 13 below top (same as tie bar)
            // 90° hanger angle (separate piece, wall side)
            double AngT = 3;            // thickness
            double AngTopL = 35;           // top leg, outer face to tip               (CHECK)
            double AngVerL = 37;           // vertical leg, top outer face down        (CHECK)
            double AngR = 3;            // inside bend radius                       (CHECK)
            double AngGap = 3;            // gap above pipe-support bracket top       (CHECK)

            // Cover strip on top of the leg / flat joint
            double StripL = 23;           // along slope
            double StripW = 33;           // along Y
            double StripT = 3;
            double StripS = 10;           // start from bend along slope              (CHECK)

            // Pipe 3/4" NB x 15 SWG
            double PipeWall = 1.83;
            double FlatL = 60;       // bend -> start of round pipe
            double TopTransL = 16;       // bulb: flat -> round (inside the 60)      (CHECK)
            double FlatT = 2 * PipeWall;
            double PipeTopAbove = 4;        // round pipe top above flat top face       (CHECK)

            // Crimped bottom end
            double CrimpTransL = 20;       // (CHECK)
            double CrimpTongueL = 8;        // (CHECK)

            // Bottom tab (detail A)
            double TabX = 818.4;    // tab +X face from bracket outer face (plan)
            double TabBotZ = -693;     // tab bottom below bracket top (side view)
            double TabW = 38;
            double TabHoleD = 8;
            double TabHolePitch = 19;
            double TabHoleZ = 15;       // (CHECK)

            double a = PipeDeg * Math.PI / 180.0;
            double ux = Math.Cos(a), uz = -Math.Sin(a);       // down the slope
            double nx = Math.Sin(a), nz = Math.Cos(a);        // normal, upper side
            double yC = TopL / 2;
            double r = PipeOD / 2;
            double fW = Math.PI * PipeOD / 4;                 // half flat width (≈21)
            double fT = FlatT / 2;

            Vector3D d = new Vector3D(ux, 0, uz);
            Vector3D n = new Vector3D(nx, 0, nz);

            // ================= 1. PIPE SUPPORT TOP BRACKET =================
            // 1a. vertical leg with Ø8 holes (YZ, through X): outer face x = 0, top z = 0
            var vLeg = new List<ICurve>
    {
        RectYZ(0, 0, TopL, -LegV, 0),
        new Circle(Plane.YZ, new Point3D(0, TopHoleE,        TopHoleZ), TopHoleD / 2),
        new Circle(Plane.YZ, new Point3D(0, TopL - TopHoleE, TopHoleZ), TopHoleD / 2)
    };
            AddExtrude(design, new devDept.Eyeshot.Entities.Region(vLeg.ToArray()), new Vector3D(T, 0, 0));

            // 1b. 126° short leg
            double bx = T / 2, bz = -LegV;                                   // bend (centreline)
            var leg = new List<(double X, double Z)> { (bx, bz), (bx + ux * LegA, bz + uz * LegA) };
            Thicken(leg, T / 2.0, out var lL, out var lR);
            AddPiece(design, lL, lR, 0, 1, 0, TopL);

            // ================= 1c. 90° HANGER ANGLE (wall side, bolted through same Ø8 holes) =================
            // outer faces: x = -AngT (on wall), top z = AngGap + AngT
            double angTopZ = AngGap + AngT;
            double rcA = AngR + AngT / 2;                                     // centreline radius
            double acX = -AngT / 2 + rcA, acZ = angTopZ - AngT / 2 - rcA;     // bend centre

            // vertical part with Ø8 holes (YZ, through X), from bottom up to bend start
            var aVer = new List<ICurve>
    {
        RectYZ(-AngT, 0, TopL, angTopZ - AngVerL, acZ),
        new Circle(Plane.YZ, new Point3D(-AngT, TopHoleE,        TopHoleZ), TopHoleD / 2),
        new Circle(Plane.YZ, new Point3D(-AngT, TopL - TopHoleE, TopHoleZ), TopHoleD / 2)
    };
            AddExtrude(design, new devDept.Eyeshot.Entities.Region(aVer.ToArray()), new Vector3D(AngT, 0, 0));

            // round corner + top leg (XZ, along Y)
            var aTop = new List<(double X, double Z)> { (-AngT / 2, acZ) };
            for (int i = 1; i <= 12; i++)
            {
                double ang = Math.PI - (Math.PI / 2) * i / 12;               // 180° -> 90°
                aTop.Add((acX + rcA * Math.Cos(ang), acZ + rcA * Math.Sin(ang)));
            }
            aTop.Add((-AngT + AngTopL, angTopZ - AngT / 2));                 // tip of top leg
            Thicken(aTop, AngT / 2.0, out var aL, out var aR);
            AddPiece(design, aL, aR, 0, aTop.Count - 1, 0, TopL);

            // ================= 2. PIPE =================
            // 2a. flat end: in line with the short leg, from end of leg to start of bulb
            double flatEnd = FlatL - TopTransL;
            var flat = new List<(double X, double Z)>
    {
        (bx + ux * LegA,    bz + uz * LegA),
        (bx + ux * flatEnd, bz + uz * flatEnd)
    };
            Thicken(flat, fT, out var fL, out var fR);
            AddPiece(design, fL, fR, 0, 1, yC - fW, yC + fW);

            // cover strip on top, across the leg / flat joint
            double so = Math.Max(T, FlatT) / 2 + StripT / 2;
            var strip = new List<(double X, double Z)>
    {
        (bx + ux * StripS            + nx * so, bz + uz * StripS            + nz * so),
        (bx + ux * (StripS + StripL) + nx * so, bz + uz * (StripS + StripL) + nz * so)
    };
            Thicken(strip, StripT / 2, out var sL, out var sR);
            AddPiece(design, sL, sR, 0, 1, yC - StripW / 2, yC + StripW / 2);

            // 2b. round pipe axis: top of pipe is PipeTopAbove above the flat top face
            double axOff = fT + PipeTopAbove - r;                            // negative -> below centreline
            AxisX = bx + nx * axOff;                                         // axis point at s = 0
            AxisZ = bz + nz * axOff;

            // top bulb: flat ellipse (s = flatEnd) -> round (s = FlatL)
            Point3D cF = new Point3D(bx + ux * flatEnd, yC, bz + uz * flatEnd);
            Point3D p0 = new Point3D(AxisX + ux * FlatL, yC, AxisZ + uz * FlatL);
            AddTransition(design, cF, fW, fT, p0, r, n, yC);

            // round pipe until its underside reaches tab-bottom level
            double ulX = AxisX - nx * r, ulZ = AxisZ - nz * r;
            double sEnd = (TabBotZ - ulZ) / uz;
            double roundL = sEnd - FlatL;

            Plane pp = new Plane(p0, Vector3D.AxisY, n);
            var ring = new devDept.Eyeshot.Entities.Region(
                new Circle(pp, p0, r), new Circle(pp, p0, r - PipeWall));
            AddExtrude(design, ring, new Vector3D(d.X * roundL, 0, d.Z * roundL), Color.Silver);

            // 2c. crimped bottom end: round -> flat tongue along the underside
            Point3D p1 = new Point3D(p0.X + d.X * roundL, yC, p0.Z + d.Z * roundL);
            Point3D c2 = new Point3D(p1.X + d.X * CrimpTransL - n.X * (r - fT), yC,
                                     p1.Z + d.Z * CrimpTransL - n.Z * (r - fT));
            AddTransition(design, c2, fW, fT, p1, r, n, yC);

            Plane tp = new Plane(c2, Vector3D.AxisY, n);
            AddExtrude(design, new devDept.Eyeshot.Entities.Region(new Ellipse(tp, c2, fW, fT)),
                       new Vector3D(d.X * CrimpTongueL, 0, d.Z * CrimpTongueL), Color.Silver);

            // ================= 3. BOTTOM TAB (detail A) =================
            double sTab = (TabX - ulX) / ux;
            double tabTop = ulZ + uz * sTab;

            var tab = new List<ICurve>
    {
        RectYZ(TabX - T, yC - TabW / 2, yC + TabW / 2, TabBotZ, tabTop),
        new Circle(Plane.YZ, new Point3D(TabX - T, yC - TabHolePitch / 2, TabBotZ + TabHoleZ), TabHoleD / 2),
        new Circle(Plane.YZ, new Point3D(TabX - T, yC + TabHolePitch / 2, TabBotZ + TabHoleZ), TabHoleD / 2)
    };
            AddExtrude(design, new devDept.Eyeshot.Entities.Region(tab.ToArray()), new Vector3D(T, 0, 0));

            //// ================= Export DWG =================
            //var path = _configuration["FolderPathConfig:DrawingPath"].ToString();
            //if (!Directory.Exists(path + "/PaintBooth drawing"))
            //    Directory.CreateDirectory(path + "/PaintBooth drawing");
            //string dwgFilePath = $"{path}/PaintBooth drawing/BasePlatePipeSupport {DateTime.Now:hh - mm}.dwg";
            //new WriteAutodesk(new WriteAutodeskParams(design), dwgFilePath).DoWork();

            return design;
        }

        // Flat ellipse section (centre cF, half-width fw, half-thk ft) -> round section (centre cR, radius r), as a mesh skin
        private static void AddTransition(DesignDocument design, Point3D cF, double fw, double ft,
                                          Point3D cR, double r, Vector3D n, double yC)
        {
            int N = 32;
            var verts = new List<Point3D>();
            for (int i = 0; i < N; i++)
            {
                double t = 2 * Math.PI * i / N;
                verts.Add(new Point3D(cR.X + n.X * r * Math.Sin(t), yC + r * Math.Cos(t), cR.Z + n.Z * r * Math.Sin(t)));
            }
            for (int i = 0; i < N; i++)
            {
                double t = 2 * Math.PI * i / N;
                verts.Add(new Point3D(cF.X + n.X * ft * Math.Sin(t), yC + fw * Math.Cos(t), cF.Z + n.Z * ft * Math.Sin(t)));
            }
            var tris = new List<IndexTriangle>();
            for (int i = 0; i < N; i++)
            {
                int a0 = i, a1 = (i + 1) % N, b0 = N + i, b1 = N + (i + 1) % N;
                tris.Add(new IndexTriangle(a0, a1, b1));
                tris.Add(new IndexTriangle(a0, b1, b0));
            }
            design.Entities.Add(new Mesh(verts.ToArray(), tris.ToArray()), Color.Silver);
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

        private static void AddExtrude(DesignDocument design, devDept.Eyeshot.Entities.Region r,
                                       Vector3D dir, Color? color = null)
        {
            design.Entities.Add(r.ExtrudeAsBrep(dir), color ?? Color.DarkGray);
        }
    }
}