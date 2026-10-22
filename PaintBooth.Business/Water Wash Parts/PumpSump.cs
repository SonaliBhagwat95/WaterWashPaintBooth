using devDept.Eyeshot;
using devDept.Eyeshot.Entities;
using devDept.Geometry;
using Microsoft.Extensions.Configuration;
using PaintBooth.Models;
using System.Drawing;

namespace PaintBooth.Business.Water_Wash_Parts
{
    // Pump suction sump on the pump end of the water tank.
    // Local: X = depth from tank front outer face (0..635), Y = from sump outer face (0..750, 750 = tank wall),
    //        Z = 0 tank bottom .. 460 tank top.
    public class PumpSump
    {
        private readonly IConfiguration _configuration;
        public PumpSump(IConfiguration configuration) { _configuration = configuration; }

        public DesignDocument DrawPumpSump(PaintBoothModel model, string name, double H = 460)
        {
            DesignDocument d = new DesignDocument();
            d.Units = linearUnitsType.Millimeters;


            double inner = 498;                                      // client plan: guides wall to wall    (CHECK)
            double Dout = inner + 2 * 38, Wout = 750, wall = 38, t = 3;   // 580 outer depth

            // ---- suction pipe position (needed for back wall hole) ----
            double OD = 73.0, W = 3.05;                  
            double yc = wall + 325;                                   // pipe axis from outer inner face
            double zc = 150;                                          // pipe axis above tank bottom (CHECK)

            // ================= 1. BOX: floor + front + back + outer side (tank wall closes 4th side) =================
            Box(d, 0, Dout, 0, Wout, 0, t, Color.SaddleBrown);                       // floor
            Box(d, 0, wall, 0, Wout, 0, H, Color.SaddleBrown);                       // front wall 38 (inner face X = 38)
            Box(d, 0, Dout, 0, wall, 0, H, Color.SaddleBrown);                       // outer side wall 38

            // back wall 38 (inner face X = 597) with hole for the 65NB suction pipe
            {
                var face = new LinearPath(new[]
                {
            new Point3D(Dout - wall, 0,    0), new Point3D(Dout - wall, Wout, 0),
            new Point3D(Dout - wall, Wout, H), new Point3D(Dout - wall, 0,    H),
            new Point3D(Dout - wall, 0,    0)
        });
                var hole = new Circle(new Plane(new Point3D(Dout - wall, yc, zc), Vector3D.AxisX), OD / 2 + 1);
                d.Entities.Add(new devDept.Eyeshot.Entities.Region(face, hole)
                               .ExtrudeAsBrep(new Vector3D(wall, 0, 0)), Color.SaddleBrown);
            }

            // ================= 2. SCREEN GUIDES at the two sides of the 495 opening =================
            // view 'A': [25 foot | leg] 8 slot [leg | 25 foot] 17 [25 foot | leg] 8 slot [leg | 25 foot]
            double gA = 25, gT = 3, gL = 325;
            double s1 = wall + 568, s2 = s1 + 8 + 67;                 // slot starts (from sump outer inner face)
            var guides = new (double ya, double yb, bool legHigh)[]
            {
        (s1 - gA, s1,          true),
        (s1 + 8,  s1 + 8 + gA, false),
        (s2 - gA, s2,          true),
        (s2 + 8,  s2 + 8 + gA, false),
            };
            double xF = wall, openW = 495;                            // opening: front inner face .. +495
            foreach (var (xw, sx) in new[] { (xF, +1.0), (xF + openW, -1.0) })   // front jamb, back jamb
                foreach (var (ya, yb, legHigh) in guides)
                    GuideAngle(d, xw, sx, ya, yb, legHigh, t, t + gL, gA, gT);       // floor → 325 up

            // ISA 38x38x3 x 495 along top of opening, bolted to tank wall (sump side, local Y = Wout)   (CHECK height)
            //double zOpenTop = H - 25;
            double zOpenTop = t + gL;                                // top of opening = top of guides
            var isa = new[]
            {
        new Point3D(xF, Wout,      zOpenTop - 38), new Point3D(xF, Wout - 3,  zOpenTop - 38),
        new Point3D(xF, Wout - 3,  zOpenTop - 3),  new Point3D(xF, Wout - 38, zOpenTop - 3),
        new Point3D(xF, Wout - 38, zOpenTop),      new Point3D(xF, Wout,      zOpenTop),
        new Point3D(xF, Wout,      zOpenTop - 38)
    };
            d.Entities.Add(new devDept.Eyeshot.Entities.Region(new LinearPath(isa))
                           .ExtrudeAsBrep(new Vector3D(openW, 0, 0)), Color.SteelBlue);

            // ================= 3. SUCTION PIPE 65NB through back wall, 312 inside =================
            double xIn = (Dout - wall) - 312;                         // inner end, 312 from back inner face
            double xFl = xIn + 600;                                   // flange face, outside back wall
            TubeX(d, xIn, xFl, yc, zc, OD, W);
            FlangeX(d, xFl, -12.7, yc, zc, OD, 165.1, 127.0, 17.5, 4);   // BS10 Table D 65NB

            // ================= 4. STRAINERS 480 x 320 in the opening, ends in the jamb slots =================
            double stW = 480, stH = 320, stT = 6, rim = 20;
            double sx0 = xF + (openW - stW) / 2;                     // 7.5 clear each side in 495
            double zb = t, zt = t + stH;                             // sits on floor, 320 up

            Strainer(d, sx0, stW, s2 + 4, zb, zt, stT, rim, Color.FromArgb(140, Color.Silver));     // 06 101 (10 wire) - tank side
            Strainer(d, sx0, stW, s1 + 4, zb, zt, stT, rim, Color.FromArgb(170, Color.LightGray));  // 06 102 (6 wire)  - pump side

            // ================= 5. STRAINER COVER 743 x 574 x 1.6 on sump top =================
            double cY = 743, cX = 574, cT = 1.6;
            double lipY = 667, lipX = 498, lipD = 25;                    // locating skirt underneath   (CHECK depth)
            double xm = Dout / 2, ym = Wout / 2;                         // cover centred on sump
            Color coverCol = Color.FromArgb(80, Color.LightSteelBlue);   // alpha 80 of 255 → mostly see-through

            Box(d, xm - cX / 2, xm + cX / 2, ym - cY / 2, ym + cY / 2, H, H + cT, coverCol);   // cover sheet

            double lx0 = xm - lipX / 2, lx1 = xm + lipX / 2, ly0 = ym - lipY / 2, ly1 = ym + lipY / 2;
            Box(d, lx0, lx1, ly0, ly0 + cT, H - lipD, H, coverCol);          // skirt
            Box(d, lx0, lx1, ly1 - cT, ly1, H - lipD, H, coverCol);
            Box(d, lx0, lx0 + cT, ly0, ly1, H - lipD, H, coverCol);
            Box(d, lx1 - cT, lx1, ly0, ly1, H - lipD, H, coverCol);

            // 2 handles: Ø8 rod bent U, legs 120 apart along X, 50 high, at ±150 from centre along Y   (CHECK size)
            double hD = 8, hSpan = 120, hHt = 50;
            foreach (double hy in new[] { ym - 150, ym + 150 })
            {
                double xa = xm - hSpan / 2, xb = xm + hSpan / 2, z0 = H + cT, z1 = z0 + hHt;
                RodZ(d, xa, hy, z0, z1, hD);                             // leg
                RodZ(d, xb, hy, z0, z1, hD);                             // leg
                RodX(d, xa, xb, hy, z1, hD);                             // grip
            }



            // ================= Export DWG =================
            //var path = _configuration["FolderPathConfig:DrawingPath"].ToString();
            //if (!Directory.Exists(path + "/PaintBooth drawing"))
            //    Directory.CreateDirectory(path + "/PaintBooth drawing");
            //string f = $"{path}/PaintBooth drawing/{name} {DateTime.Now:hh - mm}.dwg";
            //new WriteAutodesk(new WriteAutodeskParams(d), f).DoWork();
            return d;
        }
        
