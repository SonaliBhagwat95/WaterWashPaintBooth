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
    public class BasePlateReturnTrough
    {
        private readonly IConfiguration _configuration;

        public BasePlateReturnTrough(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        // holeYs = positions of the Ø8 hole pairs along the length (from the square end)
        public DesignDocument DrawBasePlateReturnTrough(PaintBoothModel model, double[] holeYs)
        {
            DesignDocument design = new DesignDocument();
            design.Units = linearUnitsType.Millimeters;

            // ---- Inputs (mm) ----
            double T = 1.6;
            double TopW = 139;     // top flat (outer)
            double BotW = 196;     // bottom opening (outer)
            double H = 180;     // height
            double LenTop = 1870;    // length at top
            double EndVert = 38;      // vertical part of end cut
            double CutDeg = 45;      // end cut angle
            double HoleDia = 8;
            double Hole1X = 24;      // across top, from left edge
            double HolePitch = 92;

            double off = (BotW - TopW) / 2.0;          // 28.5
            double cutH = H - EndVert;                  // 142
            double LenBot = LenTop - cutH / Math.Tan(CutDeg * Math.PI / 180.0);  // 1728

            // ================= 1. TOP PLATE (XY, extruded down) with Ø8 holes =================
            double xL = off, xR = off + TopW;            // top outer edges
            var topCurves = new List<ICurve>
            {
                new LinearPath(new Point3D[]
                {
                    new Point3D(xL, 0,      H), new Point3D(xR, 0,      H),
                    new Point3D(xR, LenTop, H), new Point3D(xL, LenTop, H),
                    new Point3D(xL, 0,      H)
                })
            };
            if (holeYs != null)
                foreach (double y in holeYs)
                {
                    topCurves.Add(new Circle(Plane.XY, new Point3D(xL + Hole1X, y, H), HoleDia / 2));
                    topCurves.Add(new Circle(Plane.XY, new Point3D(xL + Hole1X + HolePitch, y, H), HoleDia / 2));
                }

            devDept.Eyeshot.Entities.Region top = new devDept.Eyeshot.Entities.Region(topCurves.ToArray());
            design.Entities.Add(top.ExtrudeAsBrep(new Vector3D(0, 0, -T)), Color.BurlyWood);

            // ================= 2. SIDE WALLS (sloped planes, end cut, thickened inwards) =================
            // wall outline in elevation (Y, Z): square end at Y=0, 38 vertical + 45° at the other end
            var elev = new (double Y, double Z)[]
            {
                (0,      0),
                (LenBot, 0),
                (LenTop, cutH),
                (LenTop, H - T),
                (0,      H - T),
                (0,      0)
            };

            AddWall(design, elev, H, T,
                    z => off * z / H,                // left outer face X at height z
                    +1);                             // inward = +X
            AddWall(design, elev, H, T,
                    z => BotW - off * z / H,         // right outer face X at height z
                    -1);                             // inward = -X

            // ================= Export DWG =================
            var path = _configuration["FolderPathConfig:DrawingPath"].ToString();

            if (!Directory.Exists(path + "/PaintBooth drawing"))
                Directory.CreateDirectory(path + "/PaintBooth drawing");

            string dwgFilePath = $"{path}/PaintBooth drawing/BasePlateReturnTrough {DateTime.Now:hh - mm}.dwg";

            WriteAutodeskParams auto = new WriteAutodeskParams(design);
            WriteAutodesk dwgg1 = new WriteAutodesk(auto, dwgFilePath);
            dwgg1.DoWork();

            return design;
        }

        private static void AddWall(DesignDocument design, (double Y, double Z)[] elev, double H, double T,
                                    Func<double, double> xOuter, int inward)
        {
            var pts = new List<Point3D>();
            foreach (var p in elev)
                pts.Add(new Point3D(xOuter(p.Z), p.Y, p.Z));

            // inward normal of the sloped wall
            double dx = xOuter(H) - xOuter(0);             // wall run over full height
            double len = Math.Sqrt(dx * dx + H * H);
            Vector3D n = new Vector3D(inward * H / len, 0, -inward * dx / len * (inward));
            // n points into the trough, perpendicular to the wall face
            n = new Vector3D(inward * H / len, 0, -Math.Abs(dx) / len);

            devDept.Eyeshot.Entities.Region wall = new devDept.Eyeshot.Entities.Region(new LinearPath(pts.ToArray()));
            Brep b = wall.ExtrudeAsBrep(new Vector3D(n.X * T, 0, n.Z * T));
            design.Entities.Add(b, Color.BurlyWood);
        }
    }
}