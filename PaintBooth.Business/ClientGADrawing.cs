using devDept.Eyeshot;
using devDept.Eyeshot.Entities;
using devDept.Eyeshot.Translators;
using devDept.Geometry;
using Microsoft.Extensions.Configuration;
using PaintBooth.Models;
using System.Drawing;
using devregion = devDept.Eyeshot.Entities.Region;
using System.Drawing.Imaging;

namespace PaintBooth.Business
{
    #region Input models
    public class ClientGAInfo
    {
        public string Customer { get; set; } = "";
        public string SalesNo { get; set; } = "";
        public string Title { get; set; } = "GENERAL ARRANGEMENT OF PAINT BOOTH";
        public double PanelWidth { get; set; }
        public bool HasBaffles { get; set; }            // panel setting width → bay positions
        public string DrawingNo { get; set; } = "";
        public string DrawnBy { get; set; } = "";
        public string CheckedBy { get; set; } = "";
        public string ApprovedBy { get; set; } = "";
        public int Lights { get; set; }                     // tube light fixtures (qty)
        public string BoothType { get; set; } = "";        // PaintBoothTypefromEnquiry
        public double ExtractionHeight { get; set; }
        public double OverallWidth { get; set; }           // outside of side panels (client: 2626)
        public double OverallHeight { get; set; }          // floor to roof, WITHOUT blower (client: 2416)
        public List<GAScopeItem> Scope { get; set; } = new();
        public List<string> Notes { get; set; } = new();
        public List<GABalloon> Balloons { get; set; } = new();
        public string DrawnDate { get; set; } = DateTime.Now.ToString("dd.MM.yyyy");
        public string CheckedDate { get; set; } = DateTime.Now.ToString("dd.MM.yyyy");
        public string ApprovedDate { get; set; } = DateTime.Now.ToString("dd.MM.yyyy");
        public string Product { get; set; } = "Booth";
        public string Model { get; set; } = "";              // e.g. "S.D."
        public string Part1 { get; set; } = "";              // e.g. "DRY BACK"
        public string Part2 { get; set; } = "SPRAY PAINTING BOOTH";
        public string Material { get; set; } = "G.I.";
        public string Qty { get; set; } = "AS SHOWN";
        public string Revision { get; set; } = "R0";
        public string SheetNo { get; set; } = "1 OF 1";
        public string CompanyLine1 { get; set; } = "Bullows Paint";
        public string CompanyLine2 { get; set; } = "Eqpt. Pvt. Ltd.";
        public string CompanyLine3 { get; set; } = "(THANE-400604)";
        public double WallDistance { get; set; }
        public double WallThickness { get; set; } = 250;      // client sheet shows 250 brick wall
        public double DuctHeightFromFFL { get; set; }
        public double DuctTopZ { get; set; }
        public double DuctCutZ { get; set; }
        public double DuctCutSize { get; set; }
        public double BoothDepth { get; set; }
        public double ECDepth { get; set; }
        public double DuctMaxX { get; set; }      // furthest X of the duct run (original model coords)
        public double BoothWidth { get; set; }      // model.W
        public int DuctRuns { get; set; } = 1;      // one run per transition piece
        public double WallMargin { get; set; } = 500;   // how far the wall extends past the booth sides
    }

    public class GAScopeItem
    {
        public string Title { get; set; } = "";
        public List<string> Lines { get; set; } = new();
    }

    public class GABalloon
    {
        public int No { get; set; }
        public string View { get; set; } = "ELEV";        // PLAN / ELEV / SIDE
        public Point3D Target { get; set; } = Point3D.Origin; // ORIGINAL model coords (X=depth, Y=width, Z=height)
        public double Dx { get; set; } = 20;               // balloon offset from target, paper mm
        public double Dy { get; set; } = 20;
    }
    #endregion

    /// <summary>
    /// Builds a client-style 2D GA (A1): PLAN, ELEVATION, SIDE VIEW (R.H.), scope-of-supply table,
    /// notes, title block, item balloons and dimension chains — from the combined 3D model.
    /// All sheet values are paper mm; model → sheet mapping uses the view's centre-anchored X/Y and Scale.
    /// </summary>
    public class ClientGADrawing(IConfiguration _configuration)
    {
        // A1 landscape (paper mm)
        const double SW = 841, SH = 594;
        const double ML = 20, MO = 10;          // left binding margin / other margins
        const double RCOL = 230;                // right column: scope + notes + title block
        const double GAP = 50;                  // room for dimensions between views
        const double TXT = 2.5, ARW = 2.5, EXT = 2;

        const string LFrame = "GA_Frame", LDim = "GA_Dim", LText = "GA_Text",
                     LTitle = "GA_Title", LBal = "GA_Balloon", LFloor = "GA_Floor";
        const string LOper = "GA_Operator", LAirBand = "GA_Airflow", LAirHead = "GA_AirflowHead";
        const string LLight = "GA_TubeLight"; const string LBaffle = "GA_Baffle"; const string LTB = "GA_TitleText";
        Sheet sheet;
        Point3D c;                                                  // model bbox centre = rotation pivot
        readonly Dictionary<string, View> views = new();
        readonly Dictionary<string, (double w, double h)> ext = new();  // model extents per view
        int scaleN;

        public string Build(DesignDocument gaModel, PaintBoothModel m, ClientGAInfo info)
        {
            #region 1. Model prep — rotate 180° about Z so E/C + blower sit on the LEFT (client layout)
            gaModel.Entities.Regen();
            gaModel.Entities.UpdateBoundingBox();
            Point3D bMin = gaModel.Entities.BoxMin, bMax = gaModel.Entities.BoxMax;
            c = new Point3D((bMin.X + bMax.X) / 2, (bMin.Y + bMax.Y) / 2, (bMin.Z + bMax.Z) / 2);
            foreach (Entity e in gaModel.Entities)
                e.Rotate(Math.PI, Vector3D.AxisZ, new Point3D(c.X, c.Y, 0));
            gaModel.Entities.Regen();
            gaModel.Entities.UpdateBoundingBox();

            double lx = bMax.X - bMin.X, ly = bMax.Y - bMin.Y, lz = bMax.Z - bMin.Z;
            ext["PLAN"] = (lx, ly);
            ext["ELEV"] = (lx, lz);
            ext["SIDE"] = (ly, lz);
            #endregion
            #region 2. Scale + layout (plan above elevation, side view right of elevation)
            const double DIMROOM = 38;     // room for 2 dimension tiers + text on ONE side of a view
            const double TITLEROOM = 45;   // bottom dimensions + view title under a view
            double availW = SW - ML - MO - RCOL - 10;
            double availH = SH - 2 * MO;
            int[] std = { 5, 10, 15, 20, 25, 30, 40, 50, 75, 100 };
            scaleN = std.FirstOrDefault(n =>
                  DIMROOM + lx / n + (2 * DIMROOM + 10) + ly / n + DIMROOM <= availW &&
                  TITLEROOM + lz / n + TITLEROOM + 10 + ly / n + 10 <= availH);
            if (scaleN == 0) scaleN = std[^1];
            double s = 1.0 / scaleN;

            double x0 = ML + DIMROOM, y0 = MO + TITLEROOM;
            Point2D elevC = new Point2D(x0 + lx * s / 2, y0 + lz * s / 2);
            Point2D planC = new Point2D(elevC.X, y0 + lz * s + TITLEROOM + 10 + ly * s / 2);
            // elevation right dims + side-view left dims never share space now
            Point2D sideC = new Point2D(x0 + lx * s + 2 * DIMROOM + 10 + ly * s / 2, elevC.Y);
            #endregion

            #region 3. Sheet, layers, views
            DrawingDocument doc = new AddSheet().AddSheets("ClientGA");
            sheet = doc.ActiveSheet;

            doc.Layers.Add(new Layer(LFrame, Color.RoyalBlue));
            doc.Layers.Add(new Layer(LDim, Color.Red));
            doc.Layers.Add(new Layer(LText, Color.White));
            doc.Layers.Add(new Layer(LTitle, Color.Magenta));
            doc.Layers.Add(new Layer(LBal, Color.Blue));
            doc.Layers.Add(new Layer(LFloor, Color.LimeGreen));
            doc.Layers.Add(new Layer(LOper, Color.MediumSlateBlue));
            doc.Layers.Add(new Layer(LAirBand, Color.LightGreen));
            doc.Layers.Add(new Layer(LAirHead, Color.DodgerBlue));
            doc.Layers.Add(new Layer(LLight, Color.Magenta));
            doc.Layers.Add(new Layer(LBaffle, Color.Blue));
            doc.Layers.Add(new Layer(LTB, Color.Lime));

            AddView("GA_Plan", viewType.Top, planC, s);
            AddView("GA_Elev", viewType.Front, elevC, s);
            AddView("GA_Side", viewType.Right, sideC, s);   // looks at the open front → baffles visible

            var vb = new ViewBuilder(gaModel, doc);
            vb.DoWork();
            vb.AddTo(doc);

            foreach (var kvp in vb.SheetsViews)
                foreach (var v in kvp.Value)
                {
                    if (v.BlockName == "GA_Plan") views["PLAN"] = v;
                    else if (v.BlockName == "GA_Elev") views["ELEV"] = v;
                    else if (v.BlockName == "GA_Side") views["SIDE"] = v;
                }
            // remove unwanted layers from the 2D views (e.g. dashed "Wires" lines)
            string[] hideLayers = { "Wires" };

            foreach (Block blk in doc.Blocks)
                for (int n = blk.Entities.Count - 1; n >= 0; n--)
                    if (hideLayers.Contains(blk.Entities[n].LayerName, StringComparer.OrdinalIgnoreCase))
                        blk.Entities.RemoveAt(n);

            foreach (string ln in hideLayers)
            {
                var hideLayer = doc.Layers.FirstOrDefault(l => string.Equals(l.Name, ln, StringComparison.OrdinalIgnoreCase));
                if (hideLayer != null) hideLayer.Visible = false;     // also off in the DWG
            }
            // view geometry → yellow (ViewBuilder puts it on layer 0 = white)
            foreach (var v in views.Values)
            {
                Block blk = doc.Blocks.FirstOrDefault(b => b.Name == v.BlockName);
                if (blk == null) continue;
                foreach (Entity e in blk.Entities)
                {
                    e.ColorMethod = colorMethodType.byEntity;
                    e.Color = Color.WhiteSmoke;
                }
            }
            #endregion

            DrawFrame();
            double tableBottom = DrawScopeTable(info);
            double titleTop = DrawTitleBlock(info, doc);
            DrawNotes(info, tableBottom - 6, titleTop + 4);

            DrawDimensions(m, info, bMax.Z);
            if (views.ContainsKey("ELEV")) FloorLine("ELEV", markerLeft: true);
            if (views.ContainsKey("SIDE")) FloorLine("SIDE", markerLeft: false);
            DrawElevationDecor(doc, m, info);
            if (views.ContainsKey("ELEV")) AddWallAndFflDims(sheet.Entities, info, ElevToSheet);
            if (views.ContainsKey("ELEV")) AddEcHeightDim(sheet.Entities, info, ElevToSheet);
            if (views.ContainsKey("PLAN")) AddWallPlan(sheet.Entities, info, PlanToSheet);
            if (views.ContainsKey("SIDE")) AddWallSide(sheet.Entities, info, SideToSheet);
            NamePlate(doc, m, info);
            DrawBalloons(info);

            ViewTitle("PLAN", "PLAN");
            ViewTitle("ELEV", "ELEVATION");
            ViewTitle("SIDE", "SIDE VIEW (R.H.)");

            #region Write DWG
            string path = _configuration["FolderPathConfig:DrawingPath"].ToString();
            string dir = Path.Combine(path, "MFG Drawing");
            Directory.CreateDirectory(dir);
            string safeNo = string.Join("_", (info.SalesNo ?? "GA").Split(Path.GetInvalidFileNameChars()));
            string file = Path.Combine(dir, $" GA {safeNo} {DateTime.Now:HH-mm}.dwg");
            new WriteAutodesk(new WriteAutodeskParams(doc), file).DoWork();
            return file;
            #endregion
        }

