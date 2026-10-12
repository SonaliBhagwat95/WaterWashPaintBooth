using devDept.Eyeshot;
using devDept.Eyeshot.Entities;
using devDept.Eyeshot.Translators;
using devDept.Geometry;
using Microsoft.Extensions.Configuration;
using PaintBooth.Models;
using System;
using System.Drawing;
using System.IO;

namespace PaintBooth.Business
{
    public class DeflectorGusset
    {
        private readonly IConfiguration _configuration;

        public DeflectorGusset(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        // 218: DrawDeflectorGusset(model, "DeflectorGusset_218", +1)
        // 219: DrawDeflectorGusset(model, "DeflectorGusset_219", -1)
        public DesignDocument DrawDeflectorGusset(PaintBoothModel model, string name, int flangeSide)
        {
            DesignDocument design = new DesignDocument();
            design.Units = linearUnitsType.Millimeters;

            // ---- Inputs (mm) ----
            double T = 1.6;    // sheet thickness
            double BotLen = 203;    // bottom (flanged) edge
            double BotDeg = 25;     // 115° - 90°
            double Step = 10;     // vertical step at bottom-right
            double StepW = 38;     // horizontal at bottom-right
            double RightH = 277;    // right vertical edge
            double TopW = 48;     // top edge

            double PlHoleDia = 8;     // plate holes Ø   (CHECK - not dimensioned)
            double PlHoleX = 16;    // from right edge
            double PlHoleTop = 73;    // from top
            double PlHoleBot = 64;    // from bottom of right edge

            double FlangeW = 25;    // flange width (out of plate)
            double FlHoleDia = 8;     // flange holes Ø  (CHECK - drawn as slots?)
            double FlHole1 = 52;    // along edge from tip
            double FlPitch = 100;
            double FlHoleY = 12.5;  // across flange   (CHECK)

            // ---- Plate outline (X, Z) - origin = bottom tip ----
            double a = BotDeg * Math.PI / 180.0;
            double ux = Math.Cos(a), uz = Math.Sin(a);       // along bottom edge
            double nx = -uz, nz = ux;                         // inward normal of bottom edge

            var E = (X: BotLen * ux, Z: BotLen * uz);        // end of bottom edge
            var S = (X: E.X, Z: E.Z + Step);
            var R = (X: S.X + StepW, Z: S.Z);
            var T2 = (X: R.X, Z: R.Z + RightH);      // top-right
            var T1 = (X: T2.X - TopW, Z: T2.Z);              // top-left (long edge 411 @ 65°)

            // plate stops T above the bottom edge (flange takes that thickness)
            double wl = Math.Sqrt(T1.X * T1.X + T1.Z * T1.Z);
            double wx = T1.X / wl, wz = T1.Z / wl;            // long edge direction
            double sinTip = ux * wz - uz * wx;                // sin(40°)
            var A0 = (X: wx * T / sinTip, Z: wz * T / sinTip);// long edge meets offset line
            var E1 = (X: E.X, Z: E.Z + T / ux);               // offset line meets step

            LinearPath plateOuter = new LinearPath(new Point3D[]
            {
                P(A0.X, A0.Z),
                P(E1.X, E1.Z),
                P(S.X,  S.Z),
                P(R.X,  R.Z),
                P(T2.X, T2.Z),
                P(T1.X, T1.Z),
                P(A0.X, A0.Z)   // close
            });

            Circle ph1 = new Circle(Plane.XZ, P(R.X - PlHoleX, T2.Z - PlHoleTop), PlHoleDia / 2);
            Circle ph2 = new Circle(Plane.XZ, P(R.X - PlHoleX, R.Z + PlHoleBot), PlHoleDia / 2);

            devDept.Eyeshot.Entities.Region plate = new devDept.Eyeshot.Entities.Region(plateOuter, ph1, ph2);
            Brep plateBrep = plate.ExtrudeAsBrep(new Vector3D(0, T, 0));
            design.Entities.Add(plateBrep, Color.BurlyWood);

            // ---- Flange along bottom edge (plane: along edge x Y, extruded along inward normal) ----
            double y0 = flangeSide > 0 ? 0 : T - FlangeW;     // +1: towards +Y, -1: towards -Y
            Vector3D u3 = new Vector3D(ux, 0, uz);

            Point3D L(double s, double y) => new Point3D(ux * s, y0 + y, uz * s);

            LinearPath flangeOuter = new LinearPath(new Point3D[]
            {
                L(0, 0), L(BotLen, 0), L(BotLen, FlangeW), L(0, FlangeW), L(0, 0)
            });

            Circle FlHole(double s)
            {
                Point3D c = L(s, FlHoleY);
                return new Circle(new Plane(c, u3, Vector3D.AxisY), c, FlHoleDia / 2);
            }

            devDept.Eyeshot.Entities.Region flange = new devDept.Eyeshot.Entities.Region(
                flangeOuter, FlHole(FlHole1), FlHole(FlHole1 + FlPitch));
            Brep flangeBrep = flange.ExtrudeAsBrep(new Vector3D(nx * T, 0, nz * T));
            design.Entities.Add(flangeBrep, Color.BurlyWood);

            // ---- Export DWG ----
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
    }
}