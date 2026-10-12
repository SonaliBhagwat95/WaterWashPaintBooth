
#region

using devDept.Eyeshot;
using devDept.Eyeshot.Entities;
using devDept.Geometry;
using PaintBooth.Database;
using PaintBooth.Models;
using System.Drawing;
using devregion = devDept.Eyeshot.Entities.Region;

namespace PaintBooth.Business
{
    public class AddSheet
    {
        private readonly Dictionary<string, string> _formatBlockNames = new Dictionary<string, string>();

        private double margin = 25;                  // Frame margin
        private double viewSpacing = 80;             // Spacing between views


        public DrawingDocument AddSheets(string name)
        {
            DrawingDocument dr = new DrawingDocument();
            bool sheetExists = dr.Sheets.Any(s => s.Name == name);
            if (sheetExists)
            {
                // Generate a unique name for the new sheet
                name = GenerateUniqueSheetName(name);
            }
            MySheet sheet = new MySheet(new Sheet(linearUnitsType.Millimeters, 1000, 1000, name, angleProjectionType.ThirdAngle), name);
            dr = AddLayers(dr, sheet);
            return dr;
        }
        private string GenerateUniqueSheetName(string baseName)
        {
            // Generate a unique sheet name by appending a timestamp or a counter
            return $"{baseName}_{DateTime.Now.ToString("yyyyMMddHHmmss")}";
        }

        public DrawingDocument AddLayers(DrawingDocument drawings1, MySheet sheet)
        {
            drawings1.Sheets.Add(sheet);
            drawings1.ActiveSheet = sheet;
            drawings1.LineTypes.Add("Hidden", new float[] { 5F, -3F, 5F, -2F });
            drawings1.Layers[drawings1.HiddenSilhouettesLayerName].LineTypeName = "Hidden";
            drawings1.Layers[drawings1.HiddenSilhouettesLayerName].Color = System.Drawing.Color.Yellow;
            drawings1.Layers[drawings1.HiddenEdgesLayerName].LineTypeName = "Hidden";
            drawings1.Layers[drawings1.HiddenEdgesLayerName].Color = System.Drawing.Color.Yellow;
            drawings1.Layers[drawings1.HiddenWiresLayerName].LineTypeName = "Hidden";
            drawings1.Layers[drawings1.HiddenWiresLayerName].Color = System.Drawing.Color.White;
            drawings1.Layers[drawings1.WiresLayerName].LineTypeName = "Hidden";
            drawings1.Layers[drawings1.WiresLayerName].Color = System.Drawing.Color.White;
            drawings1.Layers[drawings1.EdgesLayerName].LineTypeName = "Hidden";
            drawings1.Layers[drawings1.EdgesLayerName].Color = System.Drawing.Color.White;
            drawings1.TextStyles["Default"].FontFamilyName = "Arial";
            drawings1.Layers[0].Color = System.Drawing.Color.Green;
            drawings1.Layers[0].LineTypeName = "Default";
            drawings1.Layers["Default"].Color = System.Drawing.Color.White;
            drawings1.Layers["Silhouettes"].Color = System.Drawing.Color.White;
            drawings1.Layers["HiddenSilhouettes"].Color = System.Drawing.Color.Yellow;
            drawings1.Layers[7].Color = System.Drawing.Color.Red;
            const string Dim = "Dim";
            drawings1.Layers.Add(Dim);
            drawings1.Layers[8].LineWeight = 0.2f;
            drawings1.Layers[8].Color = Color.Lime;
            return drawings1;
        }


        public MySheet AddTopViews(MySheet sheet, Point2D p, bool hiddenseg, double scale = 1.0)
        {
            var top = new VectorView(p.X, p.Y, viewType.Top, scale, "Top");
            top.HiddenSegments = hiddenseg;
            sheet.Entities.Add(top);
            return sheet;
        }

        public MySheet AddFrontViews(MySheet sheet, Point2D p, bool hiddenseg, double scale = 1.0)
        {
            var front = new VectorView(p.X, p.Y, viewType.Front, scale, "Front");
            front.HiddenSegments = hiddenseg;
            sheet.Entities.Add(front);
            return sheet;
        }

        public MySheet AddSideViews(MySheet sheet, Point2D p, bool hiddenseg, double scale = 1.0)
        {
            var side = new VectorView(p.X, p.Y, viewType.Left, scale, "Left");
            side.HiddenSegments = hiddenseg;
            sheet.Entities.Add(side);
            return sheet;
        }

        public MySheet AddIsometricViews(MySheet sheet, Point2D p, bool hiddenseg, double scale = 1.0)
        {
            var isometric = new VectorView(p.X, p.Y, viewType.Isometric, scale, "Isometric");
            isometric.HiddenSegments = hiddenseg;
            isometric.Shaded = true;
            isometric.FillRegions = true;
            sheet.Entities.Add(isometric);
            return sheet;
        }
        //    public MySheet StandardFrame(int scaleFator, MySheet sheet, Point2D p, PaintBoothModel model, List<PanelDetails> panels)
        //    {
        //        double UntrimmedWidth = 625 * scaleFator;
        //        double UntrimmedHeight = 450 * scaleFator;
        //        double trimmedWidth = 594 * scaleFator;
        //        double trimmedHeight = 420 * scaleFator;

        //        double outerX0 = 0, outerX1 = UntrimmedWidth, outerY0 = 0, outerY1 = UntrimmedHeight;

        //        double iX0 = (UntrimmedWidth - trimmedWidth) / 2;
        //        double iY0 = (UntrimmedHeight - trimmedHeight) / 2;

        //        double innerX0 = iX0, innerX1 = trimmedWidth + iX0, innerY0 = iY0, innerY1 = trimmedHeight + iY0;

        //        // ---------- Outer border ----------
        //        sheet.Entities.Add(devregion.CreatePolygon(Plane.XY, new Point2D[]
        //        {
        //            new Point2D(outerX0, outerY0),
        //            new Point2D(outerX1, outerY0),
        //            new Point2D(outerX1, outerY1),
        //            new Point2D(outerX0, outerY1),
        //        }));

        //        // ---------- Inner border ----------
        //        sheet.Entities.Add(devregion.CreatePolygon(Plane.XY, new Point2D[]
        //        {
        //            new Point2D(innerX0, innerY0),
        //            new Point2D(innerX1, innerY0),
        //            new Point2D(innerX1, innerY1),
        //            new Point2D(innerX0, innerY1),
        //        }));

        //        // ==========================================================
        //        //  TITLE BOX (bottom-right)
        //        // ==========================================================


        //        #region Title box
        //        double titleBoxWidth = 185 * scaleFator;
        //        double titleBoxHeight = 65 * scaleFator;
        //        double titleBoxX = innerX1 - titleBoxWidth;
        //        double titleBoxY = innerY0 + titleBoxHeight; // top of title box

        //        sheet.Entities.Add(devregion.CreatePolygon(Plane.XY, new Point2D[]
        //        {
        //            new Point2D(titleBoxX, titleBoxY),
        //            new Point2D(innerX1, titleBoxY),
        //            new Point2D(innerX1, innerY0),
        //            new Point2D(titleBoxX, innerY0)
        //        }));

