
using devDept.Eyeshot;
using devDept.Eyeshot.Entities;
using devDept.Geometry;
using System;

namespace PaintBooth.Business.Water_Wash_Parts
{
    public static class AssemblyHelper
    {
        // Clone every entity of src, rotate about Y then Z (degrees, about origin), move, add to dst
        public static void AddCopy(DesignDocument dst, DesignDocument src, double dx, double dy, double dz,
                                   double rotYDeg = 0, double rotZDeg = 0)
        {
            foreach (Entity e in src.Entities)
            {
                Entity c = (Entity)e.Clone();
                if (rotYDeg != 0) c.Rotate(rotYDeg * Math.PI / 180.0, Vector3D.AxisY, Point3D.Origin);
                if (rotZDeg != 0) c.Rotate(rotZDeg * Math.PI / 180.0, Vector3D.AxisZ, Point3D.Origin);
                c.Translate(dx, dy, dz);
                dst.Entities.Add(c);
            }
        }

        public static DesignDocument NewDoc(DesignDocument like)
        {
            var d = new DesignDocument();
            d.Units = like.Units;
            return d;
        }
    }
}
