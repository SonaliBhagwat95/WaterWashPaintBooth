using devDept.Eyeshot;
using devDept.Eyeshot.Entities;
using devDept.Geometry;

namespace PaintBooth.Business
{

 
    public static class EOTCutoutHelper
    {
        public static int Apply(DesignDocument doc, double x0, double length,
                                double yCenter, double width,
                                double zBottom, double zTop)
        {
            double y0 = yCenter - width / 2.0;
            var cMin = new Point3D(x0, y0, zBottom);
            var cMax = new Point3D(x0 + length, y0 + width, zTop);

            var toRemove = new List<Entity>();
            var toAdd = new List<Entity>();
            int cutCount = 0;

            foreach (var ent in doc.Entities.ToList())
            {
                if (ent is BlockReference br && doc.Blocks.Contains(br.BlockName))
                {
                    Block blk = doc.Blocks[br.BlockName];
                    var worldParts = new List<Entity>();
                    bool hit = false;

                    foreach (var be in blk.Entities)
                    {
                        var c = (Entity)be.Clone();
                        c.TransformBy(br.Transformation);

                        if (c.ColorMethod == colorMethodType.byParent)
                        {
                            c.ColorMethod = br.ColorMethod;
                            c.Color = br.Color;
                        }

                        if (TryCut(c, cMin, cMax, out var pieces))
                        {
                            hit = true;
                            worldParts.AddRange(pieces);
                            continue;
                        }
                        worldParts.Add(c);
                    }

                    if (hit)
                    {
                        toRemove.Add(br);
                        toAdd.AddRange(worldParts);
                        cutCount++;
                    }
                }
                else if (TryCut(ent, cMin, cMax, out var pieces))
                {
                    toRemove.Add(ent);
                    toAdd.AddRange(pieces);
                    cutCount++;
                }
            }
            foreach (var e in toRemove) doc.Entities.Remove(e);
            doc.Entities.AddRange(toAdd);
            doc.Entities.Regen();
            return cutCount;
        }

        private static bool TryCut(Entity e, Point3D cMin, Point3D cMax, out List<Entity> pieces)
        {
            pieces = new List<Entity>();
            e.Regen(0.1);
            if (!Overlaps(e.BoxMin, e.BoxMax, cMin, cMax)) return false;

            double dx = cMax.X - cMin.X, dy = cMax.Y - cMin.Y, dz = cMax.Z - cMin.Z;
            Entity[] result;

            switch (e)
            {
                case Region r:
                    // flat roof sheet → cut with a coplanar rectangle at the sheet's Z
                    if (r.BoxMax.Z - r.BoxMin.Z > 0.01) return false;   // only horizontal regions
                    double z = r.BoxMin.Z;
                    var slot = Region.CreatePolygon(new Point3D[]
                    {
                    new Point3D(cMin.X, cMin.Y, z),
                    new Point3D(cMax.X, cMin.Y, z),
                    new Point3D(cMax.X, cMax.Y, z),
                    new Point3D(cMin.X, cMax.Y, z),
                    });
                    result = Region.Difference(r, slot);
                    break;

                case Solid s:
                    var sCut = Solid.CreateBox(dx, dy, dz);
                    sCut.Translate(cMin.X, cMin.Y, cMin.Z);
                    result = Solid.Difference(s, sCut);
                    break;

                case Brep b:
                    var bCut = Brep.CreateBox(dx, dy, dz);
                    bCut.Translate(cMin.X, cMin.Y, cMin.Z);
                    result = Brep.Difference(b, bCut);
                    break;

                default:
                    return false;
            }

            if (result == null) return false;          // boolean failed → keep original

            foreach (var r in result)                  // empty → part fully inside slot, removed
            {
                r.ColorMethod = e.ColorMethod;
                r.Color = e.Color;
                r.LayerName = e.LayerName;
                pieces.Add(r);
            }
            return true;
        }

        private static bool Overlaps(Point3D aMin, Point3D aMax, Point3D bMin, Point3D bMax) =>
            aMin.X < bMax.X && aMax.X > bMin.X &&
            aMin.Y < bMax.Y && aMax.Y > bMin.Y &&
            aMin.Z < bMax.Z && aMax.Z > bMin.Z;
    }
}
