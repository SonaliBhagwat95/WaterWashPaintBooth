using devDept.Eyeshot;
using devDept.Eyeshot.Entities;
using devDept.Eyeshot.Translators;
using devDept.Geometry;
using Microsoft.Extensions.Configuration;
using PaintBooth.Models;
using System.Drawing;

namespace PaintBooth.Business.Water_Wash_Parts
{
    public class DeliveryPipe
    {
        private readonly IConfiguration _configuration;
        public DeliveryPipe(IConfiguration configuration) { _configuration = configuration; }

        // Vertical pipe along -Z. Origin = axis at top face of top flange.
        // Delivery Pipe 3: DrawDeliveryPipe(model, "DeliveryPipe3", 1516, 88.9, 683)
        public DesignDocument DrawDeliveryPipe(PaintBoothModel model, string name,
                                               double upperL, double reducerL, double lowerL)
        {
            DesignDocument design = new DesignDocument();
            design.Units = linearUnitsType.Millimeters;

            // ---- Pipe sizes (ASME OD, Sch 10S) ----
            double OD65 = 73.0, W65 = 3.05;     // 65NB                     (CHECK wall)
            double OD50 = 60.3, W50 = 2.77;     // 50NB                     (CHECK wall)

            // ---- BS10 Table D slip-on flanges ----                         (CHECK all)
            double Fl65OD = 165.1, Fl65PCD = 127.0, Fl65T = 12.7;
            double Fl50OD = 152.4, Fl50PCD = 114.3, Fl50T = 12.7;
            double BoltHole = 17.5;
            int NBolts = 4;

            double z1 = -upperL;                 // reducer top (65NB end)
            double z2 = z1 - reducerL;           // reducer bottom (50NB end)
            double z3 = z2 - lowerL;             // bottom face of bottom flange

            // ================= 1. PIPES =================
            AddTube(design, 0, z1, OD65, W65);          // 65NB, top flange face -> reducer
            AddTube(design, z2, z3, OD50, W50);          // 50NB, reducer -> bottom flange face

            // ================= 2. REDUCER (outer + inner skin) =================
            AddCone(design, z1, OD65 / 2, z2, OD50 / 2);
            AddCone(design, z1, OD65 / 2 - W65, z2, OD50 / 2 - W50);

            // ================= 3. SLIP-ON FLANGES =================
            AddFlange(design, 0, -Fl65T, OD65, Fl65OD, Fl65PCD, BoltHole, NBolts);   // top, 65NB
            AddFlange(design, z3, +Fl50T, OD50, Fl50OD, Fl50PCD, BoltHole, NBolts);   // bottom, 50NB

            // ================= Export DWG =================
            var path = _configuration["FolderPathConfig:DrawingPath"].ToString();
            if (!Directory.Exists(path + "/PaintBooth drawing"))
                Directory.CreateDirectory(path + "/PaintBooth drawing");
            string dwgFilePath = $"{path}/PaintBooth drawing/{name} {DateTime.Now:hh - mm}.dwg";
            new WriteAutodesk(new WriteAutodeskParams(design), dwgFilePath).DoWork();

            return design;
        }
        // L-shaped pipe. Origin = axis at bottom face of bottom flange.
        // Vertical leg up +Z, 90° bend, horizontal leg along +X.
        // Delivery Pipe 4: DrawDeliveryPipe4(model, "DeliveryPipe4", 430, 279, 95.25)
        public DesignDocument DrawDeliveryPipe4(PaintBoothModel model, string name,
                                                double vertL, double horizL, double bendR)
        {
            DesignDocument design = new DesignDocument();
            design.Units = linearUnitsType.Millimeters;

            // ---- 65NB, Sch 10S ----                                         (CHECK wall)
            double OD65 = 73.0, W65 = 3.05;

            // ---- BS10 Table D slip-on flange, 65NB ----                     (CHECK)
            double Fl65OD = 165.1, Fl65PCD = 127.0, Fl65T = 12.7;
            double BoltHole = 17.5;
            int NBolts = 4;

            double zBend = vertL - bendR;        // vertical straight ends / bend starts

            // ================= 1. VERTICAL LEG =================
            AddTube(design, zBend, 0, OD65, W65);                                    // 0 -> zBend
            AddFlange(design, 0, +Fl65T, OD65, Fl65OD, Fl65PCD, BoltHole, NBolts);   // bottom flange

            // ================= 2. 90° BEND (up -> +X) =================
            Point3D c = new Point3D(0, 0, zBend);
            var ring = new devDept.Eyeshot.Entities.Region(
                new Circle(Plane.XY, c, OD65 / 2), new Circle(Plane.XY, c, OD65 / 2 - W65));
            var bend = ring.RevolveAsBrep(Math.PI / 2, Vector3D.AxisY,
                                          new Point3D(bendR, 0, zBend), 0.01);
            design.Entities.Add(bend, Color.Silver);

            // ================= 3. HORIZONTAL LEG =================
            // build along +Z from origin, then rotate +90° about Y (+Z -> +X), lift to vertL
            var hPipe = TubeBrep(bendR, horizL, OD65, W65);
            var hFlg = FlangeBrep(horizL, -Fl65T, OD65, Fl65OD, Fl65PCD, BoltHole, NBolts);

            foreach (var e in new Entity[] { hPipe, hFlg })
            {
                e.Rotate(Math.PI / 2, Vector3D.AxisY, Point3D.Origin);
                e.Translate(0, 0, vertL);
                design.Entities.Add(e, Color.Silver);
            }

            // ================= Export DWG =================
            var path = _configuration["FolderPathConfig:DrawingPath"].ToString();
            if (!Directory.Exists(path + "/PaintBooth drawing"))
                Directory.CreateDirectory(path + "/PaintBooth drawing");
            string dwgFilePath = $"{path}/PaintBooth drawing/{name} {DateTime.Now:hh - mm}.dwg";
            new WriteAutodesk(new WriteAutodeskParams(design), dwgFilePath).DoWork();

            return design;
        }
        // tube from zTop down to zBot
        private static void AddTube(DesignDocument design, double zTop, double zBot, double od, double wall)
        {
            Point3D c = new Point3D(0, 0, zTop);
            var ring = new devDept.Eyeshot.Entities.Region(
                new Circle(Plane.XY, c, od / 2), new Circle(Plane.XY, c, od / 2 - wall));
            design.Entities.Add(ring.ExtrudeAsBrep(new Vector3D(0, 0, zBot - zTop)), Color.Silver);
        }

