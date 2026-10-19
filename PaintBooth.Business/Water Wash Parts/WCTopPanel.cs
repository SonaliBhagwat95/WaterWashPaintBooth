using devDept.Eyeshot;
using devDept.Eyeshot.Entities;
using devDept.Eyeshot.Translators;
using devDept.Geometry;
using Microsoft.Extensions.Configuration;
using PaintBooth.Models;
using System.Drawing;

namespace PaintBooth.Business.Water_Wash_Parts
{
    public class WCTopPanel
    {
        private readonly IConfiguration _configuration;
        public WCTopPanel(IConfiguration configuration) { _configuration = configuration; }

        // Sheet tray. Top face at Z = 0, 38 flanges turned down (-Z).
        // Local X = across chamber depth (0..depth), Y = along chamber width (0..width).
        // Panel 1: notch at joint end (Y = width). mirror = Panel 2: notch at Y = 0.
        // WC Top Panel 1: DrawWCTopPanel(model, "WCTopPanel1", 1067, 2038, false)
        public DesignDocument DrawWCTopPanel(PaintBoothModel model, string name,
                                             double depth, double width, bool mirror, double T = 1.6)
        {
            DesignDocument design = new DesignDocument();
            design.Units = linearUnitsType.Millimeters;

            double F = 38;                               // flange depth
            double a = (depth - 550) / 2, b = a + 550;   // cutout band across depth (258.5 .. 808.5)
            double c1 = 640, c2 = 640 + 800;              // 800 cutout along width
            double n1 = width - 398;                      // 398 notch to joint edge

            double Ym(double y) => mirror ? width - y : y;

            // ================= 1. TOP SHEET (with notch + cutout) =================
            var outer = Loop((0, 0), (depth, 0), (depth, width), (b, width), (b, n1), (a, n1), (a, width), (0, width));
            var hole = Loop((a, c1), (b, c1), (b, c2), (a, c2));
            var top = new devDept.Eyeshot.Entities.Region(outer, hole);
            design.Entities.Add(top.ExtrudeAsBrep(new Vector3D(0, 0, -T)), Color.BurlyWood);

            // ================= 2. FLANGES (turned down) =================
            Flange(design, 0, T, 0, width, F);                      // long edge, booth side
            Flange(design, depth - T, depth, 0, width, F);          // long edge, back side
            Flange(design, 0, depth, Ym(0), Ym(T), F);              // outer end
            Flange(design, 0, a, Ym(width - T), Ym(width), F);      // joint end, either side of notch
            Flange(design, b, depth, Ym(width - T), Ym(width), F);

            // ================= Export DWG =================
            var path = _configuration["FolderPathConfig:DrawingPath"].ToString();
            if (!Directory.Exists(path + "/PaintBooth drawing"))
                Directory.CreateDirectory(path + "/PaintBooth drawing");
            string dwgFilePath = $"{path}/PaintBooth drawing/{name} {DateTime.Now:hh - mm}.dwg";
            new WriteAutodesk(new WriteAutodeskParams(design), dwgFilePath).DoWork();

            return design;

            // closed loop in XY at Z = 0, Y mirrored if needed
            LinearPath Loop(params (double x, double y)[] pts)
            {
                var p = pts.Select(q => new Point3D(q.x, Ym(q.y), 0)).ToList();
                p.Add(p[0]);
                return new LinearPath(p.ToArray());
            }
        }

        // rectangular flange strip, top at Z = 0, down by h
        private static void Flange(DesignDocument design, double x0, double x1, double y0, double y1, double h)
        {
            double xa = Math.Min(x0, x1), xb = Math.Max(x0, x1);
            double ya = Math.Min(y0, y1), yb = Math.Max(y0, y1);
            var r = new devDept.Eyeshot.Entities.Region(new LinearPath(new[]
            {
                new Point3D(xa, ya, 0), new Point3D(xb, ya, 0),
                new Point3D(xb, yb, 0), new Point3D(xa, yb, 0), new Point3D(xa, ya, 0)
            }));
            design.Entities.Add(r.ExtrudeAsBrep(new Vector3D(0, 0, -h)), Color.BurlyWood);
        }
    }
}