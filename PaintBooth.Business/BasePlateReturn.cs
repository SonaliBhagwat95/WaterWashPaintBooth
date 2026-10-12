using devDept.Eyeshot;
using devDept.Eyeshot.Entities;
using devDept.Eyeshot.Translators;
using devDept.Geometry;
using Microsoft.Extensions.Configuration;
using PaintBooth.Models;
using System.Drawing;
using Region = devDept.Eyeshot.Entities.Region;

namespace PaintBooth.Business
{
    public class BasePlateReturn
    {
        private readonly IConfiguration _configuration;

        public BasePlateReturn(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        public DesignDocument DrawBasePlateReturn(PaintBoothModel model, double zBottom)
        {
            DesignDocument design = new DesignDocument();
            design.Units = linearUnitsType.Millimeters;

            // ---- Inputs (mm) ----
            double T = 1.6;    // sheet thickness
            double TotalX = 347;    // overall length
            double DuctX = 128;    // vertical leg width (outer)
            double OutletH = 184;    // outlet height (outer)
            double TopZ = 368;    // top-left height
            double Rout = 180;    // outer bend radius
            double Rin = 127;    // inner bend radius
            double InStr = 92;     // straight after inner bend
            double Depth = 131 + 2 * T;                      // Y depth (131 I/S)
            double SlopeDeg = Math.Atan2(181, 682) * 180 / Math.PI; // 14.86° - same as BasePlate1
            int Seg = 36;     // arc segments
            double H = 460;   // overall height

            // Top flange plate
            double PlateW = 177;   // Y width
            double TabLen = 25;    // tab beyond inner wall (along slope)
            double HoleDia = 8;
            double SideHoleE = 9;     // side holes from Y edge (159 pitch)
            double SideHoleS = 27;    // side holes along slope from left edge  (CHECK)
            double TabPitch = 76;    // tab holes pitch in Y
            double TabHoleS = 12.5;  // tab holes from inner wall along slope (CHECK)

            double a = SlopeDeg * Math.PI / 180.0;
            double tanA = Math.Tan(a), cosA = Math.Cos(a), sinA = Math.Sin(a);
            Func<double, double> zTop = x => TopZ + x * tanA;

            // centres
            double icx = TotalX - InStr, icz = OutletH + Rin;   // inner bend centre (255, 311)
            double ocx = Rout, ocz = Rout;            // outer bend centre (180, 180)

            // ================= 1. SIDE CHEEKS (XZ -> Y) =================
            var cheek = new List<Point3D>();
            cheek.Add(P(TotalX, 0));
            cheek.Add(P(TotalX, OutletH));
            cheek.Add(P(icx, OutletH));
            cheek.AddRange(ArcPts(icx, icz, Rin, 270, 180, Seg, true));   // ends at (128, 311)
            cheek.Add(P(DuctX, zTop(DuctX)));
            cheek.Add(P(0, TopZ));
            cheek.Add(P(0, ocz));
            cheek.AddRange(ArcPts(ocx, ocz, Rout, 180, 270, Seg, true));  // ends at (180, 0)
            cheek.Add(P(TotalX, 0));                                      // close

            Region cheekRegion = new Region(new LinearPath(cheek.ToArray()));
            Brep cheekFront = cheekRegion.ExtrudeAsBrep(new Vector3D(0, T, 0));
            Brep cheekBack = cheekRegion.ExtrudeAsBrep(new Vector3D(0, T, 0));
            cheekBack.Translate(0, Depth - T, 0);
            design.Entities.Add(cheekFront, Color.BurlyWood);
            design.Entities.Add(cheekBack, Color.BurlyWood);

            // ================= 2. OUTER WRAPPER (R180 + bottom) =================
            var ow = new List<Point3D>();
            ow.Add(P(0, TopZ));
            ow.Add(P(0, ocz));
            ow.AddRange(ArcPts(ocx, ocz, Rout, 180, 270, Seg, true));
            ow.Add(P(TotalX, 0));
            ow.Add(P(TotalX, T));
            ow.Add(P(ocx, T));
            ow.AddRange(ArcPts(ocx, ocz, Rout - T, 270, 180, Seg, true));
            ow.Add(P(T, zTop(T)));
            ow.Add(P(0, TopZ));                                           // close

            Region owRegion = new Region(new LinearPath(ow.ToArray()));
            Brep outerWrap = owRegion.ExtrudeAsBrep(new Vector3D(0, Depth - 2 * T, 0));
            outerWrap.Translate(0, T, 0);
            design.Entities.Add(outerWrap, Color.BurlyWood);

            // ================= 3. INNER WRAPPER (R127 + 92 straight) =================
            var iw = new List<Point3D>();
            iw.Add(P(TotalX, OutletH));
            iw.Add(P(icx, OutletH));
            iw.AddRange(ArcPts(icx, icz, Rin, 270, 180, Seg, true));
            iw.Add(P(DuctX, zTop(DuctX)));
            iw.Add(P(DuctX - T, zTop(DuctX - T)));
            iw.Add(P(DuctX - T, icz));
            iw.AddRange(ArcPts(icx, icz, Rin + T, 180, 270, Seg, true));
            iw.Add(P(TotalX, OutletH - T));
            iw.Add(P(TotalX, OutletH));                                   // close

            Region iwRegion = new Region(new LinearPath(iw.ToArray()));
            Brep innerWrap = iwRegion.ExtrudeAsBrep(new Vector3D(0, Depth - 2 * T, 0));
            innerWrap.Translate(0, T, 0);
            design.Entities.Add(innerWrap, Color.BurlyWood);

            // ================= 4. TOP FLANGE PLATE (on slope) =================
            double side = (PlateW - Depth) / 2.0;                         // overhang each side in Y
            Point3D O = new Point3D(0, -side, TopZ);                    // plate origin (top-left, front)
            Vector3D ex = new Vector3D(cosA, 0, sinA);                    // along slope
            Vector3D ey = Vector3D.AxisY;
            Vector3D nz = new Vector3D(-sinA, 0, cosA);                   // plate normal (up)
            Func<double, double, Point3D> L = (s, w) =>
                new Point3D(O.X + ex.X * s + ey.X * w, O.Y + ex.Y * s + ey.Y * w, O.Z + ex.Z * s + ey.Z * w);

            double Ls = DuctX / cosA + TabLen;                           // plate length along slope
            double sWall = DuctX / cosA;                                  // inner wall position along slope

            LinearPath plateOuter = new LinearPath(new Point3D[]
            {
                L(0, 0), L(Ls, 0), L(Ls, PlateW), L(0, PlateW), L(0, 0)
            });

            // duct opening (inside of duct)
            double s1 = T / cosA, s2 = (DuctX - T) / cosA;
            double w1 = side + T, w2 = side + Depth - T;
            LinearPath opening = new LinearPath(new Point3D[]
            {
                L(s1, w1), L(s2, w1), L(s2, w2), L(s1, w2), L(s1, w1)
            });

            Plane slopePlane = new Plane(O, ex, ey);
            Circle Hole(double s, double w)
            {
                Point3D c = L(s, w);
                return new Circle(new Plane(c, ex, ey), c, HoleDia / 2);
            }

            Region plateRegion = new Region(
                plateOuter,
                opening,
                Hole(SideHoleS, SideHoleE),
                Hole(SideHoleS, PlateW - SideHoleE),
                Hole(sWall + TabHoleS, PlateW / 2 - TabPitch / 2),
                Hole(sWall + TabHoleS, PlateW / 2 + TabPitch / 2));

            Brep topPlate = plateRegion.ExtrudeAsBrep(new Vector3D(nz.X * T, 0, nz.Z * T));
           
            design.Entities.Add(topPlate, Color.Gray);

            // ================= Export DWG =================
            var path = _configuration["FolderPathConfig:DrawingPath"].ToString();

            if (!Directory.Exists(path + "/PaintBooth drawing"))
                Directory.CreateDirectory(path + "/PaintBooth drawing");

            string dwgFilePath = $"{path}/PaintBooth drawing/BasePlateReturn {DateTime.Now:hh - mm}.dwg";

            WriteAutodeskParams auto = new WriteAutodeskParams(design);
            WriteAutodesk dwgg1 = new WriteAutodesk(auto, dwgFilePath);
            dwgg1.DoWork();

            return design;
        }

        // ---- Helpers (XZ plane, Y = 0) ----
        private static Point3D P(double x, double z) => new Point3D(x, 0, z);

        private static List<Point3D> ArcPts(double cx, double cz, double r,
                                            double startDeg, double endDeg, int seg, bool skipFirst)
        {
            var pts = new List<Point3D>();
            for (int i = skipFirst ? 1 : 0; i <= seg; i++)
            {
                double ang = (startDeg + (endDeg - startDeg) * i / seg) * Math.PI / 180.0;
                pts.Add(new Point3D(cx + r * Math.Cos(ang), 0, cz + r * Math.Sin(ang)));
            }
            return pts;
        }
    }
}