        //        // Title box internal lines & text (converted to supported constructors)
        //        double textHeight = 5;
        //        double textH = textHeight * scaleFator;

        //        // Lines and left text
        //        sheet.Entities.Add(new Line(titleBoxX, innerY0 + (10 * scaleFator), titleBoxX + (25 * scaleFator), innerY0 + (10 * scaleFator)));
        //        sheet.Entities.Add(new Line(titleBoxX, innerY0 + (20 * scaleFator), innerX1, innerY0 + (20 * scaleFator)));
        //        sheet.Entities.Add(new Line(titleBoxX + (130 * scaleFator), titleBoxY, titleBoxX + (130 * scaleFator), innerY0));
        //        sheet.Entities.Add(new Line(titleBoxX + (25 * scaleFator), innerY0 + (20 * scaleFator), titleBoxX + (25 * scaleFator), innerY0));

        //        sheet.Entities.Add(new Text(new Point3D(titleBoxX + (10 * scaleFator), titleBoxY - (10 * scaleFator), 0),
        //            "Bullows Paint Equipment Pvt.Ltd", textH, Text.alignmentType.TopLeft));

        //        // SCALE (centered in the little box)
        //        sheet.Entities.Add(new Text(new Point3D(titleBoxX + ((25.0 / 2.0) * scaleFator), innerY0 + ((20 - 5) * scaleFator), 0),
        //            "SCALE", 4 * scaleFator, Text.alignmentType.MiddleCenter));

        //        // TITLE label
        //        sheet.Entities.Add(new Text(new Point3D(titleBoxX + ((25 + (25.0 / 2.0)) * scaleFator), innerY0 + ((20 - 5) * scaleFator), 0),
        //            "TITLE", 3 * scaleFator, Text.alignmentType.MiddleCenter));

        //        // Right side rows & labels
        //        sheet.Entities.Add(new Line(titleBoxX + (130 * scaleFator), innerY0 + ((20 + 7) * scaleFator), innerX1, innerY0 + ((20 + 7) * scaleFator)));
        //        sheet.Entities.Add(new Text(new Point3D(titleBoxX + (130 + 10) * scaleFator, innerY0 + ((20 + (7.0 / 2.0)) * scaleFator), 0),
        //            "APPROVED", 2 * scaleFator, Text.alignmentType.MiddleCenter));

        //        sheet.Entities.Add(new Line(titleBoxX + (130 * scaleFator), innerY0 + ((20 + 7 * 2) * scaleFator), innerX1, innerY0 + ((20 + 7 * 2) * scaleFator)));
        //        sheet.Entities.Add(new Text(new Point3D(titleBoxX + (130 + 10) * scaleFator, innerY0 + ((20 + 7 + (7.0 / 2.0)) * scaleFator), 0),
        //            "STANDARD", 2 * scaleFator, Text.alignmentType.MiddleCenter));

        //        sheet.Entities.Add(new Line(titleBoxX + (130 * scaleFator), innerY0 + ((20 + 7 * 3) * scaleFator), innerX1, innerY0 + ((20 + 7 * 3) * scaleFator)));
        //        sheet.Entities.Add(new Text(new Point3D(titleBoxX + (130 + 10) * scaleFator, innerY0 + ((20 + 7 * 2 + (7.0 / 2.0)) * scaleFator), 0),
        //            "CHECKED", 2 * scaleFator, Text.alignmentType.MiddleCenter));

        //        sheet.Entities.Add(new Line(titleBoxX + (130 * scaleFator), innerY0 + ((20 + 7 * 4) * scaleFator), innerX1, innerY0 + ((20 + 7 * 4) * scaleFator)));
        //        sheet.Entities.Add(new Text(new Point3D(titleBoxX + (130 + 10) * scaleFator, innerY0 + ((20 + 7 * 3 + (7.0 / 2.0)) * scaleFator), 0),
        //            "DRAWN", 2 * scaleFator, Text.alignmentType.MiddleCenter));

        //        sheet.Entities.Add(new Line(titleBoxX + (130 * scaleFator), innerY0 + ((20 + 7 * 5) * scaleFator), innerX1, innerY0 + ((20 + 7 * 5) * scaleFator)));
        //        sheet.Entities.Add(new Text(new Point3D(titleBoxX + (130 + 10) * scaleFator, innerY0 + ((20 + 7 * 4 + (7.0 / 2.0)) * scaleFator), 0),
        //            "DESIGNED", 2 * scaleFator, Text.alignmentType.MiddleCenter));

        //        // NAME / DATE labels on title box
        //        sheet.Entities.Add(new Text(new Point3D(titleBoxX + (130 + 20 + 12.5) * scaleFator, titleBoxY - (5 * scaleFator), 0),
        //            "NAME", 2 * scaleFator, Text.alignmentType.MiddleCenter));
        //        sheet.Entities.Add(new Text(new Point3D(titleBoxX + (130 + 20 + 25 + 5) * scaleFator, titleBoxY - (5 * scaleFator), 0),
        //            "DATE", 2 * scaleFator, Text.alignmentType.MiddleCenter));

        //        // vertical divider lines in title box
        //        sheet.Entities.Add(new Line(innerX1 - (10 * scaleFator), innerY0 + (20 * scaleFator), innerX1 - (10 * scaleFator), titleBoxY));
        //        sheet.Entities.Add(new Line(innerX1 - ((10 + 25) * scaleFator), innerY0 + (20 * scaleFator), innerX1 - ((10 + 25) * scaleFator), titleBoxY));

        //        // DRAWING NO label
        //        sheet.Entities.Add(new Text(new Point3D(titleBoxX + (130 + 5) * scaleFator, innerY0 + (20 - 5) * scaleFator, 0),
        //            "DRAWING NO.", 2 * scaleFator, Text.alignmentType.MiddleLeft)); 
        //        #endregion


        //        // ==========================================================
        //        // HEIGHT CALCULATIONS FOR STACK ORDER (keep stacking so it matches PDF)
        //        // ==========================================================
        //        double bomRowHeight = 16 * scaleFator;      // slightly reduced so content fits
        //        double bomHeaderHeight = 20 * scaleFator;
        //        double bomTotalHeight = bomHeaderHeight + (bomRowHeight * 6);

        //        double notesHeight = 160 * scaleFator;     // reduced to fit inside inner frame
        //        double tolHeight = 30 * scaleFator;

        //        // Stack from bottom (title box) upward:
        //        double titleTopY = titleBoxY;                 // top of title box
        //        double tolBottomY = titleTopY;                // tolerance bottom touches title top
        //        double tolTopY = tolBottomY + tolHeight;

        //        double notesBottomY = tolTopY;                // notes bottom touches tolerance top
        //        double notesTopY = notesBottomY + notesHeight;

        //        double bomBottomY = notesTopY;                // BOM bottom touches notes top
        //        double bomTopY = bomBottomY + bomTotalHeight;


        //        // ==========================================================
        //        // BOM BLOCK (F1 – F6) — positioned based on calculated bomTopY/bomBottomY
        //        // ==========================================================
        //        //--------------------------------------------------------------
        //        //  BOM (Bill of Materials) BLOCK
        //        //--------------------------------------------------------------

