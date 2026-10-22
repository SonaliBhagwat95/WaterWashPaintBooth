using devDept.Eyeshot;
using devDept.Eyeshot.Entities;
using devDept.Eyeshot.Translators;
using devDept.Geometry;
using Microsoft.Extensions.Configuration;
using PaintBooth.Models;
using System.Drawing;

namespace PaintBooth.Business.Water_Wash_Parts
{
    // Grating support frame: long 75x40x3T box along Y with 50x5 vertical flat,
    // 5x separators 50x10x5 at joints, cross supports (594) along X at each joint,
    // drop-in joints: End Plate 1 (slots) on box ends, End Plate 2 (M10x30 studs) on mating face.
    // Local: Y = 0..L along width; X = 0 at inner face of vertical flat; Z = 0 box bottom, 40 box top.
    public class GratingSupport
    {
        private readonly IConfiguration _configuration;
        public GratingSupport(IConfiguration configuration) { _configuration = configuration; }

        const double BW = 75, BH = 40, T = 3;           // box section 75 x 40, 3T
        const double PT = 3;                             // end plate thk
        const double C1 = 22.5, C2 = 52.5, HZ = 20;      // stud / slot centres across 75, height 20
        const double StudD = 10, StudL = 30;             // M10 x 30 stud

        public DesignDocument DrawGratingSupport(PaintBoothModel model, string name, double L, double[] jointY,
                                                 double crossL = 594)
        {
            DesignDocument design = new DesignDocument();
            design.Units = linearUnitsType.Millimeters;

            // ================= 1. LONG SUPPORT along Y =================
            // map (u across 0..75 = X, v up = Z) at a given y
            Func<double, double, Point3D> AtY(double y) => (u, v) => new Point3D(u, y, v);

            HollowBox(design, AtY(2 * PT), new Vector3D(0, L - 4 * PT, 0));          // box body
            EndJoint(design, AtY(0), AtY(PT), new Vector3D(0, +1, 0));   // Y = 0 end
            EndJoint(design, AtY(L - PT), AtY(L - 2 * PT), new Vector3D(0, -1, 0));   // Y = L end

            // vertical flat 50 W x 5 T x 3994 L (stands 10 above box)
            double fL = 3994, fy0 = (L - fL) / 2;
            Box(design, -5, 0, fy0, fy0 + fL, 0, 50);

            // ================= 2. SEPARATORS + CROSS SUPPORTS at each joint =================
            double crossTot = crossL + 2 * PT;                                        // 594 + plate 2 each end = 600
            foreach (double yc in jointY)
            {
                // separator 50 x 10 x 5 on top of box (5 gap between gratings)
                Box(design, 0, 50, yc - 2.5, yc + 2.5, BH, BH + 10);

                // cross support along X, from long box side face (X = 75)
                Func<double, double, Point3D> AtX(double x) => (u, v) => new Point3D(x, yc - BW / 2 + u, v);
                double x0 = BW;
                HollowBox(design, AtX(x0 + 2 * PT), new Vector3D(crossTot - 4 * PT, 0, 0));
                EndJoint(design, AtX(x0), AtX(x0 + PT), new Vector3D(+1, 0, 0));
                EndJoint(design, AtX(x0 + crossTot - PT), AtX(x0 + crossTot - 2 * PT), new Vector3D(-1, 0, 0));
            }
            
            // ================= Export DWG =================
            var path = _configuration["FolderPathConfig:DrawingPath"].ToString();
            if (!Directory.Exists(path + "/PaintBooth drawing"))
                Directory.CreateDirectory(path + "/PaintBooth drawing");
            string dwgFilePath = $"{path}/PaintBooth drawing/{name} {DateTime.Now:hh - mm}.dwg";
            new WriteAutodesk(new WriteAutodeskParams(design), dwgFilePath).DoWork();

            return design;
        }

        // ---- 75 x 40 x 3T hollow box: section at 'map', extruded along dir ----
        private static void HollowBox(DesignDocument d, Func<double, double, Point3D> map, Vector3D dir)
        {
            var outer = Loop(map, (0, 0), (BW, 0), (BW, BH), (0, BH));
            var inner = Loop(map, (T, T), (BW - T, T), (BW - T, BH - T), (T, BH - T));
            var r = new devDept.Eyeshot.Entities.Region(outer, inner);
            d.Entities.Add(r.ExtrudeAsBrep(dir), Color.SteelBlue);
        }

        // ---- drop-in joint: End Plate 2 (studs) on mating face + End Plate 1 (slots) on box end ----
        // p2 = outer face of plate 2, p1 = outer face of plate 1, inward = toward box body
        private static void EndJoint(DesignDocument d, Func<double, double, Point3D> p2,
                                     Func<double, double, Point3D> p1, Vector3D inward)
        {
            // End Plate 2: 75 x 40 x 3T
            var pl2 = new devDept.Eyeshot.Entities.Region(Loop(p2, (0, 0), (BW, 0), (BW, BH), (0, BH)));
            d.Entities.Add(pl2.ExtrudeAsBrep(inward * PT), Color.SteelBlue);

            // M10 x 30 studs, stud welded on plate 2, through plate 1 slots into box
            foreach (double c in new[] { C1, C2 })
            {
                var sp = new Plane(p2(c, HZ) + inward * PT, inward);
                var st = new devDept.Eyeshot.Entities.Region(new Circle(sp, StudD / 2));
                d.Entities.Add(st.ExtrudeAsBrep(inward * StudL), Color.Silver);
            }

            // End Plate 1: 75 x 40 x 3T with two slots 10 wide, 25 deep from bottom (R5 top)
            var pts = new List<(double, double)> { (0, 0) };
            foreach (double c in new[] { C1, C2 })
            {
                pts.Add((c - 5, 0));
                pts.Add((c - 5, HZ));
                for (int i = 1; i < 8; i++)                         // R5 slot end
                {
                    double a = Math.PI - Math.PI * i / 8;
                    pts.Add((c + 5 * Math.Cos(a), HZ + 5 * Math.Sin(a)));
                }
                pts.Add((c + 5, HZ));
                pts.Add((c + 5, 0));
            }
            pts.Add((BW, 0)); pts.Add((BW, BH)); pts.Add((0, BH));
            var pl1 = new devDept.Eyeshot.Entities.Region(Loop(p1, pts.ToArray()));
            d.Entities.Add(pl1.ExtrudeAsBrep(inward * PT), Color.SteelBlue);
        }

        private static LinearPath Loop(Func<double, double, Point3D> map, params (double u, double v)[] uv)
        {
            var p = uv.Select(q => map(q.u, q.v)).ToList();
            p.Add(p[0]);
            return new LinearPath(p.ToArray());
        }

        private static void Box(DesignDocument d, double x0, double x1, double y0, double y1, double z0, double z1)
        {
            var r = new devDept.Eyeshot.Entities.Region(new LinearPath(new[]
            {
                new Point3D(x0, y0, z0), new Point3D(x1, y0, z0),
                new Point3D(x1, y1, z0), new Point3D(x0, y1, z0), new Point3D(x0, y0, z0)
            }));
            d.Entities.Add(r.ExtrudeAsBrep(new Vector3D(0, 0, z1 - z0)), Color.SteelBlue);
        }
    }
}