using devDept.Eyeshot;
using devDept.Eyeshot.Entities;
using devDept.Eyeshot.Translators;
using devDept.Geometry;
using Microsoft.Extensions.Configuration;
using PaintBooth.Models;
using System.Drawing;

namespace PaintBooth.Business.Water_Wash_Parts
{
    // Vertical angle support: ISA 40x40x3 on 75x75x5T base plate. Do not weld to water tank.
    // Origin = centre of base plate underside, +Z up. angleH = angle length above base plate.
    public class AngleSupport
    {
        private readonly IConfiguration _configuration;
        public AngleSupport(IConfiguration configuration) { _configuration = configuration; }

        public const double PlateT = 5;

        public DesignDocument DrawAngleSupport(PaintBoothModel model, string name, double angleH)
        {
            DesignDocument design = new DesignDocument();
            design.Units = linearUnitsType.Millimeters;

            double P = 75;                 // base plate 75 x 75 x 5T
            double A = 40, t = 3;          // ISA 40 x 40 x 3

            // ---- base plate ----
            var plate = new devDept.Eyeshot.Entities.Region(new LinearPath(new[]
            {
                new Point3D(-P / 2, -P / 2, 0), new Point3D(P / 2, -P / 2, 0),
                new Point3D(P / 2,  P / 2, 0),  new Point3D(-P / 2, P / 2, 0),
                new Point3D(-P / 2, -P / 2, 0)
            }));
            design.Entities.Add(plate.ExtrudeAsBrep(new Vector3D(0, 0, PlateT)), Color.SteelBlue);

            // ---- ISA 40x40x3, heel near plate centre ----
            double c = -A / 2;             // heel corner so the angle sits centred on the plate
            var angle = new devDept.Eyeshot.Entities.Region(new LinearPath(new[]
            {
                new Point3D(c,         c,         PlateT),
                new Point3D(c + A,     c,         PlateT),
                new Point3D(c + A,     c + t,     PlateT),
                new Point3D(c + t,     c + t,     PlateT),
                new Point3D(c + t,     c + A,     PlateT),
                new Point3D(c,         c + A,     PlateT),
                new Point3D(c,         c,         PlateT)
            }));
            design.Entities.Add(angle.ExtrudeAsBrep(new Vector3D(0, 0, angleH)), Color.SteelBlue);

            // ---- Export DWG ----
            var path = _configuration["FolderPathConfig:DrawingPath"].ToString();
            if (!Directory.Exists(path + "/PaintBooth drawing"))
                Directory.CreateDirectory(path + "/PaintBooth drawing");
            string dwgFilePath = $"{path}/PaintBooth drawing/{name} {DateTime.Now:hh - mm}.dwg";
            new WriteAutodesk(new WriteAutodeskParams(design), dwgFilePath).DoWork();

            return design;
        }
    }
}