        //        // BOM POSITION BASED ON PREVIOUS CALCULATIONS
        //        double bomX0 = titleBoxX;
        //        double bomX1 = innerX1;
        //        double bomY0 = bomTopY;        // TOP of BOM
        //        double bomY1 = bomBottomY;     // BOTTOM of BOM

        //        // HEADER HEIGHT & ROW HEIGHT
        //         //bomHeaderHeight = 20 * scaleFator;
        //        // bomRowHeight = 18 * scaleFator;

        //        //--------------------------------------------------------------
        //        //  HEADER RECTANGLE
        //        //--------------------------------------------------------------
        //        sheet.Entities.Add(devregion.CreatePolygon(Plane.XY, new Point2D[]
        //        {
        //            new Point2D(bomX0, bomY0),
        //            new Point2D(bomX1, bomY0),
        //            new Point2D(bomX1, bomY0 - bomHeaderHeight),
        //            new Point2D(bomX0, bomY0 - bomHeaderHeight)
        //        }));

        //        //--------------------------------------------------------------
        //        //  Sr.No COLUMN WIDTH + VERTICAL LINE
        //        //--------------------------------------------------------------
        //        double srNoWidth = 20 * scaleFator;

        //        // Vertical divider line in header
        //        sheet.Entities.Add(new Line(bomX0 + srNoWidth, bomY0, bomX0 + srNoWidth, bomY0 - bomHeaderHeight));

        //        //--------------------------------------------------------------
        //        //  HEADER TEXT
        //        //--------------------------------------------------------------
        //        sheet.Entities.Add(new Text(
        //            new Point3D(bomX0 + srNoWidth / 2, bomY0 - bomHeaderHeight / 2, 0),
        //            "Sr. No.", 4 * scaleFator, Text.alignmentType.MiddleCenter));

        //        sheet.Entities.Add(new Text(
        //            new Point3D(bomX0 + srNoWidth + (bomX1 - (bomX0 + srNoWidth)) / 2,
        //                        bomY0 - bomHeaderHeight / 2, 0),
        //            "DESCRIPTION OF PAINT BOOTH",
        //            4 * scaleFator,
        //            Text.alignmentType.MiddleCenter));

        //        //--------------------------------------------------------------
        //        //  BOM ROWS (F1 – F6)
        //        //--------------------------------------------------------------
        //        string[] rowText = { "F1", "F2", "F3", "F4", "F5", "F6" };

        //        string[] desc =
        //        {
        //            $"DIMENSION : {model.W} MM W X {model.D} MM D X {model.H} MM H",
        //            $"CENTRIFUGAL EXHAUST BLOWER CAPACITY: {model.CapacityofBlowerinH} M3/hr  Qty : 1 NO.",
        //            $"MOTOR FOR EXHAUST BLOWER CAPACITY: {model.RatedOutputHP} HP {model.MotorTypes} Qty : 1 NO.",
        //            $"LED LAMP FLAME PROOF  Qty : {model.Lights} No.",
        //            "DUCTING  QTY : 1 SET.",
        //            "BOOTH MOC : M.S. 1.2MM THICK"
        //        };

        //        double rowY = bomY0 - bomHeaderHeight;

        //        // NEW CORRECT TEXT HEIGHT
        //        double bomTextH = 3 * scaleFator;

        //        for (int i = 0; i < 6; i++)
        //        {
        //            // Row rectangle
        //            sheet.Entities.Add(devregion.CreatePolygon(Plane.XY, new Point2D[]
        //            {
        //                new Point2D(bomX0, rowY),
        //                new Point2D(bomX1, rowY),
        //                new Point2D(bomX1, rowY - bomRowHeight),
        //                new Point2D(bomX0, rowY - bomRowHeight)
        //            }));

        //            // Sr. No.
        //            sheet.Entities.Add(new Text(
        //                new Point3D(bomX0 + srNoWidth / 2, rowY - bomRowHeight / 2, 0),
        //                rowText[i], 4 * scaleFator, Text.alignmentType.MiddleCenter));

        //            // WRAP TEXT
        //            double descLeftX = bomX0 + srNoWidth + 3 * scaleFator;
        //            double maxWidth = (bomX1 - descLeftX) - (5 * scaleFator);

        //            List<string> wrapped = WrapText(desc[i], maxWidth, bomTextH);

        //            double textY = rowY - (4 * scaleFator);

        //            foreach (string line in wrapped)
        //            {
        //                sheet.Entities.Add(new Text(
        //                    new Point3D(descLeftX, textY, 0),
        //                    line, bomTextH, Text.alignmentType.TopLeft));

        //                textY -= (bomTextH + 2 * scaleFator);
        //            }

        //            rowY -= bomRowHeight;
        //        }
        //        //--------------------------------------------------------------
        //        //  FULL HEIGHT Sr.No VERTICAL LINE
        //        //--------------------------------------------------------------
        //        sheet.Entities.Add(new Line(bomX0 + srNoWidth, bomY0, bomX0 + srNoWidth, bomY1));

        //        #region Notes
        //        double notesX0 = titleBoxX;
        //        double notesX1 = innerX1;

        //        sheet.Entities.Add(devregion.CreatePolygon(Plane.XY, new Point2D[]
        //        {
        //            new Point2D(notesX0, notesTopY),
        //            new Point2D(notesX1, notesTopY),
        //            new Point2D(notesX1, notesBottomY),
        //            new Point2D(notesX0, notesBottomY)
        //        }));

        //        double ntH = 3 * scaleFator;
        //        double curY = notesTopY - (8 * scaleFator);

        //        sheet.Entities.Add(new Text(new Point3D(notesX0 + 5 * scaleFator, curY, 0),
        //            "NOTES", ntH + 1, Text.alignmentType.TopLeft));
        //        curY -= 12 * scaleFator;

        //        string[] notesLines =
        //        {
        //            "1) DURING EQUIPMENT INSTALLATION, WALL/ROOF",
        //            "   CUT OUT & SEALING FOR EXHAUST DUCT ROUTING",
        //            "   IS IN CUSTOMER SCOPE.",
        //            "",
        //            "2) IF REQUIRED, BLOWER MOUNTING STRUCTURE WILL BE IN CUSTOMER SCOPE.",
        //            "",
        //            "3) MAIN UTILITY IS IN CUSTOMER SCOPE.",
        //            "   COMPRESSED AIR OIL & MOISTURE FREE AT INLET",
        //            "   PRESSURE REGULATOR @ 6 BAR.",
        //            "   ELECTRICAL POWER - @ 11.5 KW (APPROX.)",
        //            "",
        //            "4) GROUND LEVEL SHOULD BE CONCRETED FLAT &",
        //            "   LEVELED, NOT TO HAVE VARIATION MORE THAN +/- 3MM."
        //        };

        //        foreach (var line in notesLines)
        //        {
        //            sheet.Entities.Add(new Text(new Point3D(notesX0 + 5 * scaleFator, curY, 0), line, ntH, Text.alignmentType.TopLeft));
        //            curY -= 6 * scaleFator;
        //        } 
        //        #endregion

        //        // UTILITIES

        //        sheet.Entities.Add(new Text(new Point3D(notesX0 + 5 * scaleFator, curY, 0),
        //            "UTILITY IS IN CUSTOMER SCOPE", ntH + 1, Text.alignmentType.TopLeft));
        //        curY -= 10 * scaleFator;

