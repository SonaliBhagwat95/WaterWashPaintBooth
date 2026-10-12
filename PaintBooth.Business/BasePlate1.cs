using devDept.Eyeshot;
using devDept.Eyeshot.Entities;
using devDept.Eyeshot.Translators;
using devDept.Geometry;
using Microsoft.Extensions.Configuration;
using PaintBooth.Models;
using System.Drawing;

namespace PaintBooth.Business
{
    public class BasePlate1
    {
        private readonly IConfiguration _configuration;

        public BasePlate1(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public DesignDocument DrawBasePlate1(PaintBoothModel model)
        {
            DesignDocument design = new DesignDocument();
            design.Units = linearUnitsType.Millimeters;

            // ---- Inputs (mm) ----
            //double Depth = (double)model.W; // extrusion along Y - change if base plate depth differs
            double Depth = 2318;
            double ApexX = 682;   // left end -> bend (horizontal)
            double ApexHeight = 211;   // lip bottom -> bend
            double Lip = 30;    // left turned-down lip
            double RightLeg = 120;   // bend -> right end (along web)
            double BendAngle = 154;   // included angle at bend (deg)
            double FH = 25;    // flange height (perpendicular to web)
            double T = 3;     // sheet thickness
            double HoleDia = 8;
            double Hole1Dist = 127;   // along web from left end
            double HolePitch = 460;

            // ---- Directions in XZ ----
            double a1 = Math.Atan2(ApexHeight - Lip, ApexX);                     // left slope (~14.86°)
            double a2 = a1 - (Math.PI - BendAngle * Math.PI / 180.0);            // right leg (~ -11.14°)
            double u1x = Math.Cos(a1), u1z = Math.Sin(a1), n1x = -u1z, n1z = u1x;
            double u2x = Math.Cos(a2), u2z = Math.Sin(a2), n2x = -u2z, n2z = u2x;

            // ---- Key points (X, 0, Z) - origin = bottom-left of lip ----
            Point3D P0 = new Point3D(0, 0, 0);                                                   // lip bottom outer
            Point3D P1 = new Point3D(0, 0, Lip);                                                 // web start (bottom)
            Point3D A = new Point3D(ApexX, 0, ApexHeight);                                      // bend (bottom)
            Point3D B = new Point3D(A.X + RightLeg * u2x, 0, A.Z + RightLeg * u2z);             // right end (bottom)

            Point3D T0 = new Point3D(0, 0, Lip + FH / Math.Cos(a1));                             // flange top-left
            Point3D Q1 = new Point3D(A.X + n1x * FH, 0, A.Z + n1z * FH);                         // V-gap left top
            Point3D K1 = new Point3D(A.X + n1x * T, 0, A.Z + n1z * T);                          // V-gap left bottom
            Point3D K2 = new Point3D(A.X + n2x * T, 0, A.Z + n2z * T);                          // V-gap right bottom
            Point3D Q2 = new Point3D(A.X + n2x * FH, 0, A.Z + n2z * FH);                         // V-gap right top
            Point3D T3 = new Point3D(B.X, 0, B.Z + FH / Math.Cos(a2));                           // flange top-right
            Point3D C = new Point3D(T, 0, Lip + T * Math.Tan(a1));                              // lip inner / web bottom
            Point3D D = new Point3D(T, 0, 0);                                                   // lip inner bottom

            LinearPath p = new LinearPath(new Point3D[]
            {
                P0,   // lip bottom outer
                T0,   // left edge up to flange top
                Q1,   // flange top, left leg
                K1,   // V-gap down
                K2,   // V-gap bottom
                Q2,   // V-gap up
                T3,   // flange top, right leg
                B,    // right end down
                A,    // web bottom, right leg
                C,    // web bottom, left leg
                D,    // lip inner down
                P0    // close
            });

            // ---- Ø8 holes (mid-height of flange) ----
            Point3D h1 = new Point3D(P1.X + u1x * Hole1Dist + n1x * FH / 2, 0,
                                     P1.Z + u1z * Hole1Dist + n1z * FH / 2);
            Point3D h2 = new Point3D(h1.X + u1x * HolePitch, 0, h1.Z + u1z * HolePitch);
            Circle hole1 = new Circle(Plane.XZ, h1, HoleDia / 2);
            Circle hole2 = new Circle(Plane.XZ, h2, HoleDia / 2);

            devDept.Eyeshot.Entities.Region profile = new devDept.Eyeshot.Entities.Region(p, hole1, hole2);
            Brep brepBasePlate = profile.ExtrudeAsBrep(new Vector3D(0, Depth, 0));
            //brepBasePlate.Translate((model.D), 0, 0);
            design.Entities.Add(brepBasePlate, Color.Gold);

            // ---- Export DWG ----
            var path = _configuration["FolderPathConfig:DrawingPath"].ToString();

            if (!Directory.Exists(path + "/PaintBooth drawing"))
                Directory.CreateDirectory(path + "/PaintBooth drawing");

            string dwgFilePath = $"{path}/PaintBooth drawing/BasePlate1 {DateTime.Now:hh - mm}.dwg";

            WriteAutodeskParams auto = new WriteAutodeskParams(design);
            WriteAutodesk dwgg1 = new WriteAutodesk(auto, dwgFilePath);
            dwgg1.DoWork();

            return design;
        }
    }
}

