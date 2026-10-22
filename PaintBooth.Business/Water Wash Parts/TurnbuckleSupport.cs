using devDept.Eyeshot;
using devDept.Eyeshot.Entities;
using devDept.Eyeshot.Translators;
using devDept.Geometry;
using Microsoft.Extensions.Configuration;
using PaintBooth.Models;
using System.Drawing;

namespace PaintBooth.Business.Water_Wash_Parts
{
    // Distributor plate lifting arrangement (client detail).
    // ISA 30x30x3 pair on tank - eye bolt L.H. - height adjuster - eye bolt R.H. - ISA pair under plate.
    // Origin = axis on tank bottom, +Z up. totalL = tank bottom -> plate underside.
    public class TurnbuckleSupport
    {
        private readonly IConfiguration _configuration;
        public TurnbuckleSupport(IConfiguration configuration) { _configuration = configuration; }

        public const double MinL = 160, MaxL = 214;   // adjustment range with these parts

        public DesignDocument DrawTurnbuckleSupport(PaintBoothModel model, string name, double totalL)
        {
            DesignDocument design = new DesignDocument();
            design.Units = linearUnitsType.Millimeters;
            double L = totalL;

            // ---- client dimensions ----
            double tabT = 5, eyeR = 10, hole = 10, tabStraight = 15;    // eye bolt: 25 tab, hole 10 from end
            double rodD = 10, holeToTip = 60;                            // ACME Ø10, hole centre -> shank tip
            double angT = 3, angLeg = 30, angW = 20, angHole = 20;      // ISA 30x30x3, hole at 20
            double adjL = 80, barT = 3, plGap = 20, plT = 8, plIn = 5;  // height adjuster
            double af = 17, headH = 7, nutH = 8;                         // M10 hex

            // ================= 1 + 2. END SETS: angle pair + eye bolt + M10 bolt =================
            void End(double zBase, int dir, bool turn90)
            {
                int first = design.Entities.Count;

                // angle pair, back to back
                foreach (int s in new[] { +1, -1 })
                {
                    double y = s * tabT / 2;
                    var leg = new devDept.Eyeshot.Entities.Region(
                        Eye(zBase + dir * angHole, zBase, angW / 2, y),
                        HoleY(y, zBase + dir * angHole, hole));
                    design.Entities.Add(leg.ExtrudeAsBrep(new Vector3D(0, s * angT, 0)), Color.SteelBlue);
                    AddBox(design, -angW / 2, angW / 2, y, y + s * angLeg, zBase, zBase + dir * angT, Color.SteelBlue);
                }

                // eye bolt: tab + threaded shank
                double zc = zBase + dir * angHole;
                double y0 = -tabT / 2;
                var tab = new devDept.Eyeshot.Entities.Region(
                    Eye(zc, zc + dir * tabStraight, eyeR, y0), HoleY(y0, zc, hole));
                design.Entities.Add(tab.ExtrudeAsBrep(new Vector3D(0, tabT, 0)), Color.Silver);
                AddRod(design, zc + dir * tabStraight, zc + dir * holeToTip, rodD, 1.25);

                // M10 bolt + nut
                double yIn = tabT / 2 + angT;
                Hex(design, -yIn - headH, -yIn, zc, af, Color.Gray);
                RodY(design, -yIn, yIn + nutH + 3, zc, 10, Color.Gray);
                Hex(design, yIn, yIn + nutH, zc, af, Color.Gray);

                // client: bottom set runs 90° to the top set
                if (turn90)
                {
                    var rot = new Rotation(Math.PI / 2, Vector3D.AxisZ, Point3D.Origin);
                    for (int n = first; n < design.Entities.Count; n++)
                        design.Entities[n].TransformBy(rot);
                }
            }

            End(0, +1, turn90: true);     // on tank bottom - L.H. eye bolt
            End(L, -1, turn90: false);    // under distributor plate - R.H. eye bolt

            // ================= 3. HEIGHT ADJUSTER (centred) =================
            double a = L / 2 - adjL / 2;
            foreach (int s in new[] { +1, -1 })                                       // 3T side bars
                AddBox(design, s * plGap / 2, s * (plGap / 2 + barT), -plGap / 2, plGap / 2, a, a + adjL, Color.DimGray);
            foreach (double z0 in new[] { a + plIn, a + adjL - plIn - plT })         // 20x20x8 threaded plates
            {
                var sq = new devDept.Eyeshot.Entities.Region(
                    Rect(-plGap / 2, plGap / 2, -plGap / 2, plGap / 2, z0),
                    new Circle(Plane.XY, new Point3D(0, 0, z0), hole / 2));
                design.Entities.Add(sq.ExtrudeAsBrep(new Vector3D(0, 0, plT)), Color.DimGray);
            }

            // ================= Export DWG =================
            var path = _configuration["FolderPathConfig:DrawingPath"].ToString();
            if (!Directory.Exists(path + "/PaintBooth drawing"))
                Directory.CreateDirectory(path + "/PaintBooth drawing");
            string dwgFilePath = $"{path}/PaintBooth drawing/{name} {DateTime.Now:hh - mm}.dwg";
            new WriteAutodesk(new WriteAutodeskParams(design), dwgFilePath).DoWork();

            return design;
        }

