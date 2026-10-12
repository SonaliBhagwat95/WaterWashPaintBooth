using devDept.Eyeshot;
using devDept.Eyeshot.Translators;
using devDept.Geometry;
using Microsoft.Extensions.Configuration;
using PaintBooth.Models;
using static PaintBooth.Business.SheetProfile;

namespace PaintBooth.Business
{
    public class ReturnTroughBracket
    {
        private readonly IConfiguration _configuration;

        public ReturnTroughBracket(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public DesignDocument DrawReturnTroughBracket(PaintBoothModel model, double width)
        {
            DesignDocument design = new DesignDocument();
            design.Units = linearUnitsType.Millimeters;

            // ---- Inputs (mm) ----
            double T = 1.6;    // sheet thickness
            double Width = width;  // extrusion along Y (not on drawing)
            double BotLeg = 41;     // bottom leg
            double LegDeg = 3;      // both legs tilted up 3°
            double Web = 195.1;  // sloped web
            double WebBend = 59;     // angle between bottom leg and web
            double TopBend = 121;    // angle between web and top leg
            double TopLeg = 48;     // top leg

            // ---- Centreline (X, Z) - bottom-left corner of bottom face = (0, 0) ----
            double webDeg = LegDeg + WebBend;                    // 62° absolute
            double topDeg = webDeg + 180 + TopBend;              // 363° = +3°

            double a0 = LegDeg * Math.PI / 180.0;
            var c = new List<(double X, double Z)>();
            c.Add((-Math.Sin(a0) * T / 2, Math.Cos(a0) * T / 2));   // start 0.8 up, perpendicular to leg
            c.Add(Add(c[0], BotLeg, LegDeg));                    // bottom bend
            c.Add(Add(c[1], Web, webDeg));                    // top bend
            c.Add(Add(c[2], TopLeg, topDeg));                    // top-right end

            Thicken(c, T / 2.0, out var left, out var right);
            AddPiece(design, left, right, 0, c.Count - 1, 0, Width);

            // ---- Export DWG ----
            var path = _configuration["FolderPathConfig:DrawingPath"].ToString();

            if (!Directory.Exists(path + "/PaintBooth drawing"))
                Directory.CreateDirectory(path + "/PaintBooth drawing");

            string dwgFilePath = $"{path}/PaintBooth drawing/ReturnTroughBracket {DateTime.Now:hh - mm}.dwg";

            WriteAutodeskParams auto = new WriteAutodeskParams(design);
            WriteAutodesk dwgg1 = new WriteAutodesk(auto, dwgFilePath);
            dwgg1.DoWork();

            return design;
        }
    }
}