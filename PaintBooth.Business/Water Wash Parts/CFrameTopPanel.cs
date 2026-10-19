using devDept.Eyeshot;
using devDept.Eyeshot.Entities;
using devDept.Eyeshot.Translators;
using devDept.Geometry;
using Microsoft.Extensions.Configuration;
using PaintBooth.Models;
using System;
using System.Drawing;
using System.IO;
using System.Linq;

namespace PaintBooth.Business.Water_Wash_Parts
{
    public class CFrameTopPanel
    {
        private readonly IConfiguration _configuration;
        public CFrameTopPanel(IConfiguration configuration) { _configuration = configuration; }

        // Welded frame of 4 bent C-sections, standing on the top panel.
        // Origin = centre of opening, Z = 0 at underside (sits on panel top face).
        // Local X = across chamber depth (side members), Y = along width (cross members).
        // C Frame: DrawCFrame(model, "CFrameTopPanel")
        public DesignDocument DrawCFrame(PaintBoothModel model, string name,
                                         double innerW = 800, double innerD = 550, double sideL = 1037,
                                         double H = 100, double F = 40, double T = 3)
        {
            DesignDocument design = new DesignDocument();
            design.Units = linearUnitsType.Millimeters;

            double wy = innerW / 2;     // 400 - side member web inner face
            double dx = innerD / 2;     // 275 - cross member web inner face

            // C profile (u = outward from opening, v = up): web on opening side, flanges outward   (CHECK)
            (double u, double v)[] C(double u0) => new[]
            {
                (u0, 0.0), (u0 + F, 0.0), (u0 + F, T), (u0 + T, T),
                (u0 + T, H - T), (u0 + F, H - T), (u0 + F, H), (u0, H)
            };

            foreach (int s in new[] { +1, -1 })
            {
                // ---- side members: along X, full 1037 ----
                var side = Loop(C(wy).Select(p => new Point3D(-sideL / 2, s * p.u, p.v)));
                Add(design, side, new Vector3D(sideL, 0, 0));

                // ---- cross members: along Y, between side member webs ----
                var cross = Loop(C(dx).Select(p => new Point3D(s * p.u, -wy, p.v)));
                Add(design, cross, new Vector3D(0, 2 * wy, 0));
            }

            // ================= Export DWG =================
            var path = _configuration["FolderPathConfig:DrawingPath"].ToString();
            if (!Directory.Exists(path + "/PaintBooth drawing"))
                Directory.CreateDirectory(path + "/PaintBooth drawing");
            string dwgFilePath = $"{path}/PaintBooth drawing/{name} {DateTime.Now:hh - mm}.dwg";
            new WriteAutodesk(new WriteAutodeskParams(design), dwgFilePath).DoWork();

            return design;
        }

        private static LinearPath Loop(System.Collections.Generic.IEnumerable<Point3D> pts)
        {
            var p = pts.ToList();
            p.Add(p[0]);
            return new LinearPath(p.ToArray());
        }

        private static void Add(DesignDocument design, LinearPath profile, Vector3D dir)
        {
            var r = new devDept.Eyeshot.Entities.Region(profile);
            design.Entities.Add(r.ExtrudeAsBrep(dir), Color.BurlyWood);
        }
    }
}