        #region Views + model→sheet mapping
        private void AddView(string name, viewType vt, Point2D at, double scale)
        {
            // ⚠ Insert this view exactly the way AddSheet.StandardFrame inserts its Front/Top/Left views.
            //   If your edition needs a different call, only this line changes.
            var v = new VectorView(at.X, at.Y, vt, scale, name);
            sheet.Entities.Add(v);
        }
        /// ORIGINAL model point → sheet point on the ELEVATION view.
        /// The model was turned 180° about Z around c, so X is mirrored; Z is unchanged.
        private Point2D ElevToSheet(Point3D p)
        {
            var v = views["ELEV"];
            double sc = v.Scale;
            return new Point2D(v.X + (c.X - p.X) * sc,
                               v.Y + (p.Z - c.Z) * sc);
        }

        /// <summary>ORIGINAL model point (before the 180° turn) → sheet point in a given view.</summary>
        private Point2D P(string view, Point3D p)
        {
            View v = views[view];
            double s = v.Scale;
            double rx = 2 * c.X - p.X;              // after 180° about Z through the centre
            double ry = 2 * c.Y - p.Y;
            double u, w;
            switch (view)
            {
                case "PLAN": u = rx - c.X; w = ry - c.Y; break;   // Top:   +X right, +Y up
                case "ELEV": u = rx - c.X; w = p.Z - c.Z; break;   // Front: +X right, +Z up
                default: u = ry - c.Y; w = p.Z - c.Z; break;   // Right: +Y right, +Z up
            }
            return new Point2D(v.X + u * s, v.Y + w * s);
        }

        private Point2D P(string view, double x, double y, double z) => P(view, new Point3D(x, y, z));

        private (double L, double R, double B, double T) Box(string view)
        {
            View v = views[view];
            var (w, h) = ext[view];
            return (v.X - w * v.Scale / 2, v.X + w * v.Scale / 2,
                    v.Y - h * v.Scale / 2, v.Y + h * v.Scale / 2);
        }
        #endregion

        /// ORIGINAL model point → sheet point on the PLAN view.
        /// Model was turned 180° about Z around c → both X and Y are mirrored.
        private Point2D PlanToSheet(Point3D p)
        {
            var v = views["PLAN"];
            double sc = v.Scale;
            return new Point2D(v.X + (c.X - p.X) * sc,
                               v.Y + (c.Y - p.Y) * sc);
        }

  
        /// <summary>
        /// Plan view: brick wall strip at (EC rear + WallDistance) with a gap per duct run,
        /// "xxx Sq. C/S" label beside each cut-out and the "1000" From-wall dimension.
        /// toSheet = ORIGINAL model point → sheet point on the PLAN view (handles the 180° turn).
        /// </summary>
        private void AddWallPlan(IList<Entity> ents, ClientGAInfo info,
                                 Func<Point3D, Point2D> toSheet, string layer = null)
        {
            if (info.WallDistance <= 0) return;
            layer ??= LDim;

            double txt = 2.5, off = 8, arr = 1.5;
            double ecRear = info.BoothDepth + info.ECDepth;
            double wx0 = ecRear + info.WallDistance;          // inner face
            double wx1 = wx0 + info.WallThickness;            // outer face
            double W = info.BoothWidth;
            double y0 = -info.WallMargin, y1 = W + info.WallMargin;
            double cut = info.DuctCutSize;
            int runs = Math.Max(1, info.DuctRuns);

            #region helpers
            Point2D P(double x, double y) => toSheet(new Point3D(x, y, 0));

            void L(Point2D a, Point2D b, Color? col = null)
            {
                var ln = new Line(a.X, a.Y, b.X, b.Y) { LayerName = layer };
                if (col.HasValue) { ln.Color = col.Value; ln.ColorMethod = colorMethodType.byEntity; }
                ents.Add(ln);
            }

            void Arrow(Point2D tip, double dx, double dy)               // (dx,dy) = pointing direction
            {
                double bx = tip.X - dx * arr, by = tip.Y - dy * arr;
                L(tip, new Point2D(bx - dy * arr * 0.35, by + dx * arr * 0.35));
                L(tip, new Point2D(bx + dy * arr * 0.35, by - dx * arr * 0.35));
            }

            void T(Point2D at, string s, Text.alignmentType al) =>
                ents.Add(new Text(at.X, at.Y, 0, s, txt, al) { LayerName = layer });

            // vertical text (reads bottom → top); bodyLeft = text body extends to the LEFT of 'at'
            void TV(Point2D at, string s, bool bodyLeft)
            {
                var al = bodyLeft ? Text.alignmentType.BottomCenter : Text.alignmentType.TopCenter;
                var pl = new Plane(new Point3D(at.X, at.Y, 0), Vector3D.AxisY, new Vector3D(-1, 0, 0));
                ents.Add(new Text(pl, new Point3D(at.X, at.Y, 0), s, txt) { LayerName = layer, Alignment = al });
            }
            #endregion

            #region wall strip with gaps at the duct runs
            var red = Color.SaddleBrown;

            // gaps along Y (one per run), sorted
            var gaps = new List<(double a, double b)>();
            if (info.DuctCutZ > 0 && cut > 0)
            {
                double pieceW = W / runs;
                for (int s = 0; s < runs; s++)
                {
                    double yc = s * pieceW + pieceW / 2;
                    gaps.Add((yc - cut / 2, yc + cut / 2));
                }
            }

            void WallBand(double ya, double yb)
            {
                if (yb - ya < 1) return;
                var a = P(wx0, ya); var b = P(wx1, ya); var c2 = P(wx1, yb); var d = P(wx0, yb);
                L(a, b, red); L(b, c2, red); L(c2, d, red); L(d, a, red);

                int course = 0;
                for (double y = ya + 150; y < yb; y += 150, course++)   // brick joints along the wall
                {
                    L(P(wx0, y), P(wx1, y), red);
                    double jx = wx0 + info.WallThickness * (course % 2 == 0 ? 0.5 : 0.25);
                    L(P(jx, y), P(jx, Math.Min(y + 150, yb)), red);
                }
            }

            double cur = y0;
            foreach (var g in gaps.OrderBy(g => g.a))
            {
                WallBand(cur, g.a);
                cur = g.b;
            }
            WallBand(cur, y1);
            #endregion

            // sheet direction inner → outer face (handles the mirror)
            double sgnX = Math.Sign(P(wx1, 0).X - P(wx0, 0).X);
            if (sgnX == 0) sgnX = 1;

            #region "xxx Sq. C/S" beside each cut-out (outer side, text body away from the wall)
            foreach (var g in gaps)
            {
                double ym = (g.a + g.b) / 2;
                TV(new Point2D(P(wx1, ym).X + sgnX * 2, P(wx1, ym).Y), $"{cut:0} Sq. C/S", bodyLeft: sgnX < 0);
            }
            #endregion

            #region "1000" From-wall dimension under the booth edge
            // put it on the sheet-bottom side of the booth
            double yEdge = P(ecRear, 0).Y < P(ecRear, W).Y ? 0 : W;
            var e0 = P(ecRear, yEdge); var e1 = P(wx0, yEdge);
            double yd = Math.Min(e0.Y, e1.Y) - off * 2;

            L(e0, new Point2D(e0.X, yd - 2)); L(e1, new Point2D(e1.X, yd - 2));
            L(new Point2D(e0.X, yd), new Point2D(e1.X, yd));
            Arrow(new Point2D(e0.X, yd), e0.X < e1.X ? -1 : 1, 0);
            Arrow(new Point2D(e1.X, yd), e0.X < e1.X ? 1 : -1, 0);
            T(new Point2D((e0.X + e1.X) / 2, yd + 1), $"{info.WallDistance:0}", Text.alignmentType.BottomCenter);
            #endregion
        }

