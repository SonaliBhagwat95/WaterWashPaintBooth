using devDept.Eyeshot;
using devDept.Eyeshot.Entities;
using devDept.Eyeshot.Translators;
using devDept.Geometry;
using Microsoft.Extensions.Configuration;
using PaintBooth.Models;
using System.Drawing;

namespace PaintBooth.Business.Water_Wash_Parts
{
    public class Grating
    {
        private readonly IConfiguration _configuration;
        public Grating(IConfiguration configuration) { _configuration = configuration; }

        // Non load bearing grating, M.S. Flat 30x5 frame + bearing bars, 6 sq cross bars flush top.
        // Origin = bottom corner. X = 650 side (bearing bars run along X), Y = 660 side, Z up 0..30.
        // Grating: DrawGrating(model, "Grating", 650, 660)
        public DesignDocument DrawGrating(PaintBoothModel model, string name, double W = 650, double L = 660)
        {
            DesignDocument design = new DesignDocument();
            design.Units = linearUnitsType.Millimeters;

            double H = 30, t = 5;                       // flat 30 x 5
            double firstBar = 27.6, barPitch = 40;      // 35 clear + 5 thk
            double sq = 6, sqEnd = 75, sqPitch = 100;   // 6 mm sq bar @100, 75 from ends

            // ================= 1. FRAME (flat 30x5 all round) =================
            Box(design, 0, W, 0, t, 0, H);                       // along X, Y = 0 side
            Box(design, 0, W, L - t, L, 0, H);                   // along X, Y = 660 side
            Box(design, 0, t, t, L - t, 0, H);                   // along Y, X = 0 side
            Box(design, W - t, W, t, L - t, 0, H);               // along Y, X = 650 side

            // ================= 2. BEARING BARS (along X, @40) =================
            for (double y = firstBar; y + t <= L - firstBar + 0.5; y += barPitch)
                Box(design, t, W - t, y, y + t, 0, H);

            // ================= 3. CROSS BARS (6 sq, along Y, flush top) =================
            for (double xc = sqEnd; xc <= W - sqEnd + 0.5; xc += sqPitch)
                Box(design, xc - sq / 2, xc + sq / 2, t, L - t, H - sq, H);

            // ================= Export DWG =================
            var path = _configuration["FolderPathConfig:DrawingPath"].ToString();
            if (!Directory.Exists(path + "/PaintBooth drawing"))
                Directory.CreateDirectory(path + "/PaintBooth drawing");
            string dwgFilePath = $"{path}/PaintBooth drawing/{name} {DateTime.Now:hh - mm}.dwg";
            new WriteAutodesk(new WriteAutodeskParams(design), dwgFilePath).DoWork();

            return design;
        }

        private static void Box(DesignDocument design, double x0, double x1, double y0, double y1,
                                double z0, double z1)
        {
            var r = new devDept.Eyeshot.Entities.Region(new LinearPath(new[]
            {
                new Point3D(x0, y0, z0), new Point3D(x1, y0, z0),
                new Point3D(x1, y1, z0), new Point3D(x0, y1, z0), new Point3D(x0, y0, z0)
            }));
            design.Entities.Add(r.ExtrudeAsBrep(new Vector3D(0, 0, z1 - z0)), Color.DimGray);
        }
    }
}