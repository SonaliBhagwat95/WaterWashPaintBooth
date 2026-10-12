using devDept.Eyeshot;
using devDept.Eyeshot.Entities;
using devDept.Eyeshot.Translators;
using devDept.Geometry;
using Microsoft.Extensions.Configuration;
using PaintBooth.Models;
using System.Drawing;

namespace PaintBooth.Business
{
    public class PSection(IConfiguration _configuration)
    {
        public DesignDocument DrawPSection(PaintBoothModel model, double zBottom)
        {
            DesignDocument design = new DesignDocument();
            design.Units =  linearUnitsType.Millimeters;
            
            double H = 460;   // overall height
            double W = 40;    // flange width
            double T = 3;     // sheet thickness
            double L = 15;    // lip length (outer)

            LinearPath p = new LinearPath(new Point3D[]
            {
               
                new Point3D(0,    0,     0),
                new Point3D(0,    0,     H),   // outer web
                new Point3D(-W,    0,     H),   // top flange outer
                new Point3D(-W,    0,     H - L),   // top lip outer
                new Point3D(-(W - T),0   , H - L),   // top lip tip
                new Point3D(-(W - T),0   , H - T),   // top lip inner
                new Point3D(-T,    0,     H - T),   // top flange inner
                new Point3D(-T,    0,     T),   // inner web
                new Point3D(-(W - T), 0, T),   // bottom flange inner
                new Point3D(-(W - T), 0, L),   // bottom lip inner
                new Point3D(-W,     0,     L),   // bottom lip tip
                new Point3D(-W,     0,     0),   // bottom lip outer
                new Point3D(0,     0,     0)    // close
            });
            devDept.Eyeshot.Entities.Region profile = new devDept.Eyeshot.Entities.Region(p);
            Brep brepPesection = profile.ExtrudeAsBrep(new Vector3D(0, (double)model.W, 0));
            // top of P-section (Z = H) sits on the underside of the base frame
            brepPesection.Translate(W, 0, zBottom - H);
            design.Entities.Add(brepPesection, Color.BurlyWood);
            var path = _configuration["FolderPathConfig:DrawingPath"].ToString();

            if (!Directory.Exists(path + "/PaintBooth drawing"))
                Directory.CreateDirectory(path + "/PaintBooth drawing");
            string dwgFilePath = "";

           
            dwgFilePath = $"{path}/PaintBooth drawing/PSection  {DateTime.Now:hh - mm}.dwg";
           
            WriteAutodeskParams auto = new WriteAutodeskParams(design);
            WriteAutodesk dwgg1 = new WriteAutodesk(auto, dwgFilePath);
            dwgg1.DoWork();
          
            return design;
        }
        //right left section
        public DesignDocument DrawQSection(PaintBoothModel model, double zBottom, double totalLength, double segmentLength, bool farSide)
        {
            DesignDocument design = new DesignDocument();
            design.Units = linearUnitsType.Millimeters;

            double H = 460;   // overall height (bottom to top of top flange)
            double T = 3;     // sheet thickness
            double Wt = 40;    // top flange width
            double Lt = 19;    // top lip, rising above top flange
            double Wb = 38;    // bottom flange width
            double Lb = 15;    // bottom lip (inward, upward)

            // ── Main C-channel profile ─────────────────────────────────
            var channel = devDept.Eyeshot.Entities.Region.CreatePolygon(new Point3D[]
            {
                new Point3D(0,      0, 0),
                new Point3D(0,      0, H),
                new Point3D(-(Wt - T), 0, H),
                new Point3D(-(Wt - T), 0, H + Lt),
                new Point3D(-Wt,     0, H + Lt),
                new Point3D(-Wt,     0, H - T),
                new Point3D(-T,      0, H - T),
                new Point3D(-T,      0, T),
                new Point3D(-Wb - T, 0, T),
                new Point3D(-Wb - T, 0, Lb),
                new Point3D(-Wb,     0, Lb),
                new Point3D(-Wb,     0, 0)
            });

            // ── Split total length into 2390 pieces + remainder ─────────
            double start = 0;
            while (start < totalLength - 0.01)
            {
                double len = Math.Min(segmentLength, totalLength - start);

                Brep b = channel.ExtrudeAsBrep(new Vector3D(0, len, 0));

                if (!farSide)
                {
                    // Wall at Y = 0: flanges face +Y, piece runs from X = start to start + len
                    b.Rotate(Math.PI / 2, Vector3D.AxisZ);
                    b.Translate(start + len, 0, zBottom - H);
                }
                else
                {
                    // Wall at Y = W: flanges face -Y, piece runs from X = start to start + len
                    b.Rotate(-Math.PI / 2, Vector3D.AxisZ);
                    b.Translate(start, (double)model.W, zBottom - H);
                }

                design.Entities.Add(b, Color.BurlyWood);
                start += len;
            }

            // ── Write file ──────────────────────────────────────────────
            string basePath = _configuration["FolderPathConfig:DrawingPath"];
            string folder = Path.Combine(basePath, "PaintBooth drawing");
            Directory.CreateDirectory(folder);

            string side = farSide ? "Right" : "Left";
            string dwgFilePath = Path.Combine(folder, $"QSection_{side}_{DateTime.Now:yyyyMMdd_HHmmss}.dwg");

            WriteAutodeskParams auto = new WriteAutodeskParams(design);
            WriteAutodesk dwg = new WriteAutodesk(auto, dwgFilePath);
            dwg.DoWork();

            return design;
        }

        public DesignDocument DrawbackQSection(PaintBoothModel model, double zBottom, double totalLength)
        {
            DesignDocument design = new DesignDocument();
            design.Units = linearUnitsType.Millimeters;

            double H = 460;   // overall height (bottom to top of top flange)
            double T = 3;     // sheet thickness
            double Wt = 40;    // top flange width
            double Lt = 19;    // top lip, rising above top flange
            double Wb = 38;    // bottom flange width
            double Lb = 15;    // bottom lip (inward, upward)

            // ── Main C-channel profile ─────────────────────────────────
            var channel = devDept.Eyeshot.Entities.Region.CreatePolygon(new Point3D[]
            {
                new Point3D(0,      0, 0),
                new Point3D(0,      0, H),
                new Point3D((Wt - T), 0, H),
                new Point3D((Wt - T), 0, H + Lt),
                new Point3D(Wt,     0, H + Lt),
                new Point3D(Wt,     0, H - T),
                new Point3D(T,      0, H - T),
                new Point3D(T,      0, T),
                new Point3D(Wb - T, 0, T),
                new Point3D(Wb - T, 0, Lb),
                new Point3D(Wb,     0, Lb),
                new Point3D(Wb,     0, 0)
            });
            Brep brepPesection = channel.ExtrudeAsBrep(new Vector3D(0, (double)model.W, 0));
            // top of P-section (Z = H) sits on the underside of the base frame
            brepPesection.Translate(totalLength, 0, zBottom - H);
            design.Entities.Add(brepPesection, Color.BurlyWood);
            var path = _configuration["FolderPathConfig:DrawingPath"].ToString();

            if (!Directory.Exists(path + "/PaintBooth drawing"))
                Directory.CreateDirectory(path + "/PaintBooth drawing");
            string dwgFilePath = "";


            dwgFilePath = $"{path}/PaintBooth drawing/BackQSection  {DateTime.Now:hh - mm}.dwg";

            WriteAutodeskParams auto = new WriteAutodeskParams(design);
            WriteAutodesk dwgg1 = new WriteAutodesk(auto, dwgFilePath);
            dwgg1.DoWork();

            return design;
        }
    }
}