        /// <summary>
        /// Elevation: "xxxx / E/C height" vertical dimension just outside the EC rear face.
        /// Value on the EC side of the line, label on the wall side — never on top of the line.
        /// </summary>
        private void AddEcHeightDim(IList<Entity> ents, ClientGAInfo info,
                                    Func<Point3D, Point2D> toSheet, string layer = null)
        {
            if (info.ExtractionHeight <= 0) return;
            layer ??= LDim;

            double txt = 2.5, arr = 1.5, off = 8;
            double ecRear = info.BoothDepth + info.ECDepth;

            Point2D P(double x, double z) => toSheet(new Point3D(x, 0, z));
            void L(Point2D a, Point2D b) => ents.Add(new Line(a.X, a.Y, b.X, b.Y) { LayerName = layer });
            void Arrow(Point2D tip, double dx, double dy)
            {
                double bx = tip.X - dx * arr, by = tip.Y - dy * arr;
                L(tip, new Point2D(bx - dy * arr * 0.35, by + dx * arr * 0.35));
                L(tip, new Point2D(bx + dy * arr * 0.35, by - dx * arr * 0.35));
            }
            void TV(Point2D at, string s, bool bodyLeft)
            {
                var al = bodyLeft ? Text.alignmentType.BottomCenter : Text.alignmentType.TopCenter;
                var pl = new Plane(new Point3D(at.X, at.Y, 0), Vector3D.AxisY, new Vector3D(-1, 0, 0));
                ents.Add(new Text(pl, new Point3D(at.X, at.Y, 0), s, txt) { LayerName = layer, Alignment = al });
            }

            double sgn = Math.Sign(P(ecRear + 100, 0).X - P(ecRear, 0).X);   // EC → wall direction on sheet
            if (sgn == 0) sgn = 1;

            var b0 = P(ecRear, 0);
            var b1 = P(ecRear, info.ExtractionHeight);
            double xd = b0.X + sgn * off;

            L(b1, new Point2D(xd + sgn * 2, b1.Y));
            L(new Point2D(xd, b0.Y), new Point2D(xd, b1.Y));
            Arrow(new Point2D(xd, b0.Y), 0, -1);
            Arrow(new Point2D(xd, b1.Y), 0, 1);

            double midY = (b0.Y + b1.Y) / 2;
            TV(new Point2D(xd - sgn * 1, midY), $"{info.ExtractionHeight:0}", bodyLeft: sgn > 0);   // EC side
            TV(new Point2D(xd + sgn * 1, midY), "E/C height", bodyLeft: sgn < 0);                  // wall side
        }
        /// ORIGINAL model point → sheet point on the SIDE (R.H.) view.
        /// Horizontal on the sheet = model Y, vertical = Z. The wall/duct layout is symmetric about
        /// the booth centre in Y, so the Y direction sign does not change the result.
        private Point2D SideToSheet(Point3D p)
        {
            var v = views["SIDE"];
            double sc = v.Scale;
            return new Point2D(v.X + (p.Y - c.Y) * sc,
                               v.Y + (p.Z - c.Z) * sc);
        }
        /// <summary>
        /// Side view: red brick courses on the parts of the site wall visible behind the booth —
        /// the strips beyond each booth side and the band above the booth roof (skipping duct cut-outs).
        /// The wall outline itself comes from the 3D SiteWall projection.
        /// </summary>
        private void AddWallSide(IList<Entity> ents, ClientGAInfo info,
                                 Func<Point3D, Point2D> toSheet, string layer = null)
        {
            if (info.WallDistance <= 0) return;
            layer ??= LDim;

            double W = info.BoothWidth;
            double bend = Math.Max(0, (info.OverallWidth - W) / 2);
            double yB0 = -bend, yB1 = W + bend;                       // booth silhouette in Y
            double boothTop = info.OverallHeight;                      // booth silhouette top
            double y0 = -info.WallMargin, y1 = W + info.WallMargin;    // wall extent
            double topH = info.DuctHeightFromFFL > 0 ? info.DuctHeightFromFFL : info.DuctTopZ;
            double wallTop = Math.Max(topH, info.DuctTopZ) + 300;      // same as 3D wall

            // duct cut-outs (Y range + Z range) — no bricks inside them
            var cuts = new List<(double ya, double yb, double za, double zb)>();
            if (info.DuctCutZ > 0 && info.DuctCutSize > 0)
            {
                int runs = Math.Max(1, info.DuctRuns);
                double pieceW = W / runs, h = info.DuctCutSize / 2;
                for (int s = 0; s < runs; s++)
                {
                    double yc = s * pieceW + pieceW / 2;
                    cuts.Add((yc - h, yc + h, info.DuctCutZ - h, info.DuctCutZ + h));
                }
            }

            Point2D P(double y, double z) => toSheet(new Point3D(0, y, z));
            void L(double ya, double za, double yb, double zb)
            {
                var a = P(ya, za); var b = P(yb, zb);
                ents.Add(new Line(a.X, a.Y, b.X, b.Y)
                { LayerName = layer, Color = Color.SaddleBrown, ColorMethod = colorMethodType.byEntity });
            }

            // horizontal course line from ya→yb at height z, broken around any cut-out at that height
            void Course(double ya, double yb, double z)
            {
                double cur = ya;
                foreach (var c2 in cuts.Where(k => z > k.za && z < k.zb && k.yb > ya && k.ya < yb).OrderBy(k => k.ya))
                {
                    if (c2.ya > cur) L(cur, z, c2.ya, z);
                    cur = Math.Max(cur, c2.yb);
                }
                if (yb > cur) L(cur, z, yb, z);
            }

            bool InCut(double y, double z) => cuts.Any(k => y > k.ya && y < k.yb && z > k.za && z < k.zb);

            // brick courses (75 high) with staggered vertical joints (230 long) inside a rectangle
            void Bricks(double ya, double yb, double za, double zb)
            {
                if (yb - ya < 1 || zb - za < 1) return;
                int course = 0;
                for (double z = za + 75; z < zb; z += 75, course++)
                {
                    Course(ya, yb, z);
                    double start = ya + (course % 2 == 0 ? 0 : 115);
                    for (double y = start + 230; y < yb; y += 230)
                        if (!InCut(y, z + 37)) L(y, z, y, Math.Min(z + 75, zb));
                }
            }

            Bricks(y0, yB0, 0, wallTop);              // strip beyond one booth side
            Bricks(yB1, y1, 0, wallTop);              // strip beyond the other side
            Bricks(yB0, yB1, boothTop, wallTop);      // band above the booth roof
        }
        #region Dimensions (client style)
        private void DrawDimensions(PaintBoothModel m, ClientGAInfo info, double topOfBlowerZ)
        {
            double D = m.D, D3 = m.D3, W = m.W, H = m.H, ECh = info.ExtractionHeight;
            double Wov = info.OverallWidth > 0 ? info.OverallWidth : W;
            double Hov = info.OverallHeight > 0 ? info.OverallHeight : H;
            double wo = (Wov - W) / 2;
            string t = info.BoothType;

            bool semiDown = t == "8" || t == "9";
            bool hasEC = t != "4" && t != "5";
            var xs = semiDown ? new List<double> { -D3, 0, D, D + D3 }
                   : hasEC ? new List<double> { 0, D, D + D3 }
                   : new List<double> { 0, D };

            string SegLabel(double a, double b) =>
                Math.Abs(Math.Abs(b - a) - D3) < 0.5 && hasEC ? "E/C" : "";

            #region PLAN
            if (views.ContainsKey("PLAN"))
            {
                var b = Box("PLAN");
                for (int i = 0; i < xs.Count - 1; i++)
                    DimH("PLAN", new Point3D(xs[i], W, 0), new Point3D(xs[i + 1], W, 0), b.B - 10, SegLabel(xs[i], xs[i + 1]));
                DimH("PLAN", new Point3D(xs[0], W, 0), new Point3D(xs[^1], W, 0), b.B - 20, "APPROX.");

                // open front (orig x = 0) is on the RIGHT after the turn
                DimV("PLAN", new Point3D(0, 0, 0), new Point3D(0, W, 0), b.R + 10, "");
                DimV("PLAN", new Point3D(0, -wo, 0), new Point3D(0, W + wo, 0), b.R + 20, "");
                VText(b.R + 30, (b.B + b.T) / 2, "ENTRY/EXIT FRONT OPEN", TXT, Text.alignmentType.MiddleCenter, LTitle);
            }
            #endregion

            #region ELEVATION
            if (views.ContainsKey("ELEV"))
            {
                var b = Box("ELEV");
                for (int i = 0; i < xs.Count - 1; i++)
                    DimH("ELEV", new Point3D(xs[i], 0, 0), new Point3D(xs[i + 1], 0, 0), b.B - 10, SegLabel(xs[i], xs[i + 1]));
                DimH("ELEV", new Point3D(xs[0], 0, 0), new Point3D(xs[^1], 0, 0), b.B - 20, "APPROX.");

                // right = open front
                DimV("ELEV", new Point3D(0, 0, 0), new Point3D(0, 0, H), b.R + 10, "Working height");
                DimV("ELEV", new Point3D(0, 0, 0), new Point3D(0, 0, Hov), b.R + 20, "");

                // left = E/C side → now drawn next to the EC / wall by AddEcHeightDim + AddWallAndFflDims
                //double xl = xs[^1];
                //if (hasEC && ECh > 0)
                //    DimV("ELEV", new Point3D(xl, 0, 0), new Point3D(xl, 0, ECh), b.L - 10, "E/C height");
                //DimV("ELEV", new Point3D(xl, 0, 0), new Point3D(xl, 0, topOfBlowerZ), b.L - 20, "Approx. from FFL");
            }
            #endregion

            #region SIDE VIEW (R.H.)
            if (views.ContainsKey("SIDE"))
            {
                var b = Box("SIDE");
                DimH("SIDE", new Point3D(0, 0, 0), new Point3D(0, W, 0), b.B - 10, "Inside working width");
                DimH("SIDE", new Point3D(0, -wo, 0), new Point3D(0, W + wo, 0), b.B - 20, "");

                // orig y = W is on the LEFT in this view
                DimV("SIDE", new Point3D(0, W, 0), new Point3D(0, W, H), b.L - 10, "Inside working height");
                DimV("SIDE", new Point3D(0, W, 0), new Point3D(0, W, Hov), b.L - 20, "");
            }
            #endregion
        }

        private void DimH(string view, Point3D a, Point3D b, double lineY, string sub = null)
        {
            Point2D p1 = P(view, a), p2 = P(view, b);
            if (p1.X > p2.X) (p1, p2) = (p2, p1);
            double val = Math.Abs(p2.X - p1.X) / views[view].Scale;
            if (val < 1) return;

            double d1 = lineY < p1.Y ? -1 : 1, d2 = lineY < p2.Y ? -1 : 1;
            Add(new Line(p1.X, p1.Y + d1, p1.X, lineY + d1 * EXT), LDim);
            Add(new Line(p2.X, p2.Y + d2, p2.X, lineY + d2 * EXT), LDim);
            Add(new Line(p1.X, lineY, p2.X, lineY), LDim);
            Arrow(new Point2D(p1.X, lineY), new Vector2D(-1, 0));
            Arrow(new Point2D(p2.X, lineY), new Vector2D(1, 0));

            double mid = (p1.X + p2.X) / 2;
            Txt(mid, lineY + 0.8, val.ToString("0"), TXT, Text.alignmentType.BottomCenter, LText);
            if (!string.IsNullOrEmpty(sub))
                Txt(mid, lineY - 0.8, sub, TXT * 0.8, Text.alignmentType.TopCenter, LText);
        }