        // flat tab outline in XZ plane at y: straight from centre zc to zEnd, R-round on the far side
        private static LinearPath Eye(double zc, double zEnd, double r, double y)
        {
            double s = Math.Sign(zEnd - zc);
            var p = new List<Point3D> { new Point3D(r, y, zc), new Point3D(r, y, zEnd),
                                        new Point3D(-r, y, zEnd), new Point3D(-r, y, zc) };
            int n = 16;
            for (int i = 1; i < n; i++)
            {
                double t = Math.PI - Math.PI * i / n;
                p.Add(new Point3D(r * Math.Cos(t), y, zc - s * r * Math.Sin(t)));
            }
            p.Add(p[0]);
            return new LinearPath(p.ToArray());
        }

        private static Circle HoleY(double y, double z, double d) =>
            new Circle(new Plane(new Point3D(0, y, z), Vector3D.AxisY), d / 2);

        private static LinearPath Rect(double x0, double x1, double y0, double y1, double z) =>
            new LinearPath(new[] { new Point3D(x0, y0, z), new Point3D(x1, y0, z),
                                   new Point3D(x1, y1, z), new Point3D(x0, y1, z), new Point3D(x0, y0, z) });

        // hex prism along Y (bolt head / nut)
        private static void Hex(DesignDocument d, double y0, double y1, double z, double af, Color c)
        {
            double r = af / Math.Sqrt(3);
            var p = new List<Point3D>();
            for (int i = 0; i <= 6; i++)
            {
                double t = Math.PI / 6 + i * Math.PI / 3;
                p.Add(new Point3D(r * Math.Cos(t), y0, z + r * Math.Sin(t)));
            }
            var reg = new devDept.Eyeshot.Entities.Region(new LinearPath(p.ToArray()));
            d.Entities.Add(reg.ExtrudeAsBrep(new Vector3D(0, y1 - y0, 0)), c);
        }

        private static void RodY(DesignDocument d, double y0, double y1, double z, double dia, Color c)
        {
            var reg = new devDept.Eyeshot.Entities.Region(HoleY(y0, z, dia));
            d.Entities.Add(reg.ExtrudeAsBrep(new Vector3D(0, y1 - y0, 0)), c);
        }

        // rod on Z axis from zFlat to chamfered zTip
        private static void AddRod(DesignDocument design, double zFlat, double zTip, double d, double c)
        {
            double r = d / 2, s = Math.Sign(zTip - zFlat);
            var profile = new LinearPath(new[]
            {
                new Point3D(0, 0, zFlat), new Point3D(r, 0, zFlat),
                new Point3D(r, 0, zTip - s * c), new Point3D(r - c, 0, zTip),
                new Point3D(0, 0, zTip), new Point3D(0, 0, zFlat)
            });
            var rod = new devDept.Eyeshot.Entities.Region(profile)
                          .RevolveAsBrep(2 * Math.PI, Vector3D.AxisZ, Point3D.Origin, 0.01);
            design.Entities.Add(rod, Color.Silver);
        }

        private static void AddBox(DesignDocument design, double x0, double x1, double y0, double y1,
                                   double z0, double z1, Color c)
        {
            var r = new devDept.Eyeshot.Entities.Region(Rect(Math.Min(x0, x1), Math.Max(x0, x1),
                                                             Math.Min(y0, y1), Math.Max(y0, y1), z0));
            design.Entities.Add(r.ExtrudeAsBrep(new Vector3D(0, 0, z1 - z0)), c);
        }
    }
}