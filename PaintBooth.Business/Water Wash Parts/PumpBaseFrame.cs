using devDept.Eyeshot;
using devDept.Eyeshot.Entities;
using devDept.Eyeshot.Translators;
using devDept.Geometry;
using Microsoft.Extensions.Configuration;
using PaintBooth.Models;
using System.Drawing;

namespace PaintBooth.Business.Water_Wash_Parts
{
    // Base frame for pump mounting, 260 x 450 x 70, 3T M.S.
    // Local: X = 0..260 across, Y = 0..450 along, Z = 0 floor .. 70 top.
    public class PumpBaseFrame
    {
        private readonly IConfiguration _configuration;
        public PumpBaseFrame(IConfiguration configuration) { _configuration = configuration; }

        public DesignDocument DrawPumpBaseFrame(PaintBoothModel model, string name)
        {
            DesignDocument d = new DesignDocument();
            d.Units = linearUnitsType.Millimeters;

            double W = 260, L = 450, H = 70, t = 3;
            double foot = 40, box = 50;                 // foot 40 + box 50 = 90 each side, 80 gap
            double hole = 14;

            foreach (int s in new[] { 0, 1 })           // 0 = left side, 1 = right side (mirrored)
            {
                double X(double u) => s == 0 ? u : W - u;          // u from that side's outer edge

                // bottom 0..90 with foundation holes (20 from outer edge, 25 from ends)
                Plate(d, X(0), X(foot + box), 0, L, 0, t,
                      new[] { (X(20), 25.0), (X(20), L - 25) }, hole);

                // inner web at 90, 70 high
                Plate(d, X(foot + box - t), X(foot + box), 0, L, 0, H, null, 0);

                // top flange 90 → 40 (open outward), pump hole 20 from its outer edge, 130 from end
                Plate(d, X(foot), X(foot + box), 0, L, H - t, H,
                      new[] { (X(foot + 20), 130.0) }, hole);
            }

            // end plates 3T both ends, across both channels + 80 gap (40 .. 220), full height
            Plate(d, foot, W - foot, -t, 0, 0, H, null, 0);
            Plate(d, foot, W - foot, L, L + t, 0, H, null, 0);

            // ---- Export DWG ----
            var path = _configuration["FolderPathConfig:DrawingPath"].ToString();
            if (!Directory.Exists(path + "/PaintBooth drawing"))
                Directory.CreateDirectory(path + "/PaintBooth drawing");
            string f = $"{path}/PaintBooth drawing/{name} {DateTime.Now:hh - mm}.dwg";
            new WriteAutodesk(new WriteAutodeskParams(d), f).DoWork();
            return d;
        }

        // flat plate x0..x1 (any order) × y0..y1 × z0..z1, optional vertical holes
        private static void Plate(DesignDocument d, double x0, double x1, double y0, double y1,
                                  double z0, double z1, (double x, double y)[] holes, double dia)
        {
            double xa = Math.Min(x0, x1), xb = Math.Max(x0, x1);
            var c = new List<ICurve>
            {
                new LinearPath(new[]
                {
                    new Point3D(xa, y0, z0), new Point3D(xb, y0, z0),
                    new Point3D(xb, y1, z0), new Point3D(xa, y1, z0), new Point3D(xa, y0, z0)
                })
            };
            if (holes != null)
                foreach (var (hx, hy) in holes)
                    c.Add(new Circle(Plane.XY, new Point3D(hx, hy, z0), dia / 2));

            d.Entities.Add(new devDept.Eyeshot.Entities.Region(c.ToArray())
                           .ExtrudeAsBrep(new Vector3D(0, 0, z1 - z0)), Color.DimGray);
        }
    }
}