        private void DimV(string view, Point3D a, Point3D b, double lineX, string sub = null)
        {
            Point2D p1 = P(view, a), p2 = P(view, b);
            if (p1.Y > p2.Y) (p1, p2) = (p2, p1);
            double val = Math.Abs(p2.Y - p1.Y) / views[view].Scale;
            if (val < 1) return;

            double d1 = lineX < p1.X ? -1 : 1, d2 = lineX < p2.X ? -1 : 1;
            Add(new Line(p1.X + d1, p1.Y, lineX + d1 * EXT, p1.Y), LDim);
            Add(new Line(p2.X + d2, p2.Y, lineX + d2 * EXT, p2.Y), LDim);
            Add(new Line(lineX, p1.Y, lineX, p2.Y), LDim);
            Arrow(new Point2D(lineX, p1.Y), new Vector2D(0, -1));
            Arrow(new Point2D(lineX, p2.Y), new Vector2D(0, 1));

            double mid = (p1.Y + p2.Y) / 2;
            VText(lineX - 0.8, mid, val.ToString("0"), TXT, Text.alignmentType.BottomCenter, LText);
            if (!string.IsNullOrEmpty(sub))
                VText(lineX + 0.8, mid, sub, TXT * 0.8, Text.alignmentType.TopCenter, LText);
        }

        private void Arrow(Point2D tip, Vector2D dir)
        {
            dir.Normalize();
            Vector2D perp = new Vector2D(-dir.Y, dir.X);
            Point2D back = tip - dir * ARW;
            var r = devregion.CreatePolygon(Plane.XY, new Point2D[]
            {
                tip, back + perp * (ARW * 0.3), back - perp * (ARW * 0.3)
            });
            Add(r, LDim);
        }
        #endregion

        #region Floor line, balloons, view titles
        private void FloorLine(string view, bool markerLeft)
        {
            var b = Box(view);
            double fy = P(view, 0, 0, 0).Y;
            double xa = markerLeft ? b.L - 36 : b.L - 5;
            double xb = markerLeft ? b.R + 5 : b.R + 20;
            Add(new Line(xa, fy, xb, fy), LFloor);

            double fx = markerLeft ? b.L - 32 : b.R + 15;
            Add(devregion.CreatePolygon(Plane.XY, new Point2D[]
            {
        new Point2D(fx, fy), new Point2D(fx - 2, fy + 3), new Point2D(fx + 2, fy + 3)
            }), LFloor);
            Txt(fx, fy + 4, "FFL", 2, Text.alignmentType.BottomCenter, LFloor);
        }
        /// <summary>
        /// Reads a DWG, centres it at (cx, cy) and scales it to fit maxW × maxH (paper mm).
        /// keepColors = true keeps the DWG's own colours (red logo, white text…); false paints it 'color'.
        /// </summary>
        private bool InsertDwgFitted(DrawingDocument doc, string file, double cx, double cy,
                                     double maxW, double maxH, string layer,
                                     bool keepColors, Color color = default)
        {
            if (!File.Exists(file)) return false;

            var ra = new ReadAutodesk(file);
            ra.DoWork();

            // resolve "by layer" colours from the DWG's own layers before we move everything to our layer
            Color Resolve(Entity e)
            {
                if (e.ColorMethod != colorMethodType.byLayer) return e.Color;
                var l = ra.Layers?.FirstOrDefault(x => x.Name == e.LayerName);
                return l != null ? l.Color : Color.White;
            }

            foreach (Block b in ra.Blocks)
                foreach (Entity e in b.Entities)
                {
                    if (keepColors) { e.Color = Resolve(e); e.ColorMethod = colorMethodType.byEntity; }
                    else e.ColorMethod = colorMethodType.byParent;
                    e.LayerName = "0";
                }

            // measure
            var tmp = new DesignDocument();
            foreach (Block b in ra.Blocks) if (!tmp.Blocks.Contains(b.Name)) tmp.Blocks.Add(b);
            var parts = new List<(Entity e, Color c)>();
            foreach (Entity e in ra.Entities)
            {
                Color col = keepColors ? Resolve(e) : color;
                Entity c = (Entity)e.Clone();
                c.LayerName = "0";
                parts.Add((c, col));
                tmp.Entities.Add(c);
            }
            tmp.Entities.Regen();
            tmp.Entities.UpdateBoundingBox();
            Point3D mn = tmp.Entities.BoxMin, mx = tmp.Entities.BoxMax;
            double w = mx.X - mn.X, h = mx.Y - mn.Y;
            if (w <= 0 || h <= 0) return false;

            double sc = Math.Min(maxW / w, maxH / h);
            Transformation tr = new Translation(cx, cy, 0) *
                                new Scaling(Point3D.Origin, sc) *
                                new Translation(-(mn.X + mx.X) / 2, -(mn.Y + mx.Y) / 2, 0);

            foreach (Block b in ra.Blocks) if (!doc.Blocks.Any(x => x.Name == b.Name)) doc.Blocks.Add(b);
            foreach (var (c, col) in parts)
            {
                tmp.Entities.Remove(c);
                c.TransformBy(tr);
                AddC(c, layer, col);
            }
            return true;
        }

        private void LogoFromDwg(DrawingDocument doc, double cx, double cy, double size)
        {
            string file = Path.Combine(_configuration["StpFilePathConfig:DrawingPath"] ?? "", "Logo.dwg");
            InsertDwgFitted(doc, file, cx, cy, size, size, LTB, keepColors: false, Color.White);
        }
        private void DrawBalloons(ClientGAInfo info)
        {
            const double r = 4.5;                                   // balloon radius (paper mm)
            Color dark = Color.FromArgb(15, 35, 160);               // outer blue
            Color light = Color.FromArgb(170, 205, 255);            // highlight

            foreach (var bl in info.Balloons)
            {
                if (!views.ContainsKey(bl.View)) continue;
                Point2D t = P(bl.View, bl.Target);
                Point2D cc = new Point2D(t.X + bl.Dx, t.Y + bl.Dy);

                // red leader + red target dot (drawn first, balloon sits on top)
                double dx = t.X - cc.X, dy = t.Y - cc.Y, len = Math.Sqrt(dx * dx + dy * dy);
                if (len > r)
                    AddC(new Line(cc.X + dx / len * r, cc.Y + dy / len * r, t.X, t.Y), LBal, Color.SaddleBrown);
                FillDisc(t.X, t.Y, 1.0, Color.SaddleBrown, Color.Red, 0);

                // shaded ball
                FillDisc(cc.X, cc.Y, r, dark, light, 0.35);
                AddC(new Circle(new Point3D(cc.X, cc.Y, 0), r), LBal, Color.FromArgb(10, 25, 120));

                // black box + white number
                string no = bl.No.ToString();
                double th = 3.2;
                double bw = th * 0.75 * no.Length + 0.8, bh = th * 1.3;
                FillRect(cc.X - bw / 2, cc.Y - bh / 2, cc.X + bw / 2, cc.Y + bh / 2, Color.Black);

                // white text, drawn twice slightly offset = bold look
                foreach (double ox in new[] { -0.06, 0.06 })
                {
                    var tx = new Text(cc.X + ox, cc.Y, 0, no, th, Text.alignmentType.MiddleCenter);
                    AddC(tx, LBal, Color.White);
                }
            }
        }

        /// <summary>Solid disc made of concentric circles; colour runs outer → inner,
        /// centre drifts toward top-left by 'highlight' × r for a 3D ball look.</summary>
        private void FillDisc(double cx, double cy, double r, Color outer, Color inner, double highlight)
        {
            const double step = 0.12;                               // circle spacing (paper mm)
            int n = Math.Max(2, (int)(r / step));
            for (int i = 0; i < n; i++)
            {
                double t = (double)i / (n - 1);                     // 0 = outer, 1 = centre
                double rr = r * (1 - t) + 0.05;
                double ox = -highlight * r * t, oy = highlight * r * t;
                Color c = Color.FromArgb(
                    (int)(outer.R + (inner.R - outer.R) * t),
                    (int)(outer.G + (inner.G - outer.G) * t),
                    (int)(outer.B + (inner.B - outer.B) * t));
                AddC(new Circle(new Point3D(cx + ox, cy + oy, 0), rr), LBal, c);
            }
        }

        /// <summary>Solid rectangle made of tight horizontal lines.</summary>
        private void FillRect(double x1, double y1, double x2, double y2, Color c)
        {
            const double step = 0.12;
            for (double y = y1; y <= y2; y += step)
                AddC(new Line(x1, y, x2, y), LBal, c);
            Rect(x1, y1, x2, y2, LBal);
        }

        /// <summary>Add with its own colour (not by layer).</summary>
        private void AddC(Entity e, string layer, Color c)
        {
            e.LayerName = layer;
            e.ColorMethod = colorMethodType.byEntity;
            e.Color = c;
            sheet.Entities.Add(e);
        }
        #region Operator + spray + airflow arrows (elevation)
        private void DrawElevationDecor(DrawingDocument doc, PaintBoothModel m, ClientGAInfo info)
        {
            if (!views.ContainsKey("ELEV")) return;
            TubeLights("ELEV", m, info.Lights, info.PanelWidth);
            BafflesAndFilter("ELEV", m, info);
            double D = m.D, H = m.H;

            int rows = Math.Max(3, (int)((H - 600) / 300));
            for (int i = 0; i < rows; i++)
            AirflowArrow("ELEV", new Point3D(D - 350, 0, 450 + i * 300), 420, pointLeft: true);

            AirflowArrow("ELEV", new Point3D(250, 0, H - 300), 420, pointLeft: true);
            AirflowArrow("ELEV", new Point3D(250, 0, H - 600), 420, pointLeft: true);

            var feet = new Point3D(Math.Min(D * 0.3, 900), 0, 0);
            if (!OperatorFromDwg(doc, "ELEV", feet, mirror: false))
                Operator("ELEV", feet, 1750);          // fallback: simple outline

            // maintenance door label (only when the door is on the wall the elevation sees)
            if (m.chkServiceDoor && (m.ServiceDoorLocation == "LeftSide" || m.ServiceDoorLocation == "Both Side"))
            {
                Point2D dc = P("ELEV", new Point3D(m.D + m.D3 / 2, 0, 100 + m.DoorHeight / 2));
                Txt(dc.X, dc.Y + 3, "MAINTAINANCE DOOR", 2.2, Text.alignmentType.MiddleCenter, LText);
            }
        }

