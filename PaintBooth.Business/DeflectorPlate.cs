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
    public class DeflectorPlate
    {
        private readonly IConfiguration _configuration;
        public DeflectorPlate(IConfiguration configuration) { _configuration = configuration; }

        // Plate 1: DrawDeflectorPlate(model, "DeflectorPlate1", 2318)
        public DesignDocument DrawDeflectorPlate(PaintBoothModel model, string name, double length)
        {
            DesignDocument design = new DesignDocument();
            design.Units = linearUnitsType.Millimeters;

            // ---- Inputs (mm) ----
            double T = 1.6;      // 1.6 THK
            double L = length;   // along Y (2318)
            double SlopeL = 216;      // sloped leg
            double SlopeDeg = 25;       // 180 - 155
            double TopW = 26;       // top flat
            double VertL = 38;       // vertical leg down (to bottom of U)
            double UWidth = 12;       // U bend outer width
            double UHeight = 16;       // U return leg height from bottom
            int SegU = 12;

            // slots 10 x 12 in top flat
            double SlotW = 10;       // across (X)
            double SlotL = 12;       // along (Y)
            double SlotFromV = 14;       // slot centre from vertical leg
            double SlotEnd1 = 80;       // first slot from Y = 0
            double SlotPitch = 200;
            int SlotsPerGroup = 6;   // 5 x 200 = 1000
            double GroupGap = 160;
            int SegSlot = 8;

            // ---- Centreline (X, Z) - origin = bottom of sloped leg ----
            double a = SlopeDeg * Math.PI / 180.0;
            var P0 = (X: 0.0, Z: 0.0);
            var P1 = (X: SlopeL * Math.Cos(a), Z: SlopeL * Math.Sin(a));   // top of slope
            var P2 = (X: P1.X + TopW, Z: P1.Z);                           // end of top flat
            double zBot = P2.Z - VertL;                                    // bottom of U (centreline)
            double rU = (UWidth - T) / 2;                                // U centreline radius

            // ================= 1. SLOPED LEG =================
            var slope = new List<(double X, double Z)> { P0, P1 };
            Thicken(slope, T / 2, out var sL, out var sR);
            AddPiece(design, sL, sR, 0, 1, 0, L);

            // ================= 2. TOP FLAT with slots (XY, extruded down) =================
            double zTop = P1.Z + T / 2;
            var flatCurves = new List<ICurve>
            {
                new LinearPath(new Point3D[]
                {
                    new Point3D(P1.X, 0, zTop), new Point3D(P2.X, 0, zTop),
                    new Point3D(P2.X, L, zTop), new Point3D(P1.X, L, zTop),
                    new Point3D(P1.X, 0, zTop)
                })
            };
            double sx = P2.X - SlotFromV;
            for (int g = 0; g < 2; g++)
            {
                double y0 = SlotEnd1 + g * ((SlotsPerGroup - 1) * SlotPitch + GroupGap);
                for (int i = 0; i < SlotsPerGroup; i++)
                {
                    double yc = y0 + i * SlotPitch;
                    if (yc < L) flatCurves.Add(SlotXY(sx, yc, zTop, SlotW, SlotL, SegSlot));
                }
            }
            var flat = new devDept.Eyeshot.Entities.Region(flatCurves.ToArray());
            design.Entities.Add(flat.ExtrudeAsBrep(new Vector3D(0, 0, -T)), Color.BurlyWood);

            // ================= 3. VERTICAL LEG + U BEND =================
            // ---- U channel inputs ----
            double UW = 12;      // outer width
            double UH = 16;      // outer height
            double UT = 1.6;     // thickness
            double ULen = L;     // length along Y (full length)                 (CHECK)

            // ================= 3. VERTICAL LEG (straight, down into U) =================
            double zTopOuter = P1.Z + T / 2;                 // top face of top flat
            double zU0 = zTopOuter - VertL;            // U outer bottom (38 below top)

            var v = new List<(double X, double Z)>
{
    P2,                                          // from top flat corner
    (P2.X, zU0 + UT)                             // down to U inside bottom
};
            Thicken(v, T / 2, out var vL, out var vR);
            AddPiece(design, vL, vR, 0, 1, 0, L);

            // ================= 4. U CHANNEL 12 x 16 x 1.6 (separate, welded) =================
            // inner leg against the vertical leg's slope-side face, U opens upward, extends away from slope
            double xU0 = P2.X - T / 2 - UT;                  // U outer face on slope side
            LinearPath uPath = new LinearPath(new Point3D[]
            {
                new Point3D(xU0,           0, zU0),
                new Point3D(xU0 + UW,      0, zU0),
                new Point3D(xU0 + UW,      0, zU0 + UH),
                new Point3D(xU0 + UW - UT, 0, zU0 + UH),
                new Point3D(xU0 + UW - UT, 0, zU0 + UT),
                new Point3D(xU0 + UT,      0, zU0 + UT),
                new Point3D(xU0 + UT,      0, zU0 + UH),
                new Point3D(xU0,           0, zU0 + UH),
                new Point3D(xU0,           0, zU0)
            });
            var uRegion = new devDept.Eyeshot.Entities.Region(uPath);
            design.Entities.Add(uRegion.ExtrudeAsBrep(new Vector3D(0, ULen, 0)), Color.BurlyWood);
            // ================= Export DWG =================
            var path = _configuration["FolderPathConfig:DrawingPath"].ToString();
            if (!Directory.Exists(path + "/PaintBooth drawing"))
                Directory.CreateDirectory(path + "/PaintBooth drawing");
            string dwgFilePath = $"{path}/PaintBooth drawing/{name} {DateTime.Now:hh - mm}.dwg";
            new WriteAutodesk(new WriteAutodeskParams(design), dwgFilePath).DoWork();

            return design;
        }

        // Obround slot in a horizontal plane: width w across X, length l along Y
        private static LinearPath SlotXY(double cx, double cy, double z, double w, double l, int seg)
        {
            double r = w / 2, h = Math.Max(0, (l - w) / 2);
            var pts = new List<Point3D>();
            for (int i = 0; i <= seg; i++)
            {
                double t = Math.PI * i / seg;
                pts.Add(new Point3D(cx + r * Math.Cos(t), cy + h + r * Math.Sin(t), z));
            }
            for (int i = 0; i <= seg; i++)
            {
                double t = Math.PI + Math.PI * i / seg;
                pts.Add(new Point3D(cx + r * Math.Cos(t), cy - h + r * Math.Sin(t), z));
            }
            pts.Add(pts[0]);
            return new LinearPath(pts.ToArray());
        }
    }
}