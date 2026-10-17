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

namespace PaintBooth.Business.Water_Wash_Parts
{
    public class WaterScreenPanel
    {
        private readonly IConfiguration _configuration;
        public WaterScreenPanel(IConfiguration configuration) { _configuration = configuration; }

        // leftDir / rightDir : +1 = flange "UP" (+X), -1 = "DOWN" (-X) as in the plan
        // Panel 1: (1140, 1130, +1, -1)   Panel 2: (1140, 1130, -1, -1)   Panel 3: (572, 570, -1, +1)
        public DesignDocument DrawWaterScreenPanel(PaintBoothModel model, string name,
                                                   double width, double stiffLen, int leftDir, int rightDir)
        {
            DesignDocument design = new DesignDocument();
            design.Units = linearUnitsType.Millimeters;

            // ---- Inputs (mm) ----
            double T = 1.6;
            double W = width;     // along Y
            double H = 2348;      // overall height
            double TopLip = 22;        // lip at top, turned back (-X)          (CHECK)
            double TopBand = 65;
            double MainL = 2091;
            double BotH = 192;       // bottom part, vertical height
            double KinkDeg = 8;         // 180 - 172
            double Foot = 38;        // (F) foot at bottom, toward -X         (CHECK)

            double FlD = 19;        // side flange depth
            double SlotW = 10, SlotL = 12;
            double Slot1 = 147;       // first slot from top
            double SlotP = 200;
            int NSlots = 10;

            double[] stiffFromTop = { 65 + 300, 65 + 300 + 800, 65 + 300 + 1600 };   // 365, 1165, 1965
            double HatD = 35, HatW = 45, HatF = 15;

            // ================= 1. PANEL PROFILE (XZ, extruded along Y) =================
            double zMainBot = H - TopBand - MainL;                 // = 192
            double kickX = BotH * Math.Tan(KinkDeg * Math.PI / 180.0);
            var c = new List<(double X, double Z)>
            {
                (-TopLip, H - T / 2),          // end of top lip
                (0,       H - T / 2),          // top corner
                (0,       H - TopBand),        // end of top band
                (0,       zMainBot),           // bottom of main face (172° bend)
                (kickX,   T / 2),              // bottom of kicked-out part
                (kickX - Foot, T / 2)          // end of foot
            };
            Thicken(c, T / 2, out var pl, out var pr);
            AddPiece(design, pl, pr, 0, c.Count - 1, 0, W);

            // ================= 2. SIDE FLANGES with 10x12 slots (XZ, through Y) =================
            AddFlange(design, 0, leftDir, FlD, T, zMainBot, H, Slot1, SlotP, NSlots, SlotW, SlotL);
            AddFlange(design, W - T, rightDir, FlD, T, zMainBot, H, Slot1, SlotP, NSlots, SlotW, SlotL);

            // ================= 3. HAT STIFFENERS on back face (-X) =================
            double x0 = -T;                                        // flange centreline on back face
            double xW = -T / 2 - HatD + T / 2;                     // web centreline
            double yS = (W - stiffLen) / 2;
            foreach (double ft in stiffFromTop)
            {
                double zc = H - ft;
                var hat = new List<(double X, double Z)>
                {
                    (x0, zc + HatW / 2 + HatF),
                    (x0, zc + HatW / 2),
                    (xW, zc + HatW / 2),
                    (xW, zc - HatW / 2),
                    (x0, zc - HatW / 2),
                    (x0, zc - HatW / 2 - HatF)
                };
                Thicken(hat, T / 2, out var hl, out var hr);
                AddPiece(design, hl, hr, 0, hat.Count - 1, yS, yS + stiffLen);
            }

            // ================= Export DWG =================
            var path = _configuration["FolderPathConfig:DrawingPath"].ToString();
            if (!Directory.Exists(path + "/PaintBooth drawing"))
                Directory.CreateDirectory(path + "/PaintBooth drawing");
            string dwgFilePath = $"{path}/PaintBooth drawing/{name} {DateTime.Now:hh - mm}.dwg";
            new WriteAutodesk(new WriteAutodeskParams(design), dwgFilePath).DoWork();

            return design;
        }

        // flange plate in XZ at Y = y0 (1.6 thick), depth along dir*X, from zBot to zTop, with slots
        private static void AddFlange(DesignDocument design, double y0, int dir, double depth, double t,
                                      double zBot, double zTop, double slot1, double pitch, int n,
                                      double sw, double sl)
        {
            double x1 = dir * depth;
            var curves = new List<ICurve>
            {
                new LinearPath(new Point3D[]
                {
                    new Point3D(0,  0, zBot), new Point3D(x1, 0, zBot),
                    new Point3D(x1, 0, zTop), new Point3D(0,  0, zTop),
                    new Point3D(0,  0, zBot)
                })
            };
            double xc = dir * depth / 2;
            for (int i = 0; i < n; i++)
            {
                double zc = zTop - slot1 - i * pitch;
                if (zc - sl / 2 > zBot) curves.Add(SlotXZ(xc, zc, sw, sl));
            }
            var r = new devDept.Eyeshot.Entities.Region(curves.ToArray());
            Brep b = r.ExtrudeAsBrep(new Vector3D(0, t, 0));
            b.Translate(0, y0, 0);
            design.Entities.Add(b, Color.BurlyWood);
        }

        // vertical obround in XZ: width w across X, length l along Z
        private static LinearPath SlotXZ(double cx, double cz, double w, double l)
        {
            double r = w / 2, h = Math.Max(0, (l - w) / 2);
            int seg = 8;
            var pts = new List<Point3D>();
            for (int i = 0; i <= seg; i++)
            {
                double a = Math.PI * i / seg;
                pts.Add(new Point3D(cx + r * Math.Cos(a), 0, cz + h + r * Math.Sin(a)));
            }
            for (int i = 0; i <= seg; i++)
            {
                double a = Math.PI + Math.PI * i / seg;
                pts.Add(new Point3D(cx + r * Math.Cos(a), 0, cz - h + r * Math.Sin(a)));
            }
            pts.Add(pts[0]);
            return new LinearPath(pts.ToArray());
        }
    }
}