using devDept.Eyeshot;
using devDept.Eyeshot.Entities;
using devDept.Eyeshot.Translators;
using devDept.Geometry;
using Microsoft.Extensions.Configuration;
using PaintBooth.Models;
using System.Drawing;

namespace PaintBooth.Business.Water_Wash_Parts
{
    // WC panel: tray W x H, 38 flanges all round with 10x12 slots at 19 from face,
    // punched angles 38x38x3 (W-10 long) tack welded inside.
    // Local: X = 0..W along panel width, Z = 0..H up, face sheet at Y = 0, flanges toward +Y (inside).
    public class WCPanel
    {
        private readonly IConfiguration _configuration;
        public WCPanel(IConfiguration configuration) { _configuration = configuration; }

        // WC Front Panel 1: DrawWCPanel(model, "WCFrontPanel1", 1160, 1676, new[] { 190.0, 701, 906 })
        public DesignDocument DrawWCFrontPanel(PaintBoothModel model, string name, double W, double H,
                                          double[] angleZ, double T = 1.6)
        {
            DesignDocument design = new DesignDocument();
            design.Units = linearUnitsType.Millimeters;

            double F = 38, slotC = 19;                        // flange depth, slot centre from face
            double sL = 12, sW = 10;                          // 10 x 12 std slot (12 along edge)
            double angA = 38, angT = 3, angL = W - 10;        // punched angle 38x38x3 x 1150

            // ================= 1. FACE SHEET =================
            design.Entities.Add(Rect(0, W, 0, H, (u, v) => new Point3D(u, 0, v))
                                .ExtrudeAsBrep(new Vector3D(0, T, 0)), Color.Green);

            // ================= 2. SIDE FLANGES (slots @200, 67 from top) =================
            var sideZ = new List<double>();
            for (double z = H - 67; z >= 150; z -= 200) sideZ.Add(z);          // 1676: 8 slots, last at 209
            foreach (double x in new[] { 0.0, W - T })
            {
                var holes = sideZ.Select(z => Loop((u, v) => new Point3D(x, u, v),
                                (slotC - sW / 2, z - sL / 2), (slotC + sW / 2, z - sL / 2),
                                (slotC + sW / 2, z + sL / 2), (slotC - sW / 2, z + sL / 2))).ToList();
                AddFlange(design, Loop((u, v) => new Point3D(x, u, v), (0, 0), (F, 0), (F, H), (0, H)),
                          holes, new Vector3D(T, 0, 0));
            }

            // ================= 3. TOP / BOTTOM FLANGES (slots @200, 80 from ends) =================
            var topX = new List<double>();
            for (double x = 80; x <= W - 80 + 0.5; x += 200) topX.Add(x);       // 1160: 6 slots
            foreach (double z in new[] { 0.0, H - T })
            {
                var holes = topX.Select(xc => Loop((u, v) => new Point3D(u, v, z),
                                (xc - sL / 2, slotC - sW / 2), (xc + sL / 2, slotC - sW / 2),
                                (xc + sL / 2, slotC + sW / 2), (xc - sL / 2, slotC + sW / 2))).ToList();
                AddFlange(design, Loop((u, v) => new Point3D(u, v, z), (0, 0), (W, 0), (W, F), (0, F)),
                          holes, new Vector3D(0, 0, T));
            }

            // ================= 4. PUNCHED ANGLES (tack welded inside) =================
            double ax = (W - angL) / 2;
            foreach (double zA in angleZ)
            {
                var L = Loop((u, v) => new Point3D(ax, u, v),
                             (T, zA), (T + angA, zA), (T + angA, zA + angT),
                             (T + angT, zA + angT), (T + angT, zA + angA), (T, zA + angA));
                design.Entities.Add(new devDept.Eyeshot.Entities.Region(L)
                                    .ExtrudeAsBrep(new Vector3D(angL, 0, 0)), Color.SteelBlue);
            }

            // ================= Export DWG =================
            var path = _configuration["FolderPathConfig:DrawingPath"].ToString();
            if (!Directory.Exists(path + "/PaintBooth drawing"))
                Directory.CreateDirectory(path + "/PaintBooth drawing");
            string dwgFilePath = $"{path}/PaintBooth drawing/{name} {DateTime.Now:hh - mm}.dwg";
            new WriteAutodesk(new WriteAutodeskParams(design), dwgFilePath).DoWork();

            return design;
        }

        private static void AddFlange(DesignDocument d, LinearPath outer, List<LinearPath> holes, Vector3D dir)
        {
            var curves = new List<ICurve> { outer };
            curves.AddRange(holes);
            var r = new devDept.Eyeshot.Entities.Region(curves.ToArray());
            d.Entities.Add(r.ExtrudeAsBrep(dir), Color.Green);
        }

        private static devDept.Eyeshot.Entities.Region Rect(double u0, double u1, double v0, double v1,
                                                            Func<double, double, Point3D> map)
            => new devDept.Eyeshot.Entities.Region(Loop(map, (u0, v0), (u1, v0), (u1, v1), (u0, v1)));

        private static LinearPath Loop(Func<double, double, Point3D> map, params (double u, double v)[] uv)
        {
            var p = uv.Select(q => map(q.u, q.v)).ToList();
            p.Add(p[0]);
            return new LinearPath(p.ToArray());
        }
    }
}