        // vertical ISA: foot (y ya..yb) flat on wall face at x = xw, leg sticks out sx*A at the ya or yb end
        private static void GuideAngle(DesignDocument d, double xw, double sx, double ya, double yb, bool legHigh,
                                       double z0, double z1, double A, double t)
        {
            Point3D P(double x, double y) => new Point3D(x, y, z0);
            var p = legHigh
                ? new[] { P(xw, ya), P(xw, yb), P(xw + sx * A, yb), P(xw + sx * A, yb - t),
                          P(xw + sx * t, yb - t), P(xw + sx * t, ya), P(xw, ya) }
                : new[] { P(xw, ya), P(xw, yb), P(xw + sx * t, yb), P(xw + sx * t, ya + t),
                          P(xw + sx * A, ya + t), P(xw + sx * A, ya), P(xw, ya) };
            d.Entities.Add(new devDept.Eyeshot.Entities.Region(new LinearPath(p))
                           .ExtrudeAsBrep(new Vector3D(0, 0, z1 - z0)), Color.SteelBlue);
        }
        private static void TubeX(DesignDocument d, double x0, double x1, double y, double z, double od, double w)
        {
            var pl = new Plane(new Point3D(x0, y, z), Vector3D.AxisX);
            var r = new devDept.Eyeshot.Entities.Region(new Circle(pl, od / 2), new Circle(pl, od / 2 - w));
            d.Entities.Add(r.ExtrudeAsBrep(new Vector3D(x1 - x0, 0, 0)), Color.Silver);
        }
        private static void RodZ(DesignDocument d, double x, double y, double z0, double z1, double dia)
        {
            var r = new devDept.Eyeshot.Entities.Region(new Circle(Plane.XY, new Point3D(x, y, z0), dia / 2));
            d.Entities.Add(r.ExtrudeAsBrep(new Vector3D(0, 0, z1 - z0)), Color.Silver);
        }

