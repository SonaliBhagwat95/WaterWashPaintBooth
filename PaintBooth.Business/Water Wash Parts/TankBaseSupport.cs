using devDept.Eyeshot;
using devDept.Eyeshot.Entities;
using devDept.Geometry;
using Microsoft.Extensions.Configuration;
using PaintBooth.Models;
using System.Drawing;
using System.Linq;

namespace PaintBooth.Business.Water_Wash_Parts
{
    public class TankBaseSupport
    {
        private readonly IConfiguration _configuration;
        public TankBaseSupport(IConfiguration configuration) { _configuration = configuration; }

        // U channel 3T M.S, 75 base, 20 lips outward; legs hL / hR so lips follow the base-plate slope.
        // Local: Y across (0 = outer face of left leg, 75 = outer face of right leg),
        //        Z up from tank bottom, length along +X.
        public DesignDocument DrawChannel(PaintBoothModel model, double hL, double hR, double len,
                                          double W = 75, double lip = 20, double T = 3)
        {
            DesignDocument design = new DesignDocument();
            design.Units = linearUnitsType.Millimeters;

            var pts = new (double y, double z)[]
            {
                (T - lip, hL), (T, hL),                     // left lip top
                (T, T), (W - T, T),                         // inside of base
                (W - T, hR), (W - T + lip, hR),             // right lip top
                (W - T + lip, hR - T), (W, hR - T),         // right lip underside
                (W, 0), (0, 0),                             // outside of base
                (0, hL - T), (T - lip, hL - T)              // left lip underside
            };
            var p = pts.Select(q => new Point3D(0, q.y, q.z)).ToList();
            p.Add(p[0]);

            var r = new devDept.Eyeshot.Entities.Region(new LinearPath(p.ToArray()));
            design.Entities.Add(r.ExtrudeAsBrep(new Vector3D(len, 0, 0)), Color.SteelBlue);
            return design;
        }
    }
}