        // conical skin (mesh) between two circles on the Z axis
        private static void AddCone(DesignDocument design, double zA, double rA, double zB, double rB)
        {
            int N = 48;
            var v = new List<Point3D>();
            for (int i = 0; i < N; i++)
            {
                double a = 2 * Math.PI * i / N;
                v.Add(new Point3D(rA * Math.Cos(a), rA * Math.Sin(a), zA));
            }
            for (int i = 0; i < N; i++)
            {
                double a = 2 * Math.PI * i / N;
                v.Add(new Point3D(rB * Math.Cos(a), rB * Math.Sin(a), zB));
            }
            var t = new List<IndexTriangle>();
            for (int i = 0; i < N; i++)
            {
                int a0 = i, a1 = (i + 1) % N, b0 = N + i, b1 = N + (i + 1) % N;
                t.Add(new IndexTriangle(a0, a1, b1));
                t.Add(new IndexTriangle(a0, b1, b0));
            }
            design.Entities.Add(new Mesh(v.ToArray(), t.ToArray()), Color.Silver);
        }

        // slip-on flange: disc from zFace, thickness dz (sign = direction), bore = pipe OD
        private static void AddFlange(DesignDocument design, double zFace, double dz, double pipeOD,
                                      double flOD, double pcd, double hole, int n)
        {
            Point3D c = new Point3D(0, 0, zFace);
            var curves = new List<ICurve>
            {
                new Circle(Plane.XY, c, flOD / 2),
                new Circle(Plane.XY, c, pipeOD / 2)
            };
            for (int i = 0; i < n; i++)
            {
                double a = Math.PI / n + 2 * Math.PI * i / n;          // straddle centrelines
                curves.Add(new Circle(Plane.XY, new Point3D(pcd / 2 * Math.Cos(a), pcd / 2 * Math.Sin(a), zFace), hole / 2));
            }
            var r = new devDept.Eyeshot.Entities.Region(curves.ToArray());
            design.Entities.Add(r.ExtrudeAsBrep(new Vector3D(0, 0, dz)), Color.Silver);
        }

        private static Brep TubeBrep(double zFrom, double zTo, double od, double wall)
        {
            Point3D c = new Point3D(0, 0, zFrom);
            var ring = new devDept.Eyeshot.Entities.Region(
                new Circle(Plane.XY, c, od / 2), new Circle(Plane.XY, c, od / 2 - wall));
            return ring.ExtrudeAsBrep(new Vector3D(0, 0, zTo - zFrom));
        }

        private static Brep FlangeBrep(double zFace, double dz, double pipeOD,
                                       double flOD, double pcd, double hole, int n)
        {
            Point3D c = new Point3D(0, 0, zFace);
            var curves = new List<ICurve>
            {
                new Circle(Plane.XY, c, flOD / 2),
                new Circle(Plane.XY, c, pipeOD / 2)
            };
            for (int i = 0; i < n; i++)
            {
                double a = Math.PI / n + 2 * Math.PI * i / n;
                curves.Add(new Circle(Plane.XY, new Point3D(pcd / 2 * Math.Cos(a), pcd / 2 * Math.Sin(a), zFace), hole / 2));
            }
            return new devDept.Eyeshot.Entities.Region(curves.ToArray()).ExtrudeAsBrep(new Vector3D(0, 0, dz));
        }

        // Straight 50NB spool, slip-on flange both ends.
        // Origin = axis at top face of top flange, pipe runs down -Z.
        // Delivery Pipe 2: DrawDeliveryPipe2(model, "DeliveryPipe2", 300)
        public DesignDocument DrawDeliveryPipe2(PaintBoothModel model, string name, double L)
        {
            DesignDocument design = new DesignDocument();
            design.Units = linearUnitsType.Millimeters;

            // ---- 50NB, Sch 10S ----                                         (CHECK wall)
            double OD50 = 60.3, W50 = 2.77;

            // ---- BS10 Table D slip-on flange, 50NB ----                     (CHECK)
            double Fl50OD = 152.4, Fl50PCD = 114.3, Fl50T = 12.7;
            double BoltHole = 17.5;
            int NBolts = 4;

            AddTube(design, 0, -L, OD50, W50);                                        // 300 face to face
            AddFlange(design, 0, -Fl50T, OD50, Fl50OD, Fl50PCD, BoltHole, NBolts);   // top
            AddFlange(design, -L, +Fl50T, OD50, Fl50OD, Fl50PCD, BoltHole, NBolts);   // bottom

            // ================= Export DWG =================
            var path = _configuration["FolderPathConfig:DrawingPath"].ToString();
            if (!Directory.Exists(path + "/PaintBooth drawing"))
                Directory.CreateDirectory(path + "/PaintBooth drawing");
            string dwgFilePath = $"{path}/PaintBooth drawing/{name} {DateTime.Now:hh - mm}.dwg";
            new WriteAutodesk(new WriteAutodeskParams(design), dwgFilePath).DoWork();

            return design;
        }
    }
}