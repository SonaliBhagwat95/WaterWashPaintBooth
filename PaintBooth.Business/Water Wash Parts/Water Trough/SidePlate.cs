using devDept.Eyeshot;
using devDept.Eyeshot.Entities;
using devDept.Eyeshot.Translators;
using devDept.Geometry;
using Microsoft.Extensions.Configuration;
using PaintBooth.Models;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;

namespace PaintBooth.Business.Water_Wash_Parts.Water_Trough
{
    public class SidePlate
    {
        private readonly IConfiguration _configuration;
        public SidePlate(IConfiguration configuration) { _configuration = configuration; }

        // mirror = false : 1 No as shown (holes on right),  true : opposite hand (holes on left)
        public DesignDocument DrawSidePlate(PaintBoothModel model, string name = "SidePlate", bool mirror = false)
        {
            DesignDocument design = new DesignDocument();
            design.Units = linearUnitsType.Millimeters;

            // ---- Inputs (mm) ----
            double T = 2;      // 2 THK SS304
            double W = 265;    // width  (X)
            double H = 340;    // height (Z)
            double HoleD = 11;     // Ø11

            // hole centres as shown (X from left, Z from bottom)
            var holes = new List<(double X, double Z)>
            {
                (245, 320), (245, 220), (245, 120), (245, 20),   // right column
                (45,  20),  (145, 20)                            // bottom row (245,20 shared)
            };

            // ---- Plate in XZ, extruded along Y ----
            var curves = new List<ICurve>
            {
                new LinearPath(new Point3D[]
                {
                    new Point3D(0, 0, 0), new Point3D(W, 0, 0),
                    new Point3D(W, 0, H), new Point3D(0, 0, H),
                    new Point3D(0, 0, 0)
                })
            };
            foreach (var (x, z) in holes)
            {
                double hx = mirror ? W - x : x;
                curves.Add(new Circle(Plane.XZ, new Point3D(hx, 0, z), HoleD / 2));
            }

            var plate = new devDept.Eyeshot.Entities.Region(curves.ToArray());
            design.Entities.Add(plate.ExtrudeAsBrep(new Vector3D(0, T, 0)), Color.BurlyWood);

            //// ---- Export DWG ----
            //var path = _configuration["FolderPathConfig:DrawingPath"].ToString();
            //if (!Directory.Exists(path + "/PaintBooth drawing"))
            //    Directory.CreateDirectory(path + "/PaintBooth drawing");
            //string dwgFilePath = $"{path}/PaintBooth drawing/{name} {DateTime.Now:hh - mm}.dwg";
            //new WriteAutodesk(new WriteAutodeskParams(design), dwgFilePath).DoWork();

            return design;
        }
    }
}