        /// <summary>Operator figure facing sheet-left with spray gun + spray fan. feet = model point on floor.</summary>
        private void Operator(string view, Point3D feet, double heightMm)
        {
            Point2D o = P(view, feet);
            double k = heightMm * views[view].Scale;                 // figure height on paper
            Point3D Q(double u, double v) => new Point3D(o.X + u * k, o.Y + v * k, 0);
            void Poly(params (double u, double v)[] pts) =>
                Add(new LinearPath(pts.Select(p => Q(p.u, p.v)).ToArray()), LOper);

            // head
            Add(new Circle(Q(0.0, 0.925), 0.065 * k), LOper);

            // torso
            Poly((-0.09, 0.83), (-0.10, 0.65), (-0.08, 0.50), (0.09, 0.50), (0.11, 0.65), (0.10, 0.83), (-0.09, 0.83));

            // legs (one outline: front leg down, crotch, back leg)
            Poly((-0.08, 0.50), (-0.07, 0.03), (-0.14, 0.0), (-0.01, 0.0), (0.0, 0.44),
                 (0.02, 0.0), (0.12, 0.0), (0.08, 0.03), (0.09, 0.50));

            // arms reaching forward to the gun
            Poly((-0.06, 0.80), (-0.17, 0.68), (-0.27, 0.71));
            Poly((-0.02, 0.77), (-0.14, 0.64), (-0.26, 0.67));

            // spray gun: body, handle, paint cup
            Poly((-0.27, 0.72), (-0.36, 0.72), (-0.36, 0.69), (-0.27, 0.69), (-0.27, 0.72));
            Poly((-0.29, 0.69), (-0.30, 0.63), (-0.28, 0.63), (-0.27, 0.69));
            Poly((-0.30, 0.72), (-0.31, 0.78), (-0.27, 0.78), (-0.28, 0.72));

            // spray fan from the nozzle
            Point3D nz = Q(-0.36, 0.705);
            for (int i = -3; i <= 3; i++)
            {
                double a = Math.PI + i * (15.0 / 3) * Math.PI / 180;   // ±15° around "left"
                double len = 0.22 * k;
                Add(new Line(nz, new Point3D(nz.X + Math.Cos(a) * len, nz.Y + Math.Sin(a) * len, 0)), LOper);
            }
        }

        /// <summary>
        /// Inserts the client's operator figure from Operator.dwg (WBLOCKed at real size, base point = floor under feet).
        /// Returns false if the file is missing so the simple figure can be used instead.
        /// </summary>
        private bool OperatorFromDwg(DrawingDocument doc, string view, Point3D feet, bool mirror = false)
        {
            string dir = _configuration["StpFilePathConfig:DrawingPath"];
            string file = Path.Combine(dir ?? "", "Operator.dwg");
            if (!File.Exists(file)) return false;

            var ra = new ReadAutodesk(file);
            ra.DoWork();

            // nested blocks (if the figure contains any) — put their contents on layer 0 so no "Invalid Layer" error
            foreach (Block b in ra.Blocks)
            {
                if (doc.Blocks.Any(x => x.Name == b.Name)) continue;
                foreach (Entity e in b.Entities)
                {
                    e.LayerName = "0";
                    e.ColorMethod = colorMethodType.byParent;
                }
                doc.Blocks.Add(b);
            }

            // model mm → paper: scale by the view scale, optional mirror, then move onto the floor point
            Point2D o = P(view, feet);
            Transformation tr = new Scaling(Point3D.Origin, views[view].Scale);
            if (mirror) tr = tr * new Mirror(Plane.YZ);
            tr = new Translation(o.X, o.Y, 0) * tr;

            foreach (Entity e in ra.Entities)
            {
                Entity c = (Entity)e.Clone();
                c.TransformBy(tr);
                Add(c, LOper);          // our layer → same blue-violet as the client
            }
            return true;
        }

        /// <summary>Curved, tapered airflow arrow (client style). at = model point at the arrow's middle.</summary>
        private void AirflowArrow(string view, Point3D at, double lengthMm, bool pointLeft)
        {
            Point2D o = P(view, at);
            double L = lengthMm * views[view].Scale;
            double sg = pointLeft ? 1 : -1;
            Point2D S(double u, double v) => new Point2D(o.X + sg * (u - 0.55) * L, o.Y + (v - 0.15) * L);

            // quadratic Bezier: tail upper-right → sweeps down → head at left
            (double x, double y) P0 = (1.0, 0.35), P1 = (0.45, -0.10), P2 = (0.15, 0.0);
            const int N = 16;
            var upper = new List<Point2D>();
            var lower = new List<Point2D>();
            double hx = 0, hy = 0;                                     // tangent at the head

            for (int i = 0; i <= N; i++)
            {
                double t = (double)i / N, a = (1 - t) * (1 - t), b = 2 * (1 - t) * t, cc = t * t;
                double x = a * P0.x + b * P1.x + cc * P2.x;
                double y = a * P0.y + b * P1.y + cc * P2.y;
                double tx = 2 * (1 - t) * (P1.x - P0.x) + 2 * t * (P2.x - P1.x);
                double ty = 2 * (1 - t) * (P1.y - P0.y) + 2 * t * (P2.y - P1.y);
                double len = Math.Sqrt(tx * tx + ty * ty);
                double nx = -ty / len, ny = tx / len;
                double w = 0.008 + 0.035 * t;                          // thin tail → thick near head
                upper.Add(S(x + nx * w, y + ny * w));
                lower.Add(S(x - nx * w, y - ny * w));
                if (i == N) { hx = tx / len; hy = ty / len; }
            }
            lower.Reverse();
            Add(devregion.CreatePolygon(Plane.XY, upper.Concat(lower).ToArray()), LAirBand);

            // arrowhead
            double nX = -hy, nY = hx;
            Add(devregion.CreatePolygon(Plane.XY, new Point2D[]
            {
        S(P2.x + hx * 0.16, P2.y + hy * 0.16),                // tip
        S(P2.x + nX * 0.08, P2.y + nY * 0.08),
        S(P2.x - nX * 0.08, P2.y - nY * 0.08)
            }), LAirHead);
        }

        /// <summary>Client-style tube light fixtures hanging under the roof, spread evenly along the booth depth.</summary>
        /// <summary>
        /// Centres of the bays that get a fixture (orig model X, along depth).
        /// Bays = between panel joints: 0, PW, 2·PW … D (panels start from the front, remainder at the end).
        /// One fixture per bay; if qty is less than the bay count, the used bays are spread evenly.
        /// Returns (centre X, bay width).
        /// </summary>
        public static List<(double x, double bayW)> LightCentres(double D, double panelWidth, int qty)
        {
            var result = new List<(double, double)>();
            if (qty <= 0 || D <= 0) return result;

            var joints = new List<double> { 0 };
            if (panelWidth > 0)
                for (double x = panelWidth; x < D - 1; x += panelWidth) joints.Add(x);
            joints.Add(D);

            var bays = joints.Zip(joints.Skip(1), (a, b) => (a, b)).ToList();
            int along = Math.Min(qty, bays.Count);

            for (int k = 0; k < along; k++)
            {
                int idx = (int)Math.Round((k + 0.5) * bays.Count / along - 0.5);
                idx = Math.Clamp(idx, 0, bays.Count - 1);
                var (a, b) = bays[idx];
                result.Add(((a + b) / 2, b - a));
            }
            return result;
        }

        /// <summary>Client-style tube light fixtures, one centred in each panel bay.</summary>
        private void TubeLights(string view, PaintBoothModel m, int qty, double panelWidth,
                                double fixtureLen = 600, double fixtureH = 140)
        {
            double H = m.H;
            foreach (var (xc, bayW) in LightCentres(m.D, panelWidth, qty))
            {
                double len = Math.Min(fixtureLen, bayW * 0.65);    // never touches the joints

                Point2D tl = P(view, new Point3D(xc - len * 0.35, 0, H));
                Point2D tr = P(view, new Point3D(xc + len * 0.35, 0, H));
                Point2D br = P(view, new Point3D(xc + len * 0.50, 0, H - fixtureH));
                Point2D bl = P(view, new Point3D(xc - len * 0.50, 0, H - fixtureH));

                Add(new LinearPath(new Point3D[]
                {
            new Point3D(tl.X, tl.Y, 0), new Point3D(tr.X, tr.Y, 0),
            new Point3D(br.X, br.Y, 0), new Point3D(bl.X, bl.Y, 0),
            new Point3D(tl.X, tl.Y, 0)
                }), LLight);

                double lip = 0.8;
                Add(new Line(bl.X - lip, bl.Y, bl.X, bl.Y), LLight);
                Add(new Line(br.X, br.Y, br.X + lip, br.Y), LLight);
            }
        }