        //        sheet.Entities.Add(new Text(new Point3D(notesX0 + 10 * scaleFator, curY, 0), "• AIR", ntH, Text.alignmentType.TopLeft));
        //        curY -= 8 * scaleFator;
        //        sheet.Entities.Add(new Text(new Point3D(notesX0 + 10 * scaleFator, curY, 0), "• ELECTRIC", ntH, Text.alignmentType.TopLeft));


        //        // ==========================================================
        //        // TOLERANCE BOX (just above title box)
        //        // ==========================================================
        //        sheet.Entities.Add(devregion.CreatePolygon(Plane.XY, new Point2D[]
        //        {
        //            new Point2D(notesX0, tolTopY),
        //            new Point2D(notesX1, tolTopY),
        //            new Point2D(notesX1, tolBottomY),
        //            new Point2D(notesX0, tolBottomY)
        //        }));

        //        sheet.Entities.Add(new Text(new Point3D(notesX0 + 5 * scaleFator, tolBottomY + tolHeight / 2, 0),
        //            "GENERAL TOLERANCE ± 3 MM", ntH, Text.alignmentType.MiddleLeft));
        //        double viewAreaRight = titleBoxX - (10 * scaleFator);

        //        // ==========================================================
        //        // VIEWS and return
        //        // ==========================================================
        //        //sheet = Views(sheet, innerX0, innerX1, titleBoxX, titleBoxY, innerY1, model, scaleFator);
        //        sheet = Views(sheet,
        //innerX0,        // x0 — left edge of inner frame
        //innerX1,        // innerX1 — kept for reference
        //viewAreaRight,  // x1 — right boundary of view area
        //innerY0,        // y0 — BOTTOM of inner frame  ← was titleBoxY (wrong)
        //innerY1,        // y1 — TOP of inner frame
        //model,
        //scaleFator);
        //        return sheet;

        //    }

