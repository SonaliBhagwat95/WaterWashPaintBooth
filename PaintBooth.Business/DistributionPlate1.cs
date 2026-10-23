using devDept.Eyeshot;
using devDept.Eyeshot.Entities;
using devDept.Eyeshot.Translators;
using devDept.Geometry;
using Microsoft.Extensions.Configuration;
using PaintBooth.Models;
using System.Drawing;

namespace PaintBooth.Business
{
    public class DistributionPlate1
    {
        private readonly IConfiguration _configuration;
        public double HookTopX { get; private set; }
        public double HookTopZ { get; private set; }
        public DistributionPlate1(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        // Plate 1: DrawDistributionPlate(model, "DistributionPlate1", 2318, 5 + 1128)
        // Plate 2: DrawDistributionPlate(model, "DistributionPlate2", 1678, 5 + 1130)
        public DesignDocument DrawDistributionPlate(PaintBoothModel model, string name,
                                                    double length, double slotFromLeft)
        {
            DesignDocument design = new DesignDocument();
            design.Units = linearUnitsType.Millimeters;

            // ---- Inputs (mm) ----
            double T = 1.6;           // sheet thickness
            double Length = length;        // extrusion along Y
            int Seg = 48;            // arc segments

            // Main plate (side view)
            double R = 127;           // centreline radius (outer face = 127.8)
            double Xcut = -(127.8 - 78); // vertical cut at top end = -49.8 from centre
            double StraightL = 230;
            double StraightDeg = -2.5;          // (CHECK)
            double TailL = 50;
            double TailDeg = -22;           // (CHECK)

            // H strip (detail H)
            double LapLen = 32;            // lap along main plate outer face (CHECK)
            double HookUp = 54;            // up-leg                          (CHECK)
            double HookUpDeg = 95;
            double HookFlat = 7;             // flat at top                     (CHECK)
            double HookFlatDeg = 180;
            double HookAng = 26;            // angled leg                      (CHECK)
            double HookAngDeg = 230.5;

            // Elevation cuts
            double EndCut = 5;             // (CHECK: 5 or 20)
            double SlotStart = slotFromLeft;
            double SlotW = 50;
            double TailCut = 50;

            double Rin = R - T / 2;            // main plate inner face (centre side)
            double Rout = R + T / 2;            // main plate outer face (strip side)

            // ================= 1. MAIN PLATE =================
            double startAng = Math.Acos(Xcut / R);                       // centreline at x = -49.8
            double endAng = (270 + StraightDeg) * Math.PI / 180.0;

            var m = new List<(double X, double Z)>();
            for (int i = 0; i <= Seg; i++)
            {
                double ang = startAng + (endAng - startAng) * i / Seg;
                m.Add((R * Math.Cos(ang), R * Math.Sin(ang)));
            }
            var B = Add(m[m.Count - 1], StraightL, StraightDeg);
            m.Add(B);
            int iB = m.Count - 1;
            m.Add(Add(B, TailL, TailDeg));
            int iEnd = m.Count - 1;

            Thicken(m, T / 2.0, out var mL, out var mR);

            // vertical cut at top end (left = inner face, right = outer face)
            mL[0] = (Xcut, Math.Sqrt(Rin * Rin - Xcut * Xcut));
            mR[0] = (Xcut, Math.Sqrt(Rout * Rout - Xcut * Xcut));

            AddPiece(design, mL, mR, 0, iB, 0, Length);                       // arc + straight
            AddPiece(design, mL, mR, iB, iEnd, TailCut, Length - TailCut);    // tail

            // ================= 2. H STRIP (on outer face) =================
            var Po = mR[0];                         // main plate top outer corner = strip bend corner
            double Rs = R + T;                      // strip lap centreline radius

            // up-leg centreline: line through Po shifted 0.8 to its left
            double ua = HookUpDeg * Math.PI / 180.0;
            double dx = Math.Cos(ua), dz = Math.Sin(ua);
            double px = Po.X - dz * T / 2, pz = Po.Z + dx * T / 2;

            // intersect up-leg centreline with lap centreline circle -> bend corner C
            double b = px * dx + pz * dz;
            double cc = px * px + pz * pz - Rs * Rs;
            double sq = Math.Sqrt(b * b - cc);
            double s1 = -b + sq, s2 = -b - sq;
            double sC = Math.Abs(s1) < Math.Abs(s2) ? s1 : s2;
            var C = (X: px + dx * sC, Z: pz + dz * sC);
            double angC = Math.Atan2(C.Z, C.X);

            var h = new List<(double X, double Z)>();
            double lapAng = LapLen / Rs;
            int segLap = 12;
            for (int i = 0; i <= segLap; i++)                            // lap end -> bend corner
            {
                double ang = angC + lapAng * (1.0 - (double)i / segLap);
                h.Add((Rs * Math.Cos(ang), Rs * Math.Sin(ang)));
            }
            var H1 = Add(C, HookUp, HookUpDeg);
            var H2 = Add(H1, HookFlat, HookFlatDeg);
            var H3 = Add(H2, HookAng, HookAngDeg);
            h.Add(H1);
            h.Add(H2);
            h.Add(H3);
            HookTopX = H1.X;          // hook top corner (centreline), local coords
            HookTopZ = H1.Z;
            //Thicken(h, T / 2.0, out var hL, out var hR);

            //int hEnd = h.Count - 1;
            //AddPiece(design, hL, hR, 0, hEnd, EndCut, SlotStart);
            //AddPiece(design, hL, hR, 0, hEnd, SlotStart + SlotW, Length - EndCut);
            HookTopX = H1.X;          // hook top corner (centreline), local coords
            HookTopZ = H1.Z;
            Thicken(h, T / 2.0, out var hL, out var hR);

            int hEnd = h.Count - 1;
            double slotEnd = SlotStart + SlotW;

            if (SlotStart > EndCut && slotEnd < Length - EndCut)
            {
                // slot fits inside this plate -> two strip pieces
                AddPiece(design, hL, hR, 0, hEnd, EndCut, SlotStart);
                AddPiece(design, hL, hR, 0, hEnd, slotEnd, Length - EndCut);
            }
            else
            {
                // plate too short for the slot -> one continuous strip
                AddPiece(design, hL, hR, 0, hEnd, EndCut, Length - EndCut);
            }

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

        // ---------- Helpers ----------

        private static (double X, double Z) Add((double X, double Z) p, double len, double deg)
        {
            double a = deg * Math.PI / 180.0;
            return (p.X + len * Math.Cos(a), p.Z + len * Math.Sin(a));
        }

        // Offsets a centreline by ±h with mitred joints
        private static void Thicken(List<(double X, double Z)> p, double h,
                                    out List<(double X, double Z)> left,
                                    out List<(double X, double Z)> right)
        {
            int n = p.Count;
            var nx = new double[n - 1];
            var nz = new double[n - 1];
            for (int k = 0; k < n - 1; k++)
            {
                double dx = p[k + 1].X - p[k].X, dz = p[k + 1].Z - p[k].Z;
                double l = Math.Sqrt(dx * dx + dz * dz);
                nx[k] = -dz / l; nz[k] = dx / l;
            }

            left = new List<(double X, double Z)>();
            right = new List<(double X, double Z)>();
            for (int i = 0; i < n; i++)
            {
                double mx, mz, s;
                if (i == 0) { mx = nx[0]; mz = nz[0]; s = h; }
                else if (i == n - 1) { mx = nx[n - 2]; mz = nz[n - 2]; s = h; }
                else
                {
                    double bx = nx[i - 1] + nx[i], bz = nz[i - 1] + nz[i];
                    double bl = Math.Sqrt(bx * bx + bz * bz);
                    if (bl < 1e-9) { mx = nx[i - 1]; mz = nz[i - 1]; s = h; }
                    else
                    {
                        mx = bx / bl; mz = bz / bl;
                        double cosHalf = mx * nx[i - 1] + mz * nz[i - 1];
                        s = Math.Min(h / cosHalf, 10 * h);
                    }
                }
                left.Add((p[i].X + mx * s, p[i].Z + mz * s));
                right.Add((p[i].X - mx * s, p[i].Z - mz * s));
            }
        }

        // Closed band between centreline indices i..j, extruded from y0 to y1
        private static void AddPiece(DesignDocument design,
                                     List<(double X, double Z)> left, List<(double X, double Z)> right,
                                     int i, int j, double y0, double y1)
        {
            var pts = new List<Point3D>();
            for (int k = i; k <= j; k++) pts.Add(new Point3D(left[k].X, 0, left[k].Z));
            for (int k = j; k >= i; k--) pts.Add(new Point3D(right[k].X, 0, right[k].Z));
            pts.Add(pts[0]);

            devDept.Eyeshot.Entities.Region profile = new devDept.Eyeshot.Entities.Region(new LinearPath(pts.ToArray()));
            Brep brep = profile.ExtrudeAsBrep(new Vector3D(0, y1 - y0, 0));
            brep.Translate(0, y0, 0);
            design.Entities.Add(brep, Color.DarkGray);
        }
    }
}
//public DesignDocument DrawDistributionPlate1(PaintBoothModel model)
//{
//    DesignDocument design = new DesignDocument();
//    design.Units = linearUnitsType.Millimeters;

//    // ---- Inputs (mm) ----
//    double T = 1.6;     // sheet thickness
//    double Length = 2318;    // extrusion along Y

//    // Side view
//    double R = 127;     // bend radius
//    double TopDx = 127.8 - 78;  // arc top end: 49.8 left of centre
//    double TopDz = 119;     // arc top end: 119 above centre
//    double StraightL = 230;     // straight after arc
//    double StraightDeg = -2.5;    // straight slope (down to right)   (CHECK)
//    double TailL = 50;      // last leg
//    double TailDeg = -22;     // last leg slope                  (CHECK)
//    double HookUp = 49;      // hook: up leg from arc top       (CHECK - detail H)
//    double HookUpDeg = 95;
//    double HookBack = 29;      // hook: folded-back leg           (CHECK - detail H)
//    double HookBackDeg = 223;
//    int Seg = 48;      // arc segments

//    // Elevation cut-outs
//    double EndCut = 5;       // hook cut back at each end
//    double SlotStart = 5 + 1128;// slot position from left
//    double SlotW = 50;      // slot width
//    double TailCut = 50;      // tail leg shorter at each end (2218)

//    // ================= Centreline (X, Z) - origin = arc centre =================
//    var c = new List<(double X, double Z)>();

//    double startAng = Math.Atan2(TopDz, -TopDx);                 // ~112.7°
//    double endAng = (270 + StraightDeg) * Math.PI / 180.0;     // tangent to straight
//    var A = (X: R * Math.Cos(startAng), Z: R * Math.Sin(startAng));

//    // hook (tip -> apex -> arc top)
//    var H1 = Add(A, HookUp, HookUpDeg);
//    var H0 = Add(H1, HookBack, HookBackDeg);
//    c.Add(H0);
//    c.Add(H1);

//    int iA = c.Count;                                            // arc top index
//    for (int i = 0; i <= Seg; i++)
//    {
//        double ang = startAng + (endAng - startAng) * i / Seg;
//        c.Add((R * Math.Cos(ang), R * Math.Sin(ang)));
//    }

//    var E = c[c.Count - 1];
//    var B = Add(E, StraightL, StraightDeg);
//    c.Add(B);
//    int iB = c.Count - 1;                                        // tail bend index
//    c.Add(Add(B, TailL, TailDeg));
//    int iEnd = c.Count - 1;

//    // ================= Thicken (mitred offsets) =================
//    Thicken(c, T / 2.0, out var left, out var right);

//    // ================= Extrude pieces =================
//    // body: arc + straight, full length
//    AddPiece(design, left, right, iA, iB, 0, Length);

//    // hook: end cut-backs + slot
//    AddPiece(design, left, right, 0, iA, EndCut, SlotStart);
//    AddPiece(design, left, right, 0, iA, SlotStart + SlotW, Length - EndCut);

//    // tail: 2218 long
//    AddPiece(design, left, right, iB, iEnd, TailCut, Length - TailCut);

//    // ================= Export DWG =================
//    var path = _configuration["FolderPathConfig:DrawingPath"].ToString();

//    if (!Directory.Exists(path + "/PaintBooth drawing"))
//        Directory.CreateDirectory(path + "/PaintBooth drawing");

//    string dwgFilePath = $"{path}/PaintBooth drawing/DistributionPlate1 {DateTime.Now:hh - mm}.dwg";

//    WriteAutodeskParams auto = new WriteAutodeskParams(design);
//    WriteAutodesk dwgg1 = new WriteAutodesk(auto, dwgFilePath);
//    dwgg1.DoWork();

//    return design;
//}

// ---------- Helpers ----------
//        private static (double X, double Z) Add((double X, double Z) p, double len, double deg)
//        {
//            double a = deg * Math.PI / 180.0;
//            return (p.X + len * Math.Cos(a), p.Z + len * Math.Sin(a));
//        }

//        // Offsets the centreline by ±h with mitred joints
//        private static void Thicken(List<(double X, double Z)> p, double h,
//                                    out List<(double X, double Z)> left,
//                                    out List<(double X, double Z)> right)
//        {
//            int n = p.Count;
//            var nx = new double[n - 1];
//            var nz = new double[n - 1];
//            for (int k = 0; k < n - 1; k++)
//            {
//                double dx = p[k + 1].X - p[k].X, dz = p[k + 1].Z - p[k].Z;
//                double l = Math.Sqrt(dx * dx + dz * dz);
//                nx[k] = -dz / l; nz[k] = dx / l;                      // left normal
//            }

//            left = new List<(double X, double Z)>();
//            right = new List<(double X, double Z)>();
//            for (int i = 0; i < n; i++)
//            {
//                double mx, mz, s;
//                if (i == 0) { mx = nx[0]; mz = nz[0]; s = h; }
//                else if (i == n - 1) { mx = nx[n - 2]; mz = nz[n - 2]; s = h; }
//                else
//                {
//                    double bx = nx[i - 1] + nx[i], bz = nz[i - 1] + nz[i];
//                    double bl = Math.Sqrt(bx * bx + bz * bz);
//                    if (bl < 1e-9) { mx = nx[i - 1]; mz = nz[i - 1]; s = h; }
//                    else
//                    {
//                        mx = bx / bl; mz = bz / bl;
//                        double cosHalf = mx * nx[i - 1] + mz * nz[i - 1];
//                        s = Math.Min(h / cosHalf, 10 * h);
//                    }
//                }
//                left.Add((p[i].X + mx * s, p[i].Z + mz * s));
//                right.Add((p[i].X - mx * s, p[i].Z - mz * s));
//            }
//        }

//        // Builds the closed band between indices i..j and extrudes it from y0 to y1
//        private static void AddPiece(DesignDocument design,
//                                     List<(double X, double Z)> left, List<(double X, double Z)> right,
//                                     int i, int j, double y0, double y1)
//        {
//            var pts = new List<Point3D>();
//            for (int k = i; k <= j; k++) pts.Add(new Point3D(left[k].X, 0, left[k].Z));
//            for (int k = j; k >= i; k--) pts.Add(new Point3D(right[k].X, 0, right[k].Z));
//            pts.Add(pts[0]);                                             // close

//            devDept.Eyeshot.Entities.Region profile = new devDept.Eyeshot.Entities.Region(new LinearPath(pts.ToArray()));
//            Brep brep = profile.ExtrudeAsBrep(new Vector3D(0, y1 - y0, 0));
//            brep.Translate(0, y0, 0);
//            design.Entities.Add(brep, Color.BurlyWood);
//        }
//    }
//}
