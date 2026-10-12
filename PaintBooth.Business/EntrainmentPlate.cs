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
    public class EntrainmentPlate
    {
        private readonly IConfiguration _configuration;

        public EntrainmentPlate(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        // Entrainment Plate 1: DrawEntrainmentPlate(model, "EntrainmentPlate1", 1152)
        public DesignDocument DrawEntrainmentPlate(PaintBoothModel model, string name, double width)
        {
            DesignDocument design = new DesignDocument();
            design.Units = linearUnitsType.Millimeters;

            // ---- Inputs (mm) ----
            double T = 2;       // 2T
            double W = width;   // along X
            double H = 216;

            // serrations
            double EndFlat = 28.4;    // plain at each end
            double VWidth = 64;      // notch width at bottom edge
            double VDepth = 38;      // notch depth (80°)
            double FlatNom = 5;       // nominal flat between notches

            // slots
            double SlotW = 11;
            double SlotL = 87;
            double SlotTop = 13;      // from top edge
            double SlotEnd = 157;     // first/last slot from ends
            int SegSlot = 12;

            // angle 25 x 25 x 3
            double AngLeg = 25;
            double AngT = 3;
            double AngTopZ = 70;      // top of angle from bottom
            double AngEnd = 111;     // from each end (930 long for W = 1152)

            // ================= 1. PLATE (XZ, extruded along Y) =================
            double avail = W - 2 * EndFlat;
            int nV = (int)Math.Round((avail + FlatNom) / (VWidth + FlatNom));   // 16
            double flat = nV > 1 ? (avail - nV * VWidth) / (nV - 1) : 0;           // ≈ 4.75

            var pts = new List<Point3D>();
            pts.Add(P(0, H));
            pts.Add(P(0, 0));
            double x = EndFlat;
            pts.Add(P(x, 0));
            for (int i = 0; i < nV; i++)
            {
                pts.Add(P(x + VWidth / 2, VDepth));     // notch apex
                pts.Add(P(x + VWidth, 0));
                x += VWidth;
                if (i < nV - 1)
                {
                    x += flat;
                    pts.Add(P(x, 0));
                }
            }
            pts.Add(P(W, 0));
            pts.Add(P(W, H));
            pts.Add(P(0, H));                            // close

            var curves = new List<ICurve> { new LinearPath(pts.ToArray()) };

            // slots (vertical obround)
            double zTopC = H - SlotTop - SlotW / 2;      // upper arc centre
            double zBotC = zTopC - (SlotL - SlotW);      // lower arc centre (76 apart)
            foreach (double sx in new[] { SlotEnd, W / 2, W - SlotEnd })
                curves.Add(Slot(sx, zTopC, zBotC, SlotW / 2, SegSlot));

            devDept.Eyeshot.Entities.Region plate = new devDept.Eyeshot.Entities.Region(curves.ToArray());
            Brep plateBrep = plate.ExtrudeAsBrep(new Vector3D(0, T, 0));
            design.Entities.Add(plateBrep, Color.BurlyWood);

            // ================= 2. ANGLE 25x25x3 (YZ, extruded along X), on -Y face =================
            double ax = AngEnd;
            LinearPath angPath = new LinearPath(new Point3D[]
            {
                new Point3D(ax,  0,       AngTopZ - AngLeg),   // leg against plate, bottom
                new Point3D(ax,  0,       AngTopZ),
                new Point3D(ax, -AngLeg,  AngTopZ),            // outstanding leg
                new Point3D(ax, -AngLeg,  AngTopZ - AngT),
                new Point3D(ax, -AngT,    AngTopZ - AngT),
                new Point3D(ax, -AngT,    AngTopZ - AngLeg),
                new Point3D(ax,  0,       AngTopZ - AngLeg)    // close
            });
            devDept.Eyeshot.Entities.Region ang = new devDept.Eyeshot.Entities.Region(angPath);
            Brep angBrep = ang.ExtrudeAsBrep(new Vector3D(W - 2 * AngEnd, 0, 0));
            design.Entities.Add(angBrep, Color.BurlyWood);

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

        private static Point3D P(double x, double z) => new Point3D(x, 0, z);

        // Vertical obround slot in XZ: arcs centred at (cx, zTop) and (cx, zBot)
        private static LinearPath Slot(double cx, double zTop, double zBot, double r, int seg)
        {
            var s = new List<Point3D>();
            for (int i = 0; i <= seg; i++)                       // top arc: 0° -> 180°
            {
                double a = Math.PI * i / seg;
                s.Add(P(cx + r * Math.Cos(a), zTop + r * Math.Sin(a)));
            }
            for (int i = 0; i <= seg; i++)                       // bottom arc: 180° -> 360°
            {
                double a = Math.PI + Math.PI * i / seg;
                s.Add(P(cx + r * Math.Cos(a), zBot + r * Math.Sin(a)));
            }
            s.Add(s[0]);                                         // close
            return new LinearPath(s.ToArray());
        }
    }
}