        public MySheet StandardFrame(int scaleFator, MySheet sheet, Point2D p, PaintBoothModel model, List<PanelDetails> panels)
        {
            double s = scaleFator;

            double UntrimmedWidth = 625 * s;
            double UntrimmedHeight = 450 * s;
            double trimmedWidth = 594 * s;
            double trimmedHeight = 420 * s;

            double outerX0 = 0, outerX1 = UntrimmedWidth, outerY0 = 0, outerY1 = UntrimmedHeight;

            double iX0 = (UntrimmedWidth - trimmedWidth) / 2;
            double iY0 = (UntrimmedHeight - trimmedHeight) / 2;

            double innerX0 = iX0, innerX1 = trimmedWidth + iX0, innerY0 = iY0, innerY1 = trimmedHeight + iY0;

            // ---------- Local helpers ----------
            var TL = Text.alignmentType.TopLeft;
            var ML = Text.alignmentType.MiddleLeft;
            var MR = Text.alignmentType.MiddleRight;
            var MC = Text.alignmentType.MiddleCenter;

            void Rect(double x0, double y0, double x1, double y1) =>
                sheet.Entities.Add(devregion.CreatePolygon(Plane.XY, new[]
                {
                new Point2D(x0, y0), new Point2D(x1, y0),
                new Point2D(x1, y1), new Point2D(x0, y1)
                }));
            void Ln(double x0, double y0, double x1, double y1) =>
                sheet.Entities.Add(new Line(x0, y0, x1, y1));
            void Txt(double x, double y, string t, double h, Text.alignmentType a) =>
                sheet.Entities.Add(new Text(new Point3D(x, y, 0), t ?? "", h, a));

            // ---------- Outer & inner border ----------
            Rect(outerX0, outerY1, outerX1, outerY0);
            Rect(innerX0, innerY1, innerX1, innerY0);

            // ==========================================================
            //  TITLE BLOCK DATA  (TODO: map to your model / enquiry fields)
            // ==========================================================
            string drawnBy = "", checkedBy = "", approvedBy = "";
            string drgDate = DateTime.Now.ToString("dd.MM.yyyy");
            string client = "";                               // e.g. enquiry customer name
            string product = "BOOTH";
            string modelCode = "S.D.";                        // Side Draft
            string part = "DRY BACK SPRAY PAINTING BOOTH";    // from booth type / sub-type
            string drgNo = "", rev = "R0", material = "M.S.", scaleText = "NTS";
            string elecLoadKw = "5";                          // electrical load for note 7

            // ==========================================================
            //  RIGHT COLUMN
            // ==========================================================
            double colW = 160 * s;
            double colX0 = innerX1 - colW;
            double colX1 = innerX1;

            #region Scope of Supply (top, grows downward)
            double snW = 22 * s;
            double hdrH = 16 * s;
            double scTxtH = 3.5 * s;
            double scGap = scTxtH * 1.6;
            double rowPad = 6 * s;

            double y = innerY1;

            Rect(colX0, y, colX1, y - hdrH);
            Txt(colX0 + snW / 2, y - hdrH / 2, "S.N.", 4.5 * s, MC);
            Txt(colX0 + snW + (colW - snW) / 2, y - hdrH / 2, "SCOPE OF SUPPLY", 4.5 * s, MC);
            y -= hdrH;

            var scope = new List<(string sn, string[] lines)>
        {
            ("(1)", new[] {
                "EXHAUST BLOWER",
                $"CAPACITY : {model.CapacityofBlowerinH:N0} M3/HR",
                $"MOTOR CAPACITY : {model.RatedOutputHP} H.P.",
                $"{model.MotorTypes}".ToUpper(),
                "QTY : 1 NO." }),
            ("(2)", new[] {
                "EXTRACTION CHAMBER",
                "METAL BAFFLE + P.A. FILTER",
                "QTY : 1 SET." }),
            ("(3)", new[] {
                "LED LIGHT FIXTURE FLAME PROOF",
                $"QTY : {model.Lights} NOS." }),
            ("(4)", new[] { "INCLINED TUBE MANOMETER", "QTY :- 1 NO." }),
            ("(5)", new[] { "EXHAUST DUCT SET UP", "QTY :- 1 SET" }),
            ("(6)", new[] { "STAR DELTA STARTER + MCB", "QTY :- 1 NO." }),
        };

            double descX = colX0 + snW + 4 * s;
            double descMaxW = colX1 - descX - 3 * s;

            foreach (var item in scope)
            {
                var wrapped = item.lines
                    .Where(l => !string.IsNullOrWhiteSpace(l))
                    //.SelectMany(l => WrapText(l, descMaxW, scTxtH))
                    .SelectMany(l => WrapTextMeasured(l, descMaxW, scTxtH))
                    .ToList();

                double rowH = rowPad * 2 + wrapped.Count * scGap;

                Rect(colX0, y, colX1, y - rowH);
                Txt(colX0 + snW / 2, y - rowH / 2, item.sn, 3.5 * s, MC);

                double ty = y - rowPad;
                foreach (var l in wrapped)
                {
                    Txt(descX, ty, l, scTxtH, TL);
                    ty -= scGap;
                }
                y -= rowH;
            }

            Ln(colX0 + snW, innerY1, colX0 + snW, y);   // full-height S.N. divider
            double scopeBottomY = y;
            #endregion

            #region Notes (directly below scope)
            double nTxtH = 2.6 * s;
            double nGap = nTxtH * 1.55;
            double nPad = 5 * s;
            double nx0 = colX0 + 4 * s;
            double prefixW = 7 * s;
            double levelIndent = 6 * s;

            var noteItems = new List<(string prefix, string text, int level)>
        {
            ("1)", "PAINT SCHEME -", 0),
            ("A)", "PAINT BOOTH INSIDE SHADE - BRILLIANT WHITE", 1),
            ("B)", "PAINT BOOTH OUTSIDE SHADE - JASMINE WISP", 1),
            ("C)", "EXTERNAL FACE OF STRUCTURAL MEMBER SHADE - POST OFFICE RED", 1),
            ("D)", "IF ANY SPECIAL SHADE IS REQUIRED PLEASE SPECIFY ON G.A. AT THE TIME OF APPROVAL.", 1),
            ("2)", "WALL CUT OUT & SEALING FOR EXHAUST DUCT ROUTING IS IN CUSTOMER SCOPE AT THE TIME OF EQUIPMENT INSTALLATION.", 0),
            ("3)", "DUCT SUPPORTING FROM BUILDING WALL / PURLIN OF PRE FAB. BUILDING AT THE TIME OF INSTALLATION.", 0),
            ("4)", "PLATFORM, LADDER FOR MAINTENANCE OF LIGHT FIXTURES IN CUSTOMER SCOPE.", 0),
            ("5)", "MAIN UTILITY IS IN CUSTOMER SCOPE.", 0),
            ("6)", "MATERIAL HANDLING (O.H. CONVEYOR) IS IN CUSTOMER SCOPE & COMPONENT ENTRY / EXIT C/O TO BE CONFIRMED BY CUSTOMER.", 0),
            ("7)", $"ELECTRICAL LOAD :- {elecLoadKw} KW APPROX. ALL ELECTRICAL CABLES WILL BE COPPER FLEXIBLE CABLE.", 0),
        };

            double ny = scopeBottomY - nPad;
            double notesHdrH = nTxtH * 1.2;
            Txt(nx0, ny, "NOTES:", notesHdrH, TL);
            Ln(nx0, ny - notesHdrH - 0.8 * s, nx0 + 13 * s, ny - notesHdrH - 0.8 * s);   // underline
            ny -= notesHdrH + 3 * s;

            bool first = true;
            foreach (var n in noteItems)
            {
                if (n.level == 0 && !first) ny -= nTxtH * 0.8;   // gap between main notes
                first = false;

                double px = nx0 + n.level * levelIndent;
                double tx = px + prefixW;
                // var lines = WrapText(n.text, colX1 - tx - 3 * s, nTxtH);
                var lines = WrapTextMeasured(n.text, colX1 - tx - 3 * s, nTxtH);
                Txt(px, ny, n.prefix, nTxtH, TL);
                foreach (var l in lines)
                {
                    Txt(tx, ny, l, nTxtH, TL);    // hanging indent for wrapped lines
                    ny -= nGap;
                }
            }

            double notesBottomY = ny - nPad + nGap * 0.4;
            Rect(colX0, scopeBottomY, colX1, notesBottomY);
            #endregion

            #region Title block (bottom-right)
            double tbH = 40 * s;
            double stripH = 6 * s;
            double tbY0 = innerY0;
            double tbY1 = innerY0 + tbH;
            double mY0 = tbY0 + stripH;          // top of bottom strip

            double xA = colX0;                   // tolerance
            double xB = xA + 38 * s;             // drawn / checked / approved / client
            double xC = xB + 50 * s;             // product / model / part
            double xD = xC + 40 * s;             // company / drg no / rev
            double xE = colX1;

            double lh = 1.8 * s;                 // label text
            double vh = 2.6 * s;                 // value text

            Rect(xA, tbY1, xE, tbY0);
            Ln(xA, mY0, xE, mY0);
            Ln(xB, tbY1, xB, mY0);               // MAT'L strip spans A+B
            Ln(xC, tbY1, xC, tbY0);
            Ln(xD, tbY1, xD, tbY0);

            // ---- A : Tolerance ----
            double aHdrH = 6 * s, aFootH = 6 * s;
            double aSplit = xA + 22 * s;
            double tolTop = tbY1 - aHdrH;
            double tolBot = mY0 + aFootH;

            Ln(xA, tolTop, xB, tolTop);
            Ln(xA, tolBot, xB, tolBot);
            Ln(aSplit, tbY1, aSplit, tolBot);

            Txt((xA + aSplit) / 2, tbY1 - aHdrH * 0.33, "OPEN TOLERANCE", 1.5 * s, MC);
            Txt((xA + aSplit) / 2, tbY1 - aHdrH * 0.72, "MEDIUM FINISH", 1.5 * s, MC);
            Txt((aSplit + xB) / 2, tbY1 - aHdrH * 0.33, "BREAK", 1.5 * s, MC);
            Txt((aSplit + xB) / 2, tbY1 - aHdrH * 0.72, "CORNERS", 1.5 * s, MC);

            string[,] tol =
            {
            { "-",    "10",   "±0.1" },
            { "10",   "30",   "±0.2" },
            { "30",   "100",  "±0.3" },
            { "100",  "300",  "±0.5" },
            { "300",  "1000", "±0.8" },
            { "1000", "3000", "±1.2" }
        };
            double tolRowH = (tolTop - tolBot) / 6;
            for (int i = 0; i < 6; i++)
            {
                double cy = tolTop - tolRowH * (i + 0.5);
                Txt(xA + 6 * s, cy, tol[i, 0], 1.5 * s, MR);
                Txt(xA + 14 * s, cy, tol[i, 1], 1.5 * s, MR);
                Txt(xA + 15.5 * s, cy, tol[i, 2], 1.5 * s, ML);
            }

            string[] corners = { "~ = DRESSED", "▽ = ROUGH", "▽▽ = MEDIUM", "▽▽▽ = FINE" };
            double cRowH = (tolTop - tolBot) / corners.Length;
            for (int i = 0; i < corners.Length; i++)
                Txt(aSplit + 1 * s, tolTop - cRowH * (i + 0.5), corners[i], 1.4 * s, ML);

            Txt((xA + xB) / 2, tolBot - aFootH * 0.32, "ALL DIMENSIONS ARE IN MILLIMETERS,", 1.2 * s, MC);
            Txt((xA + xB) / 2, tolBot - aFootH * 0.70, "UNLESS OTHERWISE SPECIFIED.", 1.2 * s, MC);

            // ---- B : Signatures + Client ----
            string[] sigLabels = { "DRAWN BY", "CHECKED BY", "APPROVED BY" };
            string[] sigNames = { drawnBy, checkedBy, approvedBy };
            double sigRowH = 6 * s;
            double bName = xB + 22 * s, bDate = xB + 36 * s;

            Ln(bName, tbY1, bName, tbY1 - 3 * sigRowH);
            Ln(bDate, tbY1, bDate, tbY1 - 3 * sigRowH);

            for (int i = 0; i < 3; i++)
            {
                double rTop = tbY1 - i * sigRowH;
                Ln(xB, rTop - sigRowH, xC, rTop - sigRowH);
                double cy = rTop - sigRowH / 2;
                Txt(xB + 1.5 * s, cy, sigLabels[i], lh, ML);
                Txt((bName + bDate) / 2, cy, sigNames[i], lh, MC);
                Txt((bDate + xC) / 2, cy, drgDate, 1.5 * s, MC);
            }

            double clTop = tbY1 - 3 * sigRowH;
            Txt(xB + 1.5 * s, clTop - 1.5 * s, "CLIENT :", lh, TL);
            Txt((xB + xC) / 2, (clTop + mY0) / 2 - 1 * s, client, vh, MC);

            // ---- C : Product / Model / Part ----
            double cProdB = tbY1 - 10 * s;
            double cModelB = tbY1 - 18 * s;
            Ln(xC, cProdB, xD, cProdB);
            Ln(xC, cModelB, xD, cModelB);

            Txt(xC + 1.5 * s, tbY1 - 1.5 * s, "PRODUCT :", lh, TL);
            Txt((xC + xD) / 2, cProdB + 3 * s, product, vh, MC);

            Txt(xC + 1.5 * s, cProdB - 1.5 * s, "MODEL :", lh, TL);
            Txt((xC + xD) / 2, cModelB + 2.5 * s, modelCode, vh, MC);

            Txt(xC + 1.5 * s, cModelB - 1.5 * s, "PART :", lh, TL);
            double py = cModelB - 7 * s;
            // foreach (var l in WrapText(part, (xD - xC) - 4 * s, vh))
            foreach (var l in WrapTextMeasured(part, (xD - xC) - 4 * s, vh))
            {
                Txt((xC + xD) / 2, py, l, vh, MC);
                py -= vh * 1.5;
            }

            // ---- D : Company / DRG NO / REV ----
            double dLogoB = tbY1 - 18 * s;
            double dRev = xE - 8 * s;
            double dcx = (xD + xE) / 2;
            Ln(xD, dLogoB, xE, dLogoB);
            Ln(dRev, dLogoB, dRev, mY0);

            Txt(dcx, tbY1 - 5 * s, "BULLOWS PAINT", 2.2 * s, MC);
            Txt(dcx, tbY1 - 9 * s, "EQPT. PVT. LTD.", 2.2 * s, MC);
            Txt(dcx, tbY1 - 14 * s, "(THANE-400604)", 1.8 * s, MC);

            Txt(xD + 1.5 * s, dLogoB - 1.5 * s, "DRG. NO.", lh, TL);
            Txt((xD + dRev) / 2, (dLogoB + mY0) / 2 - 1 * s, drgNo, 2 * s, MC);
            Txt(dRev + 1 * s, dLogoB - 1.5 * s, "REV.", lh, TL);
            Txt((dRev + xE) / 2, (dLogoB + mY0) / 2 - 1 * s, rev, vh, MC);

            // ---- Bottom strip ----
            double stripCy = tbY0 + stripH / 2;
            double cStripSplit = xC + 20 * s;
            Ln(cStripSplit, tbY0, cStripSplit, mY0);

            Txt(xA + 1.5 * s, stripCy, $"MAT'L : {material}", lh, ML);
            Txt(xC + 1.5 * s, stripCy, "QTY : AS SHOWN", 1.5 * s, ML);
            Txt(cStripSplit + 1.5 * s, stripCy, $"SCALE : {scaleText}", 1.5 * s, ML);
            Txt(dcx, stripCy, "SHEET 1 OF 1", lh, MC);
            #endregion

            // ==========================================================
            // VIEWS and return
            // ==========================================================
            double viewAreaRight = colX0 - (10 * s);

            sheet = Views(sheet,
                innerX0,        // x0 — left edge of inner frame
                innerX1,        // innerX1 — kept for reference
                viewAreaRight,  // x1 — right boundary of view area
                innerY0,        // y0 — bottom of inner frame
                innerY1,        // y1 — top of inner frame
                model,
                scaleFator);

            return sheet;
        }