        /// <summary>Primary metal baffle + secondary P.A. filter at the booth / E/C boundary (seen through the side wall).</summary>
        private void BafflesAndFilter(string view, PaintBoothModel m, ClientGAInfo info)
        {
            if (!info.HasBaffles || !views.ContainsKey(view)) return;

            double D = m.D, ECh = info.ExtractionHeight;
            const double tierH = 1140;                               // BaffleHeight in OuterFilterFrame
            int tiers = (int)(ECh / tierH);
            double top = tiers > 0 ? tiers * tierH : ECh;

            double bx0 = D - 60, bx1 = D;                            // primary – metal baffle
            double fx0 = D + 100, fx1 = D + 150;                     // secondary – P.A. filter

            // frames
            RectM(view, bx0, 0, bx1, top, LText);
            RectM(view, bx0 + 8, 0, bx1 - 8, top, LText);            // inner line → channel look
            RectM(view, fx0, 0, fx1, top, LText);

            // top + bottom channels tying both
            RectM(view, bx0 - 10, top, fx1 + 10, top + 40, LText);
            RectM(view, bx0 - 10, 0, fx1 + 10, 30, LText);

            // tier joints
            for (int k = 1; k < tiers; k++)
            {
                LineM(view, bx0, k * tierH, bx1, k * tierH, LText);
                LineM(view, fx0, k * tierH, fx1, k * tierH, LText);
            }

            // P.A. filter media: blue diamond pattern (two opposite zig-zags)
            const double pitch = 35;
            for (int phase = 0; phase < 2; phase++)
            {
                var pts = new List<Point3D>();
                bool left = phase == 0;
                for (double z = 30; z <= top; z += pitch, left = !left)
                {
                    Point2D p = P(view, new Point3D(left ? fx0 : fx1, 0, z));
                    pts.Add(new Point3D(p.X, p.Y, 0));
                }
                if (pts.Count > 1) Add(new LinearPath(pts.ToArray()), LBaffle);
            }

            // labels with red leaders (inside the booth, low down, clear of the arrows)
            Label(view, new Point3D(D - 30, 0, 150), new Point3D(D - 450, 0, 300), "Primary (Metal Baffle)");
            Label(view, new Point3D((fx0 + fx1) / 2, 0, 80), new Point3D(D - 450, 0, 120), "Secondary (P.A. filter)");
        }
        /// <summary>
        /// Client-style elevation extras: brick wall with duct cut-out, "Wall C/O" label,
        /// "xxxx Approx. from FFL" at the cut-out and at the top of the duct,
        /// and bottom dims: 250 | 1000 From wall | overall APPROX.
        /// toSheet = the SAME model→sheet mapping the ELEV balloons use.
        /// </summary>
        /// <summary>
        /// Client-style elevation extras: brick wall with duct cut-out, "Wall C/O" label,
        /// "xxxx Approx. from FFL" at the cut-out and at the top of the duct,
        /// and bottom dims: 250 | 1000 From wall | overall APPROX.
        /// toSheet = ORIGINAL model point → sheet point on the ELEV view (handles the 180° turn).
        /// </summary>
        /// <summary>
        /// Client-style elevation extras: brick wall with duct cut-out, "Wall C/O" label,
        /// "xxxx Approx. from FFL" at the cut-out and at the top of the duct,
        /// and bottom dims: 250 | 1000 From wall | overall APPROX.
        /// toSheet = ORIGINAL model point → sheet point on the ELEV view (handles the 180° turn).
        /// </summary>
        private void AddWallAndFflDims(IList<Entity> ents, ClientGAInfo info,
                                       Func<Point3D, Point2D> toSheet, string layer = null)
        {
            if (info.WallDistance <= 0) return;
            layer ??= LDim;

            double txt = 2.5, off = 8, arr = 1.5;                       // paper mm
            double ecRear = info.BoothDepth + info.ECDepth;
            double wx0 = ecRear + info.WallDistance;                    // inner face (booth side)
            double wx1 = wx0 + info.WallThickness;                      // outer face (duct side)
            double topH = info.DuctHeightFromFFL > 0 ? info.DuctHeightFromFFL : info.DuctTopZ;
            double wallTop = Math.Max(topH, info.DuctTopZ) + 300;

            double cut = info.DuctCutSize, cz0 = 0, cz1 = 0;
            bool hasCut = info.DuctCutZ > 0 && cut > 0;
            if (hasCut) { cz0 = info.DuctCutZ - cut / 2; cz1 = info.DuctCutZ + cut / 2; }

            #region helpers
            Point2D P(double x, double z) => toSheet(new Point3D(x, 0, z));

            void L(Point2D a, Point2D b, Color? col = null)
            {
                var ln = new Line(a.X, a.Y, b.X, b.Y) { LayerName = layer };
                if (col.HasValue) { ln.Color = col.Value; ln.ColorMethod = colorMethodType.byEntity; }
                ents.Add(ln);
            }
            void Arrow(Point2D tip, double dx, double dy)
            {
                double bx = tip.X - dx * arr, by = tip.Y - dy * arr;
                L(tip, new Point2D(bx - dy * arr * 0.35, by + dx * arr * 0.35));
                L(tip, new Point2D(bx + dy * arr * 0.35, by - dx * arr * 0.35));
            }
            void T(Point2D at, string s, Text.alignmentType al) =>
                ents.Add(new Text(at.X, at.Y, 0, s, txt, al) { LayerName = layer });

            // vertical text (reads bottom → top). bodyLeft = text body extends to the LEFT of 'at';
            // centred = centred on 'at' vertically, otherwise starts at 'at' and runs upward.
            void TV(Point2D at, string s, bool bodyLeft, bool centred = true)
            {
                var al = bodyLeft
                    ? (centred ? Text.alignmentType.BottomCenter : Text.alignmentType.BottomLeft)
                    : (centred ? Text.alignmentType.TopCenter : Text.alignmentType.TopLeft);
                var pl = new Plane(new Point3D(at.X, at.Y, 0), Vector3D.AxisY, new Vector3D(-1, 0, 0));
                ents.Add(new Text(pl, new Point3D(at.X, at.Y, 0), s, txt) { LayerName = layer, Alignment = al });
            }
            #endregion

            #region brick wall (red), split around the cut-out
            var red = Color.SaddleBrown;
            void WallBand(double z0, double z1)
            {
                if (z1 - z0 < 1) return;
                var a = P(wx0, z0); var b = P(wx1, z1);
                L(a, new Point2D(b.X, a.Y), red); L(new Point2D(b.X, a.Y), b, red);
                L(b, new Point2D(a.X, b.Y), red); L(new Point2D(a.X, b.Y), a, red);

                int course = 0;
                for (double z = z0 + 75; z < z1; z += 75, course++)
                {
                    L(P(wx0, z), P(wx1, z), red);
                    double jx = wx0 + info.WallThickness * (course % 2 == 0 ? 0.33 : 0.66);
                    L(P(jx, z), P(jx, Math.Min(z + 75, z1)), red);
                }
            }
            if (hasCut) { WallBand(0, cz0); WallBand(cz1, wallTop); }
            else WallBand(0, wallTop);
            #endregion

            // sheet direction inner → outer face (handles the 180° mirror); outward text goes to this side
            double sgn = Math.Sign(P(wx1, 0).X - P(wx0, 0).X);
            if (sgn == 0) sgn = 1;
            bool outLeft = sgn < 0;

            #region "xxx SQ. Wall C/O" — along the wall's outer face, ABOVE the cut-out (clear of the duct)
            if (hasCut)
                TV(new Point2D(P(wx1, 0).X + sgn * 1, P(0, cz1 + 60).Y), $"{cut:0} SQ. Wall C/O",
                   bodyLeft: outLeft, centred: false);
            #endregion

            #region vertical "Approx. from FFL" dims on the outer side (text on the far side of the line)
            void VDim(double h, double baseX, double offPaper)
            {
                if (h <= 0) return;
                var b0 = P(baseX, 0); var b1 = P(baseX, h);
                double xd = b0.X + sgn * offPaper;
                L(b0, new Point2D(xd + sgn * 2, b0.Y));
                L(b1, new Point2D(xd + sgn * 2, b1.Y));
                L(new Point2D(xd, b0.Y), new Point2D(xd, b1.Y));
                Arrow(new Point2D(xd, b0.Y), 0, -1);
                Arrow(new Point2D(xd, b1.Y), 0, 1);
                TV(new Point2D(xd + sgn * 1, (b0.Y + b1.Y) / 2), $"{h:0} Approx. from FFL", bodyLeft: outLeft);
            }
            if (hasCut) VDim(info.DuctCutZ, wx1, off * 2);                              // e.g. 2795
            VDim(topH, Math.Max(info.DuctMaxX, wx1), off * 2);                          // e.g. 15000
            #endregion

            #region horizontal dims below floor
            double y1 = P(0, 0).Y - off * 2;            // row 1: 250 | 1000 From wall   (same row as E/C | booth)
            double y2 = y1 - off * 3.2;                 // row 3: overall — below your existing overall row
            void HDim(double x0, double x1, double y, string top, string bottom)
            {
                var p0 = P(x0, 0); var p1 = P(x1, 0);
                L(p0, new Point2D(p0.X, y - 2)); L(p1, new Point2D(p1.X, y - 2));
                L(new Point2D(p0.X, y), new Point2D(p1.X, y));
                Arrow(new Point2D(p0.X, y), p0.X < p1.X ? -1 : 1, 0);
                Arrow(new Point2D(p1.X, y), p0.X < p1.X ? 1 : -1, 0);
                T(new Point2D((p0.X + p1.X) / 2, y + 1), top, Text.alignmentType.BottomCenter);
                if (bottom != null) T(new Point2D((p0.X + p1.X) / 2, y - 1.5), bottom, Text.alignmentType.TopCenter);
            }
            HDim(wx0, wx1, y1, $"{info.WallThickness:0}", null);
            HDim(ecRear, wx0, y1, $"{info.WallDistance:0}", "From wall");
            HDim(0, wx0, y2, $"{wx0:0} APPROX.", null);
            #endregion
        }
        private void RectM(string view, double x0, double z0, double x1, double z1, string layer)
        {
            LineM(view, x0, z0, x1, z0, layer);
            LineM(view, x1, z0, x1, z1, layer);
            LineM(view, x1, z1, x0, z1, layer);
            LineM(view, x0, z1, x0, z0, layer);
        }

        private void LineM(string view, double x0, double z0, double x1, double z1, string layer)
        {
            Point2D a = P(view, new Point3D(x0, 0, z0)), b = P(view, new Point3D(x1, 0, z1));
            Add(new Line(a.X, a.Y, b.X, b.Y), layer);
        }

        /// <summary>Text at 'textAt' with a red leader to 'target' (both in model coords).</summary>
        private void Label(string view, Point3D target, Point3D textAt, string text)
        {
            Point2D t = P(view, target), a = P(view, textAt);
            AddC(new Line(a.X - 1, a.Y, t.X, t.Y), LBal, Color.SaddleBrown  );
            Txt(a.X, a.Y, text, 2.2, Text.alignmentType.MiddleLeft, LText);
        }
        #endregion
        private void ViewTitle(string view, string title)
        {
            if (!views.ContainsKey(view)) return;
            var b = Box(view);
            double cx = (b.L + b.R) / 2, y = b.B - 32;
            Txt(cx, y, title, 5, Text.alignmentType.TopCenter, LTitle);
            double half = title.Length * 5 * 0.45;
            Add(new Line(cx - half, y - 6.5, cx + half, y - 6.5), LTitle);
        }
        #endregion