        private static void RodX(DesignDocument d, double x0, double x1, double y, double z, double dia)
        {
            var r = new devDept.Eyeshot.Entities.Region(
                new Circle(new Plane(new Point3D(x0, y, z), Vector3D.AxisX), dia / 2));
            d.Entities.Add(r.ExtrudeAsBrep(new Vector3D(x1 - x0, 0, 0)), Color.Silver);
        }
        private static void FlangeX(DesignDocument d, double xFace, double dx, double y, double z, double pipeOD,
                                    double flOD, double pcd, double hole, int n)
        {
            var pl = new Plane(new Point3D(xFace, y, z), Vector3D.AxisX);
            var c = new List<ICurve> { new Circle(pl, flOD / 2), new Circle(pl, pipeOD / 2) };
            for (int i = 0; i < n; i++)
            {
                double a = Math.PI / n + 2 * Math.PI * i / n;
                c.Add(new Circle(new Plane(new Point3D(xFace, y + pcd / 2 * Math.Cos(a), z + pcd / 2 * Math.Sin(a)),
                                           Vector3D.AxisX), hole / 2));
            }
            d.Entities.Add(new devDept.Eyeshot.Entities.Region(c.ToArray())
                           .ExtrudeAsBrep(new Vector3D(dx, 0, 0)), Color.Silver);
        }

        private static void Box(DesignDocument d, double x0, double x1, double y0, double y1,
                                double z0, double z1, Color c)
        {
            var r = new devDept.Eyeshot.Entities.Region(new LinearPath(new[]
            {
                new Point3D(x0, y0, z0), new Point3D(x1, y0, z0),
                new Point3D(x1, y1, z0), new Point3D(x0, y1, z0), new Point3D(x0, y0, z0)
            }));
            d.Entities.Add(r.ExtrudeAsBrep(new Vector3D(0, 0, z1 - z0)), c);
        }

        // strainer in XZ plane centred on y = yc: 6 thk hemmed rim, mesh panel, bent-strip handle at top centre
        private static void Strainer(DesignDocument d, double x0, double W, double yc, double z0, double z1,
                                     double T, double rim, Color mesh)
        {
            double y0 = yc - T / 2, x1 = x0 + W;

            // rim frame (outer 480 x 320, 20 wide, 6 thk)
            Point3D P(double x, double z) => new Point3D(x, y0, z);
            var outer = new LinearPath(new[] { P(x0, z0), P(x1, z0), P(x1, z1), P(x0, z1), P(x0, z0) });
            var inner = new LinearPath(new[] { P(x0 + rim, z0 + rim), P(x1 - rim, z0 + rim),
                                               P(x1 - rim, z1 - rim), P(x0 + rim, z1 - rim), P(x0 + rim, z0 + rim) });
            d.Entities.Add(new devDept.Eyeshot.Entities.Region(outer, inner)
                           .ExtrudeAsBrep(new Vector3D(0, T, 0)), Color.Silver);

            // wire mesh (shown as 1 thk see-through panel)
            Box(d, x0 + rim, x1 - rim, yc - 0.5, yc + 0.5, z0 + rim, z1 - rim, mesh);

            // handle: 17 lip | 30 deep x 100 grip | 17 lip, 15 wide strip, 173 from left, on -Y face   (CHECK face)
            double ht = 1.6, hz0 = z1 - 15, xa = x0 + 173, yF = y0;
            Box(d, xa, xa + 17, yF - ht, yF, hz0, z1, Color.Silver);   // lip
            Box(d, xa + 117, xa + 134, yF - ht, yF, hz0, z1, Color.Silver);   // lip
            Box(d, xa + 17, xa + 17 + ht, yF - 30, yF, hz0, z1, Color.Silver);   // leg
            Box(d, xa + 117 - ht, xa + 117, yF - 30, yF, hz0, z1, Color.Silver);   // leg
            Box(d, xa + 17, xa + 117, yF - 30, yF - 30 + ht, hz0, z1, Color.Silver);  // grip
        }
    }
}