        private List<string> WrapText(string text, double maxWidth, double fontHeight)
        {
            List<string> lines = new List<string>();
            string[] words = text.Split(' ');

            string current = "";
            foreach (string w in words)
            {
                string test = (current.Length == 0 ? w : current + " " + w);

                // Estimated character width (Eyeshot uses about 0.6 * height)
                double estimatedWidth = test.Length * (fontHeight * 0.6);

                if (estimatedWidth > maxWidth)
                {
                    lines.Add(current);
                    current = w;
                }
                else
                {
                    current = test;
                }
            }

            if (current.Length > 0)
                lines.Add(current);

            return lines;
        }
        /// <summary>
        /// Word-wraps text using per-character width estimates for an Arial-style
        /// font (widths are relative to text height, tuned for UPPERCASE text).
        /// </summary>
        private static List<string> WrapTextMeasured(string text, double maxWidth, double textHeight)
        {
            var result = new List<string>();
            if (string.IsNullOrWhiteSpace(text)) return result;

            double CharW(char c)
            {
                if (c == ' ') return 0.40;
                if (".,:;!|'()[]-/".IndexOf(c) >= 0) return 0.45;
                if (c == 'I') return 0.45;
                if (c == 'M' || c == 'W') return 1.25;
                if (char.IsDigit(c)) return 0.80;
                if (char.IsUpper(c)) return 0.95;
                return 0.75;                     // lowercase / symbols
            }

            double Measure(string s)
            {
                double w = 0;
                foreach (char c in s) w += CharW(c);
                return w * textHeight * 1.05;    // 5% safety margin
            }

            string line = "";
            foreach (var word in text.Split(' ', StringSplitOptions.RemoveEmptyEntries))
            {
                string candidate = line.Length == 0 ? word : line + " " + word;

                if (Measure(candidate) <= maxWidth || line.Length == 0)
                {
                    line = candidate;            // fits (or a single long word)
                }
                else
                {
                    result.Add(line);
                    line = word;
                }
            }
            if (line.Length > 0) result.Add(line);

            return result;
        }
        public MySheet ViewsOld(MySheet drawingdoc, double x0,double innerX1, double x1, double y0, double y1, PaintBoothModel model,int scaleFator)
        {
            DesignDocument drawing = new DesignDocument();
            const string Dim = "Dimension";
            Plane verticalPlane = Plane.XY;
            verticalPlane.Rotate(Math.PI / 2, Vector3D.AxisZ);
            drawing.Layers.Add(new Layer(Dim, Color.CornflowerBlue));
            double YValue = y1 - (model.H)/2 - (model.CChannelHeight * 2) - (double)(model.standardbend2 * 2);
            Line ly = new Line(Plane.XY, Point2D.Origin, new Point2D(x1 - (model.D / 2 + model.D3), YValue));
            //drawingdoc.Entities.Add(ly);

            MySheet mysheet = AddTopViews(drawingdoc, new Point2D(x1 - ((model.D / 2 + model.D3)), (y0 + model.W / 2) + (double)(2 * model.standardbend2)), true);
            mysheet = AddFrontViews(drawingdoc, new Point2D(x1 - ((model.D / 2 + model.D3)), YValue), true);
            mysheet = AddSideViews(drawingdoc, new Point2D(x0 + (model.W / 2) + (model.W / 3), YValue), true);
            mysheet = AddIsometricViews(drawingdoc, new Point2D(x0 + (model.W), (y0 + model.W / 2) + (double)(2 * model.standardbend2)), true);

            #region Top view Dimensions
            double topViewX = (x1 - ((model.D / 2 + model.D3)) - ((model.D / 2) + (model.D3 / 2)));
           

            Text topViewText = new Text(
                new Point3D(x1 - (model.D / 2 + model.D3) - 100, (y0 - 300 - (double)(2 * model.standardbend2)), 0),
                "Top View", 40// Font size for the text
            )
            {
                LayerName = Dim,  // Set the layer for the text               
            };
            drawingdoc.Entities.Add(topViewText);

            //total width of Paintbooth
            LinearDim totalWidth = new LinearDim(Plane.XY,
                new Point2D(topViewX, (y0 + model.W / 2) + (double)(2 * model.standardbend2)),
                new Point2D(topViewX +model.D+model.D3, (y0 + model.W / 2) + (double)(2 * model.standardbend2)),
                new Point2D(x1 - (model.D / 2 + model.D3),y0-200), 40)
            {
                LayerName = Dim
            };
          
            drawingdoc.Entities.Add(totalWidth);

            //total Depth of Paintbooth
            LinearDim DepthOfPaintbooth = new LinearDim(Plane.XY,
                new Point2D(topViewX, (y0 + model.W / 2) + (double)(2 * model.standardbend2)),
                new Point2D(topViewX + model.D, (y0 + model.W / 2) + (double)(2 * model.standardbend2)),
                new Point2D(((topViewX) +(topViewX + model.D))/2, y0-50), 40)
            { LayerName = Dim,
              TextOverride="DEPTH = <>"
            };

            drawingdoc.Entities.Add(DepthOfPaintbooth);

            // D3 width of Paintbooth
            LinearDim D3Width = new LinearDim(Plane.XY,
                new Point2D(topViewX + model.D, (y0 + model.W / 2) + (double)(2 * model.standardbend2)),
                new Point2D(topViewX + model.D + model.D3, (y0 + model.W / 2) + (double)(2 * model.standardbend2)),
                new Point2D(((topViewX + model.D) +(topViewX + model.D + model.D3))/2, y0-50), 40)
            { LayerName = Dim,
            TextOverride="D3 = <>"
            };
           
            drawingdoc.Entities.Add(D3Width);



            LinearDim topviewYDim = new LinearDim(verticalPlane,
                new Point2D((y0 + (double)(2 * model.standardbend2)), -x1 + (model.D3 / 2)),
               new Point2D((y0 + model.W + (double)(2 * model.standardbend2)), -x1 + (model.D3 / 2)),
               new Point2D(y0 + model.W / 2, -x1 + (model.D3 / 2) - 175), 40)
            { LayerName = Dim,
              TextOverride="WIDTH = <>"
            };
            // Add the dimension entity to the drawing document
           
            drawingdoc.Entities.Add(topviewYDim);


            Line l1 = new Line(Plane.XY, Point2D.Origin, new Point2D(x1 - (model.D / 2 + model.D3) - 100, (y0)));
            //drawingdoc.Entities.Add(l1);


            #endregion

            #region Front view Dimensions
            
            Line l = new Line(Plane.XY, Point2D.Origin, new Point2D(x1,YValue));
            //drawingdoc.Entities.Add(l);
            double FrontViewY = YValue - model.H / 2 ;

            // Add "FrontView" Text above the front view dimensions
            Text frontViewText = new Text(
                new Point3D(x1 - (model.D / 2 + model.D3) - 100, FrontViewY - 300, 0),  // Adjusted Y to move it higher
                "Front View",  // The text content
                40  // Font size for the text
            )
            {
                LayerName = Dim,  // Set the layer for the text              
            };
            drawingdoc.Entities.Add(frontViewText);

            //total Depth of Paintbooth
            LinearDim DepthOfPaintboothFrontView = new LinearDim(Plane.XY,
                new Point2D(topViewX, FrontViewY),
                new Point2D(topViewX + model.D, FrontViewY),
                new Point2D(((topViewX) + (topViewX + model.D)) / 2, FrontViewY - 150), 40)
            {
                LayerName = Dim,
                TextOverride = "DEPTH = <>"
            };
            drawingdoc.Entities.Add(DepthOfPaintboothFrontView);

            // D3 width of Paintbooth
            LinearDim D3WidthFrontView = new LinearDim(Plane.XY,
                new Point2D(topViewX + model.D, FrontViewY),
                new Point2D(topViewX + model.D + model.D3, FrontViewY),
                new Point2D(((topViewX + model.D) + (topViewX + model.D + model.D3)) / 2, FrontViewY - 150), 40)
            {
                LayerName = Dim,
                TextOverride = "D3 = <>"
            };
            drawingdoc.Entities.Add(D3WidthFrontView);

            LinearDim FrontviewXDim = new LinearDim(Plane.XY,
                 new Point2D((x1 - ((model.D / 2 + model.D3)) - ((model.D / 2) + (model.D3 / 2))), FrontViewY),
                 new Point2D((x1 - ((model.D / 2 + model.D3)) + (model.D / 2) + model.D3 / 2), FrontViewY),
                new Point2D(x1 - ((model.D / 2 + model.D3)), FrontViewY-230), 40)
            {
               LayerName = Dim
            };
               
            drawingdoc.Entities.Add(FrontviewXDim);

            LinearDim FrontviewYDim = new LinearDim(verticalPlane,
                new Point2D(FrontViewY, -x1 + (model.D3 / 2)),
              new Point2D(FrontViewY+model.H, -x1 + (model.D3 / 2)),
              new Point2D(((FrontViewY) +(FrontViewY+ model.H))/2, -x1 + (model.D3 / 2)-150),40)
            { LayerName = Dim,
              TextOverride="HEIGHT = <>"
            };
            drawingdoc.Entities.Add(FrontviewYDim);

            #endregion

            #region Side view Dimensions

            // Add "SideView" Text above the side view dimensions
            Text sideViewText = new Text(
                new Point3D(x0 + ((model.W / 2) + model.W / 3) - 100, FrontViewY - 300, 0),  // Adjusted Y to move the text above
                "Side View",  // The text content
                40  // Font size for the text
            )
            {
                LayerName = Dim,  // Set the layer for the text               
            };
            drawingdoc.Entities.Add(sideViewText);



            LinearDim SideviewXDimWidth = new LinearDim(Plane.XY,
                 new Point2D((x0 + model.W / 3), FrontViewY),
                 new Point2D((x0 + model.W + model.W / 3), FrontViewY),

                new Point2D(x0 + ((model.W / 2) + model.W / 3), FrontViewY-150), 40)
            {
                LayerName = Dim,
                TextOverride="WIDTH = <>"
            };
            drawingdoc.Entities.Add(SideviewXDimWidth);

            double sideViewX0 = x0 + model.W / 3 - (double)model.standardbend2;
            double sideViewX1 = x0 + model.W + model.W / 3 + (double)model.standardbend2;

            LinearDim SideviewXDimTotalWidth = new LinearDim(Plane.XY,
                 new Point2D(sideViewX0, FrontViewY),
                 new Point2D(sideViewX1, FrontViewY),

                new Point2D(((sideViewX0) +(sideViewX1))/2, FrontViewY - 230), 40)
            {
                LayerName = Dim,
            };
            drawingdoc.Entities.Add(SideviewXDimTotalWidth);

            LinearDim SideviewHeight = new LinearDim(verticalPlane,
                new Point2D(FrontViewY, -sideViewX0),
              new Point2D(FrontViewY + model.H, -sideViewX0),
               new Point2D(((FrontViewY + model.H) +(FrontViewY))/2, -sideViewX0 + 150), 40)
            {
                LayerName = Dim,
                TextOverride = "HEIGHT = <>"
            };
            drawingdoc.Entities.Add(SideviewHeight);
            #endregion

            Line ll = new Line(Plane.XY, new Point2D(x1, y1), new Point2D(x1, y0));
            drawingdoc.Entities.Add(ll);
       
            return mysheet;
        }
        public MySheet Views(MySheet drawingdoc, double x0, double innerX1,
                     double x1, double y0, double y1,
                     PaintBoothModel model, int scaleFator)
        {
            const string Dim = "Dimension";

            // ── Step 1: Available view area ───────────────────────────────
            double viewAreaW = x1 - x0;
            double viewAreaH = y1 - y0;

            // ── Step 2: Split into 2x2 grid ──────────────────────────────
            double cellW = viewAreaW / 2.0;
            double cellH = viewAreaH / 2.0;

            // ── Step 3: Calculate scale ───────────────────────────────────
            // padding in sheet mm — NO scaleFator here
            double padding = 40.0;
            double usableCellW = cellW - (padding * 2);
            double usableCellH = cellH - (padding * 2);

            // Model extents in model mm
            double modelDepth = model.D + model.D3;
            double modelWidth = model.W;
            double modelHeight = model.H;

            // Scale per view type
            double scaleForFront = Math.Min(usableCellW / modelDepth, usableCellH / modelHeight);
            double scaleForTop = Math.Min(usableCellW / modelDepth, usableCellH / modelWidth);
            double scaleForSide = Math.Min(usableCellW / modelWidth, usableCellH / modelHeight);

            // Most constraining scale across all views
            double rawScale = Math.Min(Math.Min(scaleForFront, scaleForTop), scaleForSide);

            // Round down to nearest standard scale
            double[] standardScales = { 0.5, 0.25, 0.2, 0.1, 0.05, 0.04, 0.02, 0.01 };
            double chosenScale = standardScales
                .Where(s => s <= rawScale)
                .DefaultIfEmpty(0.01)
                .First();

            double isoScale = chosenScale * 0.65;  // ISO slightly smaller to fit

            // Debug — remove after confirming correct
            System.Diagnostics.Debug.WriteLine(
                $"cellW={cellW:F0} cellH={cellH:F0} " +
                $"usableW={usableCellW:F0} usableH={usableCellH:F0} " +
                $"modelD={modelDepth} modelW={modelWidth} modelH={modelHeight} " +
                $"rawScale={rawScale:F4} chosenScale={chosenScale}");

            // ── Step 4: Cell centres ──────────────────────────────────────
            //  [ TOP VIEW  ] [ ISO VIEW  ]   ← row1 (top)
            //  [ FRONT VIEW] [ SIDE VIEW ]   ← row2 (bottom)
            double col1CX = x0 + cellW * 0.5;
            double col2CX = x0 + cellW * 1.5;
            double row1CY = y0 + cellH * 1.5;   // top row centre
            double row2CY = y0 + cellH * 0.5;   // bottom row centre

            // ── Step 5: Place views at cell centres with correct scale ────
            MySheet mysheet = AddTopViews(drawingdoc,
                new Point2D(col1CX, row1CY), true, chosenScale);

            mysheet = AddIsometricViews(drawingdoc,
                new Point2D(col2CX, row1CY), true, isoScale);

            mysheet = AddFrontViews(drawingdoc,
                new Point2D(col1CX, row2CY), true, chosenScale);

            mysheet = AddSideViews(drawingdoc,
                new Point2D(col2CX, row2CY), true, chosenScale);

            // ── Step 6: Grid separator lines ─────────────────────────────
            drawingdoc.Entities.Add(new Line(
                new Point3D(x0, y0 + cellH, 0),
                new Point3D(x1, y0 + cellH, 0)));

            drawingdoc.Entities.Add(new Line(
                new Point3D(x0 + cellW, y0, 0),
                new Point3D(x0 + cellW, y1, 0)));

            // ── Step 7: View labels ───────────────────────────────────────
            double labelH = 3.5 * scaleFator;
            double labelGap = 12.0 * scaleFator;
            int scaleRatio = (int)Math.Round(1.0 / chosenScale);  // e.g. 25 for 1:25

            drawingdoc.Entities.Add(new Text(
                new Point3D(col1CX, y0 + cellH * 2 - labelGap, 0),
                $"TOP VIEW   1:{scaleRatio}",
                labelH, Text.alignmentType.MiddleCenter)
            { LayerName = Dim });

            drawingdoc.Entities.Add(new Text(
                new Point3D(col2CX, y0 + cellH * 2 - labelGap, 0),
                "ISOMETRIC VIEW",
                labelH, Text.alignmentType.MiddleCenter)
            { LayerName = Dim });

            drawingdoc.Entities.Add(new Text(
                new Point3D(col1CX, y0 + cellH - labelGap, 0),
                $"FRONT VIEW   1:{scaleRatio}",
                labelH, Text.alignmentType.MiddleCenter)
            { LayerName = Dim });

            drawingdoc.Entities.Add(new Text(
                new Point3D(col2CX, y0 + cellH - labelGap, 0),
                $"SIDE VIEW   1:{scaleRatio}",
                labelH, Text.alignmentType.MiddleCenter)
            { LayerName = Dim });

            // ── Step 8: Dimension text labels ────────────────────────────
            double dimH = 3.0 * scaleFator;

            drawingdoc.Entities.Add(new Text(
                new Point3D(col1CX, row1CY - cellH * 0.4, 0),
                $"D={model.D}  W={model.W}", dimH,
                Text.alignmentType.MiddleCenter)
            { LayerName = Dim });

            drawingdoc.Entities.Add(new Text(
                new Point3D(col1CX, row2CY - cellH * 0.4, 0),
                $"D={model.D}  H={model.H}", dimH,
                Text.alignmentType.MiddleCenter)
            { LayerName = Dim });

            drawingdoc.Entities.Add(new Text(
                new Point3D(col2CX, row2CY - cellH * 0.4, 0),
                $"W={model.W}  H={model.H}", dimH,
                Text.alignmentType.MiddleCenter)
            { LayerName = Dim });

            // Scale note top-left of view area
            drawingdoc.Entities.Add(new Text(
                new Point3D(x0 + 5 * scaleFator, y1 - 10 * scaleFator, 0),
                $"SCALE 1:{scaleRatio}", dimH,
                Text.alignmentType.TopLeft)
            { LayerName = Dim });

            return mysheet;
        }

    }
}
#endregion

