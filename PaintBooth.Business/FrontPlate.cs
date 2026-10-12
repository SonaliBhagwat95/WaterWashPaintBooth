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
    public class FrontPlate
    {
        private readonly IConfiguration _configuration;

        public FrontPlate(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        // Front Plate 1: DrawFrontPlate(model, "FrontPlate1", 1158)
        public DesignDocument DrawFrontPlate(PaintBoothModel model, string name, double width, bool withNuts = true)
        {
            DesignDocument design = new DesignDocument();
            design.Units = linearUnitsType.Millimeters;

            // ---- Inputs (mm) ----
            double T = 1.6;     // plate thickness
            double W = width;   // along Y
            double H = 420;     // overall height (top of hem)
            double CurlOD = 8.5;     // hem Ø8.5 / Ø5.3
            double HemLeg = 10;      // hem return leg ends 10 below top
            int SegCurl = 24;

            double FlangeD = 25;      // side flange depth (towards back, +X)
            double FlangeTop = 386;     // flange top at plate face (420 - 34)

            double HoleDia = 10;
            double HoleZ = 76;      // from plate bottom
            double HoleEnd = 160;     // first/last hole from ends

            double StripW = 32;      // along Y
            double StripT = 5;       // 5 THK
            double StripGap = 5;       // 5 GAP to plate
            double StripZ0 = 57;      // strip bottom from plate bottom
            double StripH = 163;
            double WeldH = 40;      // top part welded via spacer

            double NutAF = 13;      // M8 nut across flats
            double NutH = 6.5;
            double NutBore = 8;

            double[] holeY = { HoleEnd, W / 2, W - HoleEnd };   // 160, 579, 998 for W = 1158

            // ================= 1. PLATE - vertical part (YZ, extruded along X) with Ø10 holes =================
            double rc = (CurlOD - T) / 2.0;          // hem centreline radius (3.45)
            double xc = T / 2 + rc;                  // hem centre X
            double zc = H - CurlOD / 2.0;            // hem centre Z

            var plateCurves = new List<ICurve> { RectYZ(0, 0, W, 0, zc) };
            foreach (double y in holeY)
                plateCurves.Add(new Circle(Plane.YZ, new Point3D(0, y, HoleZ), HoleDia / 2));

            AddRegionX(design, new devDept.Eyeshot.Entities.Region(plateCurves.ToArray()), 0, T);

            // ================= 2. PLATE - 180° hem at top (XZ, extruded along Y) =================
            var c = new List<(double X, double Z)>();
            for (int i = 0; i <= SegCurl; i++)
            {
                double ang = (180 - 180.0 * i / SegCurl) * Math.PI / 180.0;  // 180° -> 0°, over the top
                c.Add((xc + rc * Math.Cos(ang), zc + rc * Math.Sin(ang)));
            }
            c.Add((xc + rc, H - HemLeg));            // return leg straight down to 10 below top

            Thicken(c, T / 2.0, out var left, out var right);
            AddPiece(design, left, right, 0, c.Count - 1, 0, W);

            // ================= 3. SIDE FLANGES 25 deep, 45° top (XZ, extruded along Y) =================
            LinearPath flangePath = new LinearPath(new Point3D[]
            {
        new Point3D(T,       0, 0),
        new Point3D(FlangeD, 0, 0),
        new Point3D(FlangeD, 0, FlangeTop - FlangeD),   // 361 - 45° cut
        new Point3D(T,       0, FlangeTop - T),         // on 45° line from (0, 386)
        new Point3D(T,       0, 0)
            });
            devDept.Eyeshot.Entities.Region flange = new devDept.Eyeshot.Entities.Region(flangePath);

            Brep flangeL = flange.ExtrudeAsBrep(new Vector3D(0, T, 0));
            design.Entities.Add(flangeL, Color.BurlyWood);

            Brep flangeR = flange.ExtrudeAsBrep(new Vector3D(0, T, 0));
            flangeR.Translate(0, W - T, 0);
            design.Entities.Add(flangeR, Color.BurlyWood);

            // ================= 4. HANGER STRIPS + spacers + nuts =================
            double xStrip0 = T + StripGap;           // strip back face
            foreach (double y in holeY)
            {
                double y0 = y - StripW / 2, y1 = y + StripW / 2;

                // spacer (top 40, fills the 5 gap, welded)
                AddRegionX(design, new devDept.Eyeshot.Entities.Region(
                    RectYZ(T, y0, y1, StripZ0 + StripH - WeldH, StripZ0 + StripH)), T, StripGap);

                // strip 5 THK with Ø10 hole
                AddRegionX(design, new devDept.Eyeshot.Entities.Region(
                    RectYZ(xStrip0, y0, y1, StripZ0, StripZ0 + StripH),
                    new Circle(Plane.YZ, new Point3D(xStrip0, y, HoleZ), HoleDia / 2)),
                    xStrip0, StripT);

                // M8 nut tack welded on outside face
                if (withNuts)
                {
                    double xn = xStrip0 + StripT;
                    double rHex = NutAF / Math.Sqrt(3);          // circumradius
                    var hex = new List<Point3D>();
                    for (int k = 0; k <= 6; k++)
                    {
                        double a = (30 + 60 * k) * Math.PI / 180.0;
                        hex.Add(new Point3D(xn, y + rHex * Math.Cos(a), HoleZ + rHex * Math.Sin(a)));
                    }
                    AddRegionX(design, new devDept.Eyeshot.Entities.Region(
                        new LinearPath(hex.ToArray()),
                        new Circle(Plane.YZ, new Point3D(xn, y, HoleZ), NutBore / 2)),
                        xn, NutH, Color.Silver);
                }
            }

            // ================= Export DWG =================
            var path = _configuration["FolderPathConfig:DrawingPath"].ToString();

            if (!Directory.Exists(path + "/PaintBooth drawing"))
                Directory.CreateDirectory(path + "/PaintBooth drawing");

            string dwgFilePath = $"{path}/PaintBooth drawing/{name} {DateTime.Now:hh - mm}.dwg";

            WriteAutodeskParams auto = new WriteAutodeskParams(design);
            WriteAutodesk dwgg1 = new WriteAutodesk(auto, dwgFilePath);
            dwgg1.DoWork();

            return design;
        }

        // Rectangle in the YZ plane at X = x
        private static LinearPath RectYZ(double x, double y0, double y1, double z0, double z1)
        {
            return new LinearPath(new Point3D[]
            {
                new Point3D(x, y0, z0), new Point3D(x, y1, z0),
                new Point3D(x, y1, z1), new Point3D(x, y0, z1),
                new Point3D(x, y0, z0)
            });
        }

        // Extrude a YZ region along +X; region is assumed to lie at X = x0
        private static void AddRegionX(DesignDocument design, devDept.Eyeshot.Entities.Region r,
                                       double x0, double len, Color? color = null)
        {
            Brep b = r.ExtrudeAsBrep(new Vector3D(len, 0, 0));
            design.Entities.Add(b, color ?? Color.BurlyWood);
        }
    }
}