        #region Frame, scope table, notes, title block
        private void DrawFrame()
        {
            Rect(0, 0, SW, SH, LFrame);
            Rect(ML, MO, SW - MO, SH - MO, LFrame);

            int cols = 16, rows = 6;
            double cw = (SW - MO - ML) / cols, rh = (SH - 2 * MO) / rows;
            for (int i = 0; i < cols; i++)
            {
                double x = ML + i * cw;
                if (i > 0)
                {
                    Add(new Line(x, MO, x, MO - 4), LFrame);
                    Add(new Line(x, SH - MO, x, SH - MO + 4), LFrame);
                }
                Txt(x + cw / 2, MO / 2, (i + 1).ToString(), 3, Text.alignmentType.MiddleCenter, LFrame);
                Txt(x + cw / 2, SH - MO / 2, (i + 1).ToString(), 3, Text.alignmentType.MiddleCenter, LFrame);
            }
            for (int j = 0; j < rows; j++)
            {
                double y = SH - MO - j * rh;
                if (j > 0)
                {
                    Add(new Line(ML, y, ML - 4, y), LFrame);
                    Add(new Line(SW - MO, y, SW - MO + 4, y), LFrame);
                }
                string zone = ((char)('A' + j)).ToString();
                Txt(ML / 2, y - rh / 2, zone, 3, Text.alignmentType.MiddleCenter, LFrame);
                Txt(SW - MO / 2, y - rh / 2, zone, 3, Text.alignmentType.MiddleCenter, LFrame);
            }
        }

        /// <returns>Y of the table's bottom edge.</returns>
        private double DrawScopeTable(ClientGAInfo info)
        {
            double x0 = SW - MO - RCOL - 5, x1 = SW - MO - 5, snW = 18, rowH = 5.5, pad = 2.5;
            double y = SH - MO - 5;

            Rect(x0, y - 10, x1, y, LText);
            Add(new Line(x0 + snW, y - 10, x0 + snW, y), LText);
            Txt(x0 + snW / 2, y - 5, "S.N.", 3.5, Text.alignmentType.MiddleCenter, LTitle);
            Txt(x0 + snW + 15, y - 5, "SCOPE OF SUPPLY", 3.5, Text.alignmentType.MiddleLeft, LTitle);
            y -= 10;

            for (int i = 0; i < info.Scope.Count; i++)
            {
                var it = info.Scope[i];
                int n = 1 + it.Lines.Count;
                double h = n * rowH + 2 * pad;

                Rect(x0, y - h, x1, y, LText);
                Add(new Line(x0 + snW, y - h, x0 + snW, y), LText);
                Txt(x0 + snW / 2, y - pad - rowH / 2, $"({i + 1})", 3, Text.alignmentType.MiddleCenter, LText);

                double ly = y - pad - rowH / 2;
                Txt(x0 + snW + 3, ly, it.Title, 3, Text.alignmentType.MiddleLeft, LText);
                foreach (var line in it.Lines)
                {
                    ly -= rowH;
                    Txt(x0 + snW + 3, ly, line, 3, Text.alignmentType.MiddleLeft, LText);
                }
                y -= h;
            }
            return y;
        }

        private void DrawNotes(ClientGAInfo info, double top, double bottom)
        {
            if (info.Notes.Count == 0) return;
            double x0 = SW - MO - RCOL - 5, x1 = SW - MO - 5, h = 2.2, lineH = 4;
            int maxChars = (int)((x1 - x0 - 6) / (h * 0.9));

            double y = top - 4;
            Txt(x0 + 3, y, "NOTES :-", 2.8, Text.alignmentType.MiddleLeft, LTitle);
            y -= lineH + 1;

            for (int i = 0; i < info.Notes.Count && y > bottom + 3; i++)
            {
                foreach (var line in Wrap($"{i + 1}) {info.Notes[i]}", maxChars))
                {
                    if (y <= bottom + 3) break;
                    Txt(x0 + 3, y, line, h, Text.alignmentType.MiddleLeft, LTitle);
                    y -= lineH;
                }
                y -= 1;
            }
            Rect(x0, Math.Max(y, bottom), x1, top, LTitle);
        }

        /// <returns>Y of the title block's top edge.</returns>
        /// <summary>Client title block. Layout copied from the client sheet (pixel grid 153..1460 × 107..397).</summary>
        /// <returns>Y of the title block's top edge.</returns>
        private double DrawTitleBlock(ClientGAInfo info, DrawingDocument doc)
        {
            double x0 = SW - MO - RCOL - 5;
            double k = RCOL / 1307.0;                       // px → paper mm
            double y0 = MO + 5;
            double top = y0 + 290 * k;

            double X(double px) => x0 + (px - 153) * k;
            double Y(double py) => top - (py - 107) * k;
            void HL(double py, double pxa, double pxb) => Add(new Line(X(pxa), Y(py), X(pxb), Y(py)), LText);
            void VL(double px, double pya, double pyb) => Add(new Line(X(px), Y(pya), X(px), Y(pyb)), LText);
            void TLf(double px, double py, string s, double h = 2.4) => Txt(X(px), Y(py), s ?? "", h, Text.alignmentType.MiddleLeft, LTB);
            void TC(double px, double py, string s, double h = 2.4) => Txt(X(px), Y(py), s ?? "", h, Text.alignmentType.MiddleCenter, LTB);

            #region grid
            Rect(X(153), Y(397), X(1460), Y(107), LText);

            VL(347, 107, 333); VL(516, 107, 397); VL(219, 172, 333); VL(283, 172, 333);
            VL(677, 107, 236); VL(833, 107, 397); VL(962, 107, 236); VL(1017, 365, 397);
            VL(1201, 107, 397); VL(1407, 281, 365);

            HL(172, 153, 1201); HL(204, 516, 962); HL(236, 516, 1201);
            HL(281, 1201, 1460); HL(333, 153, 516); HL(365, 516, 1460);
            #endregion

            #region tolerance + finish symbols
            TC(250, 126, "OPEN TOLERANCE"); TC(250, 152, "MEDIUM FINISH");
            TC(431, 126, "BREAK"); TC(431, 152, "CORNERS");

            string[,] tol =
            {
        { "-",    "10",   "%%p0.1" }, { "10",  "30",   "%%p0.2" }, { "30",   "100",  "%%p0.3" },
        { "100",  "300",  "%%p0.5" }, { "300", "1000", "%%p0.8" }, { "1000", "3000", "%%p1.2" },
    };
            double[] tolY = { 185, 212, 239, 266, 293, 320 };
            for (int r = 0; r < 6; r++)
            {
                TC(186, tolY[r], tol[r, 0]); TC(251, tolY[r], tol[r, 1]); TC(315, tolY[r], tol[r, 2]);
            }

            // ~ / ▽ / ▽▽ / ▽▽▽
            double[] symY = { 191, 232, 273, 313 };
            string[] symTxt = { "= DRESSED", "= ROUGH", "= MEDIUM", "= FINE" };
            for (int r = 0; r < 4; r++)
            {
                if (r == 0) Tilde(X(385), Y(symY[r]), 4.5 * k * 12);
                else
                {
                    double w = 12 * k * 1.1;
                    double sx = X(392) - (r * w) / 2;
                    for (int t = 0; t < r; t++) DownTriangle(sx + t * w + w / 2, Y(symY[r]), w);
                }
                TLf(413, symY[r], symTxt[r]);
            }

            TC(334, 352, "ALL DIMENSIONS ARE IN MILLIMETERS.");
            TC(334, 380, "UNLESS OTHERWISE SPECIFIED.");
            #endregion

            #region drawn / checked / approved + product / model
            TLf(530, 139, "DRAWN BY"); TC(755, 139, info.DrawnBy, 2.8); TC(897, 139, info.DrawnDate);
            TLf(530, 188, "CHECKED BY"); TC(755, 188, info.CheckedBy, 2.8); TC(897, 188, info.CheckedDate);
            TLf(530, 220, "APPROVED BY"); TC(755, 220, info.ApprovedBy, 2.8); TC(897, 220, info.ApprovedDate);

            TLf(975, 124, "PRODUCT:"); TC(1081, 154, info.Product, 2.8);
            TLf(975, 188, "MODEL:"); TC(1081, 220, info.Model, 2.8);
            #endregion

            #region client / part / bottom row
            TLf(530, 253, "CLIENT:"); TC(674, 300, info.Customer, 2.8);
            TLf(847, 253, "PART:");
            TC(1017, 292, string.IsNullOrEmpty(info.Part1) ? info.Title : info.Part1, 2.8);
            TC(1017, 322, info.Part2, 2.8);

            TLf(530, 381, "MAT'L.:"); TC(718, 381, info.Material);
            TLf(847, 381, "QTY.: " + info.Qty);
            TLf(1031, 381, "SCALE:"); TC(1160, 381, $"1:{scaleN}");
            #endregion

            #region logo + company, drg no, rev, sheet
            LogoFromDwg(doc, X(1332), Y(143), 64 * k);
            Txt(X(1332), Y(190), info.CompanyLine1, 3.4, Text.alignmentType.MiddleCenter, LTB);
            Txt(X(1332), Y(224), info.CompanyLine2, 3.4, Text.alignmentType.MiddleCenter, LTB);
            TC(1332, 255, info.CompanyLine3, 3.0);

            TLf(1215, 298, "DRG. NO.", 2.2); TC(1304, 335, info.DrawingNo, 2.8);
            TLf(1414, 298, "REV.", 2.2); TC(1433, 341, info.Revision, 2.8);
            TLf(1215, 381, "SHEET"); TC(1375, 381, info.SheetNo);

            VText(X(1460) + 3, Y(180), "SHEET A1", 3, Text.alignmentType.MiddleCenter, LTB);
            #endregion

            return top;
        }
        /// <summary>Bullows name plate on the front fascia of the side view (centred on booth width, at the top).</summary>
        /// <summary>Bullows name plate on the front fascia of the side view (centred on booth width, at the top).</summary>
        private void NamePlate(DrawingDocument doc, PaintBoothModel m, ClientGAInfo info,
                               double plateW = 600, double plateH = 200,
                               string tagline = "Surface Finishing Excellence",
                               string website = "www.bullows.com")
        {
            if (!views.ContainsKey("SIDE")) return;

            double topZ = info.OverallHeight > 0 ? info.OverallHeight : m.H;
            double zc = topZ - plateH / 2 + 40;
            Point2D c = P("SIDE", new Point3D(0, m.W / 2, zc));
            double s = views["SIDE"].Scale;
            double bw = plateW * s, bh = plateH * s;               // plate size on paper

            // 1) vector plate from the client DWG, if available
            string dwg = Path.Combine(_configuration["StpFilePathConfig:DrawingPath"] ?? "", "NamePlate.dwg");
            if (InsertDwgFitted(doc, dwg, c.X, c.Y, bw, bh, LTB, keepColors: true)) return;

            // 2) built plate: border + screws + logo.png (white removed) + 2 text lines
            string png = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "img", "logo.png");

