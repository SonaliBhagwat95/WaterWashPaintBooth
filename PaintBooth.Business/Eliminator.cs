using devDept.Eyeshot;
using devDept.Eyeshot.Entities;
using devDept.Eyeshot.Translators;
using devDept.Geometry;
using Microsoft.Extensions.Configuration;
using PaintBooth.Models;
using System.Drawing;

using static PaintBooth.Business.SheetProfile;

namespace PaintBooth.Business
{
    public class Eliminator
    {
        private readonly IConfiguration _configuration;

        public Eliminator(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        // Eliminator 1: DrawEliminator(model, "Eliminator1", 1150)
        // Eliminator 2: DrawEliminator(model, "Eliminator2", 510)
        public DesignDocument DrawEliminator(PaintBoothModel model, string name, double width)
        {
            DesignDocument design = new DesignDocument();
            design.Units = linearUnitsType.Millimeters;

            // ---- Inputs (mm) ----
            double T = 1.6;     // plate thickness
            double W = width;   // along Y
            double H = 1028;    // overall height
            double PlateH = 533;     // plate height
            double TopFold = 38;      // top fold (towards -X, angle side)
            double BotFold = 38;      // bottom fold (towards +X)

            double AngLeg = 38;      // angle 38 x 38 x 3
            double AngT = 3;
            double AngTopGap = 12;      // angle top below plate top
            double AngSide = 13;      // angle outer face from plate side edge
            double AngLen = H - AngTopGap;   // 1016
            double Chamfer = AngLeg;  // 45° cut (135°) on outstanding leg

            // ================= 1. PLATE (side profile in XZ, extruded along Y) =================
            // plate faces: x = 0 (back, angle side) .. x = T (front)
            var c = new List<(double X, double Z)>
            {
                (-TopFold,      H - T / 2),             // top fold end
                (T / 2,         H - T / 2),             // top bend
                (T / 2,         H - PlateH + T / 2),    // bottom bend
                (T / 2 + BotFold - T / 2, H - PlateH + T / 2)  // bottom fold end (x = 38)
            };
            Thicken(c, T / 2.0, out var left, out var right);
            AddPiece(design, left, right, 0, c.Count - 1, 0, W);

            // ================= 2. ANGLES (welded on back face, x <= 0) =================
            // Outstanding leg: in XZ, x = -38..0, 45° cut at bottom, thickness 3 along Y
            LinearPath legOutPath = new LinearPath(new Point3D[]
            {
                P(0,        0),
                P(0,        AngLen),
                P(-AngLeg,  AngLen),
                P(-AngLeg,  Chamfer),       // 45° cut -> 135° corner
                P(0,        0)
            });
            devDept.Eyeshot.Entities.Region legOut = new devDept.Eyeshot.Entities.Region(legOutPath);

            // Flat leg: against plate, x = -3..0, width (38 - 3) along Y
            LinearPath legFlatPath = new LinearPath(new Point3D[]
            {
                P(-AngT, 0), P(0, 0), P(0, AngLen), P(-AngT, AngLen), P(-AngT, 0)
            });
            devDept.Eyeshot.Entities.Region legFlat = new devDept.Eyeshot.Entities.Region(legFlatPath);

            // left angle: outstanding leg at outer edge
            AddExtrude(design, legOut, AngSide, AngT);
            AddExtrude(design, legFlat, AngSide + AngT, AngLeg - AngT);

            // right angle: mirrored
            AddExtrude(design, legOut, W - AngSide - AngT, AngT);
            AddExtrude(design, legFlat, W - AngSide - AngLeg, AngLeg - AngT);

            //// ================= Export DWG =================
            //var path = _configuration["FolderPathConfig:DrawingPath"].ToString();

            //if (!Directory.Exists(path + "/PaintBooth drawing"))
            //    Directory.CreateDirectory(path + "/PaintBooth drawing");

            //string dwgFilePath = $"{path}/PaintBooth drawing/{name} {DateTime.Now:hh - mm}.dwg";

            //WriteAutodeskParams auto = new WriteAutodeskParams(design);
            //WriteAutodesk dwgg1 = new WriteAutodesk(auto, dwgFilePath);
            //dwgg1.DoWork();

            return design;
        }

        private static Point3D P(double x, double z) => new Point3D(x, 0, z);

        private static void AddExtrude(DesignDocument design, devDept.Eyeshot.Entities.Region r,
                                       double y0, double len)
        {
            Brep b = r.ExtrudeAsBrep(new Vector3D(0, len, 0));
            b.Translate(0, y0, 0);
            design.Entities.Add(b, Color.LightPink);
        }
    }
}