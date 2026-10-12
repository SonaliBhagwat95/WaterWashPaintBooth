using System;
using System.Collections.Generic;
using System.Drawing;
using devDept.Eyeshot;
using devDept.Eyeshot.Entities;
using devDept.Geometry;

namespace PaintBooth.Business
{
    public static class SheetProfile
    {
        public static (double X, double Z) Add((double X, double Z) p, double len, double deg)
        {
            double a = deg * Math.PI / 180.0;
            return (p.X + len * Math.Cos(a), p.Z + len * Math.Sin(a));
        }

        // Offsets a centreline by ±h with mitred joints
        public static void Thicken(List<(double X, double Z)> p, double h,
                                   out List<(double X, double Z)> left,
                                   out List<(double X, double Z)> right)
        {
            int n = p.Count;
            var nx = new double[n - 1];
            var nz = new double[n - 1];
            for (int k = 0; k < n - 1; k++)
            {
                double dx = p[k + 1].X - p[k].X, dz = p[k + 1].Z - p[k].Z;
                double l = Math.Sqrt(dx * dx + dz * dz);
                nx[k] = -dz / l; nz[k] = dx / l;
            }

            left = new List<(double X, double Z)>();
            right = new List<(double X, double Z)>();
            for (int i = 0; i < n; i++)
            {
                double mx, mz, s;
                if (i == 0) { mx = nx[0]; mz = nz[0]; s = h; }
                else if (i == n - 1) { mx = nx[n - 2]; mz = nz[n - 2]; s = h; }
                else
                {
                    double bx = nx[i - 1] + nx[i], bz = nz[i - 1] + nz[i];
                    double bl = Math.Sqrt(bx * bx + bz * bz);
                    if (bl < 1e-9) { mx = nx[i - 1]; mz = nz[i - 1]; s = h; }
                    else
                    {
                        mx = bx / bl; mz = bz / bl;
                        double cosHalf = mx * nx[i - 1] + mz * nz[i - 1];
                        s = Math.Min(h / cosHalf, 10 * h);
                    }
                }
                left.Add((p[i].X + mx * s, p[i].Z + mz * s));
                right.Add((p[i].X - mx * s, p[i].Z - mz * s));
            }
        }

        // Closed band between centreline indices i..j (XZ plane), extruded along Y from y0 to y1
        public static void AddPiece(DesignDocument design,
                                    List<(double X, double Z)> left, List<(double X, double Z)> right,
                                    int i, int j, double y0, double y1)
        {
            var pts = new List<Point3D>();
            for (int k = i; k <= j; k++) pts.Add(new Point3D(left[k].X, 0, left[k].Z));
            for (int k = j; k >= i; k--) pts.Add(new Point3D(right[k].X, 0, right[k].Z));
            pts.Add(pts[0]);

            devDept.Eyeshot.Entities.Region profile = new devDept.Eyeshot.Entities.Region(new LinearPath(pts.ToArray()));
            Brep brep = profile.ExtrudeAsBrep(new Vector3D(0, y1 - y0, 0));
            brep.Translate(0, y0, 0);
            design.Entities.Add(brep, Color.BurlyWood);
        }
    }
}