            // double rounded border + screw holes
            double rr = bh * 0.08;
            RoundRect(c.X, c.Y, bw, bh, rr, LTitle);
            RoundRect(c.X, c.Y, bw - bh * 0.06, bh - bh * 0.06, rr * 0.7, LTitle);
            double sx = bw / 2 - bh * 0.10, sy = bh / 2 - bh * 0.10, sr = bh * 0.025;
            foreach (var (ix, iy) in new[] { (-1, 1), (1, 1), (-1, -1), (1, -1) })
            {
                double hx = c.X + ix * sx, hy = c.Y + iy * sy;
                Add(new Circle(new Point3D(hx, hy, 0), sr), LTitle);
                Add(new Line(hx - sr * 1.6, hy, hx + sr * 1.6, hy), LTitle);
                Add(new Line(hx, hy - sr * 1.6, hx, hy + sr * 1.6), LTitle);
            }

            // logo (upper ~60 % of the plate)
            if (File.Exists(png))
            {
                byte[] img = LogoWithoutWhite(png, out double ar);
                double maxW = bw * 0.80, maxH = bh * 0.55;
                double w = maxW, h = w * ar;
                if (h > maxH) { h = maxH; w = h / ar; }
                double logoCy = c.Y + bh / 2 - bh * 0.08 - h / 2;

                var pic = new Picture(Plane.XY, w, h, img);
                pic.Translate(c.X - w / 2, logoCy - h / 2, 0);
                Add(pic, LTB);
            }

            // two white lines under the logo (drawn twice, slightly offset → bold look)
            double t1 = bh * 0.10, t2 = bh * 0.12;
            foreach (double ox in new[] { -t1 * 0.02, t1 * 0.02 })
            {
                AddC(new Text(c.X + ox, c.Y - bh * 0.20, 0, tagline, t1, Text.alignmentType.MiddleCenter), LTB, Color.White);
                AddC(new Text(c.X + ox, c.Y - bh * 0.35, 0, website, t2, Text.alignmentType.MiddleCenter), LTB, Color.White);
            }
        }

        /// <summary>Loads the PNG, makes near-white pixels transparent (soft edge), crops empty margins, returns PNG bytes.</summary>
        private static byte[] LogoWithoutWhite(string png, out double aspect)
        {
            using var src = new Bitmap(png);
            using var bmp = new Bitmap(src.Width, src.Height, PixelFormat.Format32bppArgb);
            using (var g = Graphics.FromImage(bmp))
                g.DrawImage(src, new Rectangle(0, 0, src.Width, src.Height));

            int minX = bmp.Width, minY = bmp.Height, maxX = -1, maxY = -1;
            for (int y = 0; y < bmp.Height; y++)
                for (int x = 0; x < bmp.Width; x++)
                {
                    Color p = bmp.GetPixel(x, y);
                    int white = Math.Min(p.R, Math.Min(p.G, p.B));      // how "white" the pixel is
                    int a = p.A;
                    if (white >= 235) a = 0;                             // background → fully transparent
                    else if (white >= 200) a = Math.Min(a, (int)(255 * (235 - white) / 35.0)); // soft edge
                    if (a != p.A) bmp.SetPixel(x, y, Color.FromArgb(a, p.R, p.G, p.B));

                    if (a > 20) { minX = Math.Min(minX, x); maxX = Math.Max(maxX, x); minY = Math.Min(minY, y); maxY = Math.Max(maxY, y); }
                }

            Bitmap outBmp = bmp;
            if (maxX >= minX && maxY >= minY)                            // crop to the logo itself
                outBmp = bmp.Clone(new Rectangle(minX, minY, maxX - minX + 1, maxY - minY + 1), PixelFormat.Format32bppArgb);

            aspect = (double)outBmp.Height / outBmp.Width;
            using var ms = new MemoryStream();
            outBmp.Save(ms, ImageFormat.Png);
            if (!ReferenceEquals(outBmp, bmp)) outBmp.Dispose();
            return ms.ToArray();
        }

        /// <summary>Rounded rectangle centred at (cx, cy).</summary>
        private void RoundRect(double cx, double cy, double w, double h, double r, string layer)
        {
            double x0 = cx - w / 2, x1 = cx + w / 2, y0 = cy - h / 2, y1 = cy + h / 2;
            Add(new Line(x0 + r, y0, x1 - r, y0), layer);
            Add(new Line(x1, y0 + r, x1, y1 - r), layer);
            Add(new Line(x1 - r, y1, x0 + r, y1), layer);
            Add(new Line(x0, y1 - r, x0, y0 + r), layer);
            Add(new Arc(new Point3D(x1 - r, y0 + r, 0), r, -Math.PI / 2, 0), layer);
            Add(new Arc(new Point3D(x1 - r, y1 - r, 0), r, 0, Math.PI / 2), layer);
            Add(new Arc(new Point3D(x0 + r, y1 - r, 0), r, Math.PI / 2, Math.PI), layer);
            Add(new Arc(new Point3D(x0 + r, y0 + r, 0), r, Math.PI, 3 * Math.PI / 2), layer);
        }
        private void DownTriangle(double cx, double cy, double w)
        {
            double h = w * 0.8;
            Add(new LinearPath(new Point3D[]
            {
        new Point3D(cx - w / 2, cy + h / 2, 0), new Point3D(cx + w / 2, cy + h / 2, 0),
        new Point3D(cx, cy - h / 2, 0),         new Point3D(cx - w / 2, cy + h / 2, 0)
            }), LTB);
        }

        private void Tilde(double cx, double cy, double w)
        {
            var pts = new List<Point3D>();
            for (int i = 0; i <= 16; i++)
            {
                double t = i / 16.0;
                pts.Add(new Point3D(cx - w / 2 + t * w, cy + Math.Sin(t * 2 * Math.PI) * w * 0.15, 0));
            }
            Add(new LinearPath(pts.ToArray()), LTB);
        }

        /// <summary>Inserts Logo.dwg (WBLOCKed from the client sheet), scaled to fit a 'size' square centred at (cx, cy).</summary>
        //private void LogoFromDwg(DrawingDocument doc, double cx, double cy, double size)
        //{
        //    string file = Path.Combine(_configuration["StpFilePathConfig:DrawingPath"] ?? "", "Logo.dwg");
        //    if (!File.Exists(file)) return;

        //    var ra = new ReadAutodesk(file);
        //    ra.DoWork();

        //    foreach (Block b in ra.Blocks)
        //        foreach (Entity e in b.Entities) { e.LayerName = "0"; e.ColorMethod = colorMethodType.byParent; }

        //    // measure the logo
        //    var tmp = new DesignDocument();
        //    foreach (Block b in ra.Blocks) if (!tmp.Blocks.Contains(b.Name)) tmp.Blocks.Add(b);
        //    var parts = new List<Entity>();
        //    foreach (Entity e in ra.Entities)
        //    {
        //        Entity c = (Entity)e.Clone();
        //        c.LayerName = "0";
        //        parts.Add(c);
        //        tmp.Entities.Add(c);
        //    }
        //    tmp.Entities.Regen();
        //    tmp.Entities.UpdateBoundingBox();
        //    Point3D mn = tmp.Entities.BoxMin, mx = tmp.Entities.BoxMax;
        //    double ext = Math.Max(mx.X - mn.X, mx.Y - mn.Y);
        //    if (ext <= 0) return;

        //    double sc = size / ext;
        //    Transformation tr = new Translation(cx, cy, 0) *
        //                        new Scaling(Point3D.Origin, sc) *
        //                        new Translation(-(mn.X + mx.X) / 2, -(mn.Y + mx.Y) / 2, 0);

        //    foreach (Block b in ra.Blocks) if (!doc.Blocks.Any(x => x.Name == b.Name)) doc.Blocks.Add(b);
        //    foreach (Entity c in parts)
        //    {
        //        tmp.Entities.Remove(c);
        //        c.TransformBy(tr);
        //        AddC(c, LTB, Color.White);
        //    }
        //}
        #endregion

        #region Small helpers
        private void Add(Entity e, string layer)
        {
            e.LayerName = layer;
            e.ColorMethod = colorMethodType.byLayer;
            sheet.Entities.Add(e);
        }

        private void Rect(double xa, double ya, double xb, double yb, string layer)
        {
            Add(new Line(xa, ya, xb, ya), layer);
            Add(new Line(xb, ya, xb, yb), layer);
            Add(new Line(xb, yb, xa, yb), layer);
            Add(new Line(xa, yb, xa, ya), layer);
        }

        private void Txt(double x, double y, string s, double h, Text.alignmentType a, string layer)
            => Add(new Text(x, y, 0, s ?? "", h, a), layer);

        /// <summary>Text reading bottom-to-top (rotated 90°).</summary>
        private void VText(double x, double y, string s, double h, Text.alignmentType a, string layer)
        {
            var vp = new Plane(Point3D.Origin, Vector3D.AxisY, new Vector3D(-1, 0, 0));
            Add(new Text(vp, new Point3D(x, y, 0), s ?? "", h) { Alignment = a }, layer);
        }

        private static IEnumerable<string> Wrap(string text, int max)
        {
            var line = "";
            foreach (var word in text.Split(' '))
            {
                if (line.Length + word.Length + 1 > max && line.Length > 0)
                {
                    yield return line;
                    line = "   " + word;
                }
                else line = line.Length == 0 ? word : line + " " + word;
            }
            if (line.Length > 0) yield return line;
        }
        #endregion
    }
}
