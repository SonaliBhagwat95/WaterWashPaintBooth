using devDept.Eyeshot;
using devDept.Eyeshot.Control;
using devDept.Eyeshot.Translators;
using global::PaintBooth.Models;
using Microsoft.Extensions.Configuration;
using PaintBooth.Business.Water_Wash_Parts.Water_Trough;
using static PaintBooth.Business.SheetProfile;
using static PaintBooth.Business.Water_Wash_Parts.AssemblyHelper;
namespace PaintBooth.Business.Water_Wash_Parts
{
    public class WaterWashAssembly
    {
        private readonly IConfiguration _cfg;
        private PaintBoothModel _m;
        private Dictionary<string, List<DesignDocument>> _dict;

        // ---- shared positions (set step by step, used by later parts) ----
        private double zBottom, wallIn;
        private const double WallT = 38;
        private double retX, retZ;
        private double bpX, bpZ, bpY0 = 0;
        private double cornerX, cornerZ;
        private double brX, brZ;
        private double[] brY = { 400 /*, 1600, 2800*/ };        // (CHECK)
        private double psX, psZ;
        private double pfX, pfZ;
        private double gx, gz;

        public WaterWashAssembly(IConfiguration configuration) { _cfg = configuration; }

        public void Build(PaintBoothModel model, PaintBoothDesignBusiness panel,
                          Dictionary<string, List<DesignDocument>> designDictionary)
        {
            _m = model;
            _dict = designDictionary;

            double baseFrameH = panel.Section == "Bend Section" ? panel.SettingH : 75;
            zBottom = panel.SheetThickness - baseFrameH;          // underside of base frame
            wallIn = _m.D + _m.D3 - WallT;                        // inner face of tank end wall
            DeliveryPipes();
            DrainPipes();
            PQSections();
            BasePlateReturn();
            BasePlate1();
            DistributionPlate();
            ReturnTroughBracket();
            FrontPlate();
            PipeSupportBracket();
            BasePlatePipeSupport();
            TieBar();
            Eliminators();
            DeflectorGusset();
            DeflectorPlate();
            EntrainmentPlate();
            BasePlateReturnTrough();
            WaterTrough();
            WaterScreenPanels();
        }

        private void Put(string key, DesignDocument doc) =>
            _dict[key] = new List<DesignDocument> { doc };

        // ===================== parts =====================

        private void PQSections()
        {
            var p = new PSection(_cfg);
            double qTotal = (double)_m.D + (double)_m.D3;
            double qSegment = 2390;

            Put("PSection", p.DrawPSection(_m, zBottom));
            Put("QSection_Left", p.DrawQSection(_m, zBottom, qTotal, qSegment, false));
            Put("QSection_Right", p.DrawQSection(_m, zBottom, qTotal, qSegment, true));
            Put("BackQSection", p.DrawbackQSection(_m, zBottom, qTotal));
        }

        private void BasePlateReturn()
        {
            var part = new BasePlateReturn(_cfg).DrawBasePlateReturn(_m, zBottom);

            double gap = 20, partD = 131 + 2 * 1.6;
            retX = _m.D + _m.D3 - WallT - gap;
            retZ = zBottom - 460;
            double[] retY = { 800 };                              // (CHECK)

            var doc = NewDoc(part);
            foreach (double y in retY)
                AddCopy(doc, part, retX, y + partD / 2, retZ, rotZDeg: 180);
            Put("BasePlate_Return", doc);
        }

        private void BasePlate1()
        {
            var part = new BasePlate1(_cfg).DrawBasePlate1(_m);

            double depth = 2318, T = 1.6;
            double slope = Math.Atan2(211 - 30, 682);
            bpX = wallIn;
            bpZ = retZ + 368 - 30 - (bpX - retX) * Math.Tan(slope) + T / Math.Cos(slope);

            var doc = NewDoc(part);
            AddCopy(doc, part, bpX, bpY0 + depth, bpZ, rotZDeg: 180);
            Put("BasePlate_1", doc);
        }

        private void DistributionPlate()
        {
            var d = new DistributionPlate1(_cfg);
            var plate1 = d.DrawDistributionPlate(_m, "DistributionPlate1", 2318, 5 + 1128);
            d.DrawDistributionPlate(_m, "DistributionPlate2", 1678, 5 + 1130);   // DWG only for now

            double a1 = Math.Atan2(211 - 30, 682);
            double a2 = a1 - (Math.PI - 154 * Math.PI / 180.0);
            double legEndX = 682 + 120 * Math.Cos(a2);
            double legTopZ = 211 + 120 * Math.Sin(a2) + 25 / Math.Cos(a2);
            cornerX = bpX - legEndX;
            cornerZ = bpZ + legTopZ;

            double t = 1.6;
            double x = cornerX - t / 2 + d.HookTopX;
            double z = cornerZ + t / 2 - d.HookTopZ;

            var doc = NewDoc(plate1);
            AddCopy(doc, plate1, x, bpY0 + 2318, z, rotZDeg: 180);
            Put("Distribution_Plate1", doc);
        }

        private void ReturnTroughBracket()
        {
            new ReturnTroughBracket(_cfg).DrawReturnTroughBracket(_m, 2390);      // DWG only for now
        }

        private void FrontPlate()
        {
            var fp = new FrontPlate(_cfg);
            var p203 = fp.DrawFrontPlate(_m, "FrontPlate_203", 1158);
            fp.DrawFrontPlate(_m, "FrontPlate_204", 518);                         // DWG only for now

            double x = _m.D, z = zBottom;                         // (CHECK)
            double[] ys = { 0, 1158 };                            // (CHECK)

            var doc = NewDoc(p203);
            foreach (double y in ys) AddCopy(doc, p203, x, y, z);
            Put("Front_Plate", doc);
        }

        private void PipeSupportBracket()
        {
            var part = new PipeSupportBracket(_cfg).DrawPipeSupportBracket(_m, 300);

            double brT = 3;
            brX = cornerX - 1.6 - brT / 2;
            brZ = cornerZ + 15;                                   // (CHECK)

            var doc = NewDoc(part);
            foreach (double y in brY) AddCopy(doc, part, brX, y, brZ);
            Put("PipeSupport_Bracket", doc);
        }

        private void BasePlatePipeSupport()
        {
            var bps = new BasePlatePipeSupport(_cfg);
            var part = bps.DrawBasePlatePipeSupport(_m);

            double ang = bps.PipeDeg * Math.PI / 180.0;
            double ux = Math.Cos(ang), uz = Math.Sin(ang);
            double nx = -Math.Sin(ang), nz = Math.Cos(ang);
            double off = 3.0 / 2 + bps.PipeOD / 2;

            double axX = brX + nx * off, axZ = brZ + nz * off;
            psX = wallIn - 3;                                     // 90° angle between wall and bracket
            double wX = psX - bps.AxisX;
            double wZ = axZ + (wX - axX) / ux * uz;
            psZ = wZ - bps.AxisZ;

            var doc = NewDoc(part);
            foreach (double y in brY) AddCopy(doc, part, psX, y + 300, psZ, rotZDeg: 180);
            Put("BasePlate_PipeSupport", doc);
        }

        private void TieBar()
        {
            var part = new PipeSupportFlat(_cfg).DrawTieBar(_m);

            pfX = psX - 3;                                        // against pipe-support bracket
            pfZ = psZ;

            var doc = NewDoc(part);
            foreach (double y in brY) AddCopy(doc, part, pfX, y + 300, pfZ, rotZDeg: 180);
            Put("Tie_Bar", doc);
        }

        private void Eliminators()
        {
            var part = new Eliminator(_cfg).DrawEliminator(_m, "Eliminator1", 1150);

            double slopeDeg = 9;                                  // (CHECK)
            double sA = Math.Sin(slopeDeg * Math.PI / 180.0), cA = Math.Cos(slopeDeg * Math.PI / 180.0);
            double rotY = 90 - slopeDeg;
            double H = 1028, T = 1.6, W = 1150, elY = 0;          // (CHECK elY)

            double restX = wallIn - 9;                            // (CHECK)
            double restZ = psZ + 3 + 3;                           // top face of 90° angle
            double elX = restX - (T * sA + H * cA);
            double elZ = restZ - (-T * cA + H * sA);

            var bottom = NewDoc(part);
            AddCopy(bottom, part, elX, elY, elZ, rotYDeg: rotY);
            Put("Eliminator1", bottom);

            var upper = NewDoc(part);
            AddCopy(upper, part, restX, elY + W, restZ + 197, rotYDeg: rotY, rotZDeg: 180);   // middle, reversed
            AddCopy(upper, part, elX, elY, elZ + 197 + 508, rotYDeg: rotY);                   // top
            Put("Eliminator_Upper", upper);
        }

        private void DeflectorGusset()
        {
            var dg = new DeflectorGusset(_cfg);
            var g218 = dg.DrawDeflectorGusset(_m, "DeflectorGusset_218", +1);
            var g219 = dg.DrawDeflectorGusset(_m, "DeflectorGusset_219", -1);

            double a = 25 * Math.PI / 180.0;
            double rx = 203 * Math.Cos(a) + 38, rz = 203 * Math.Sin(a) + 10;
            double holeX = rx - 16, holeZ = rz + 64, t = 1.6;

            double tieHoleX = pfX - 1004, tieHoleZ = pfZ - 259;
            gx = tieHoleX + holeX;
            gz = tieHoleZ - holeZ;

            var doc = NewDoc(g218);
            foreach (double y in brY)
            {
                double tongueY = y + 152.5;
                AddCopy(doc, g218, gx, tongueY - 2.5, gz, rotZDeg: 180);
                AddCopy(doc, g219, gx, tongueY + 2.5 + t, gz, rotZDeg: 180);
            }
            Put("Deflector_Gusset", doc);
        }
        private void DeflectorPlate()
        {
            var part = new DeflectorPlate(_cfg).DrawDeflectorPlate(_m, "DeflectorPlate1", 2318);

            double a = 25 * Math.PI / 180.0, t = 1.6, over = 2, len = 2318, y0 = 0;   // (CHECK y0)
            double x = gx + over * Math.Cos(a) - (t / 2) * Math.Sin(a);
            double z = gz - over * Math.Sin(a) - (t / 2) * Math.Cos(a);

            var doc = NewDoc(part);
            AddCopy(doc, part, x, y0 + len, z, rotZDeg: 180);
            Put("Deflector_Plate1", doc);
        }

        private void EntrainmentPlate()
        {
            var ep = new EntrainmentPlate(_cfg);
            ep.DrawEntrainmentPlate(_m, "EntrainmentPlate1", 1152);              // DWG only for now
            ep.DrawEntrainmentPlate(_m, "EntrainmentPlate2", 518);
        }
        private void BasePlateReturnTrough()
        {
            new BasePlateReturnTrough(_cfg).DrawBasePlateReturnTrough(_m, new double[] { 127, 587 });   // DWG only for now
        }

        private void WaterTrough()
        {
            // ---------- trough local frame ----------
            // X = 0 : outer face of side plate (left in section X-X), Z = 0 : bottom of side plate
            // Y = 0 : outer face of first side plate, Y = 3996 : outer face of second
            double sideT = 2, inner = 3992, total = inner + 2 * sideT;     // 3996

            // ---- trough position in booth ----                         (CHECK - from GA)
            double tx = 0, ty = 0, tz = 0;

            // ---------- parts ----------
            var p1 = new Plate1(_cfg).DrawPlate1(_m, inner);
            var p2 = new Plate2(_cfg).DrawPlate2(_m, inner);
            var p3 = new Plate3(_cfg).DrawPlate3(_m, inner);
            var sp = new SidePlate(_cfg);
            var spA = sp.DrawSidePlate(_m, "SidePlate_A", mirror: false);
            sp.DrawSidePlate(_m, "SidePlate_B", mirror: true);             // opposite hand - DWG only
            var pipe = new PipeWithFlange(_cfg).DrawPipeWithFlange(_m, 300);

            var doc = NewDoc(p1);
            double y0 = ty + sideT;                                         // plates start inside side plate

            // ④ side plates - both ends, same hole pattern in assembly
            AddCopy(doc, spA, tx, ty, tz);
            AddCopy(doc, spA, tx, ty + total - sideT, tz);

            // ③ Plate 3 - body: base outer face Z = 38, left lip against Plate 1 inner face
            double p3x = 2, p3z = 38;
            AddCopy(doc, p3, tx + p3x, y0, tz + p3z);

            // ① Plate 1 - L: outer face X = 0, stands on Plate 3 base (Z = 40 .. 340), lip +X
            AddCopy(doc, p1, tx + 0, y0, tz + 40);

            // ② Plate 2 - C: web outer face X = 140, top Z = 340, lips toward Plate 1 (turn 180°)
            double p2Web = 248;
            AddCopy(doc, p2, tx + 140, y0 + inner, tz + 340 - p2Web, rotZDeg: 180);

            // ⑤ clips - 8 Nos at 248 + n x 500, hooked under Plate 3 bottom-right   (CHECK - detail 'Y')
            double clipT = 2, clipW = 45;                                   // thickness, length along Y
            var clipC = new List<(double X, double Z)>
    {
        (167, 37),  (221, 37),  (221, -20),  (180, -20)            // centreline, scaled from section
    };
            Thicken(clipC, clipT / 2, out var cL, out var cR);
            var clip = new DesignDocument();
            clip.Units = doc.Units;
            AddPiece(clip, cL, cR, 0, clipC.Count - 1, -clipW / 2, clipW / 2);   // centred on Y = 0
            for (int i = 0; i < 8; i++)
                AddCopy(doc, clip, tx, ty + 248 + i * 500, tz);

            // drain pipe with flange - 100 from right end, hanging under Plate 3 base
            double pipeX = 70;                                              // across trough     (CHECK)
            AddCopy(doc, pipe, tx + pipeX, ty + total - 100, tz + p3z);

            // ---- Export DWG ----
            var path = _cfg["FolderPathConfig:DrawingPath"].ToString();
            if (!Directory.Exists(path + "/PaintBooth drawing"))
                Directory.CreateDirectory(path + "/PaintBooth drawing");
            string dwgFilePath = $"{path}/PaintBooth drawing/WaterTrough {DateTime.Now:hh - mm}.dwg";
            new WriteAutodesk(new WriteAutodeskParams(doc), dwgFilePath).DoWork();

            Put("Water_Trough", doc);
        }
        private void WaterScreenPanels()
        {
            var wsp = new WaterScreenPanel(_cfg);
            var p1 = wsp.DrawWaterScreenPanel(_m, "WaterScreenPanel1", 1140, 1130, +1, -1);   // QTY 1
            var p2 = wsp.DrawWaterScreenPanel(_m, "WaterScreenPanel2", 1140, 1130, -1, -1);   // QTY 2
            var p3 = wsp.DrawWaterScreenPanel(_m, "WaterScreenPanel3", 572, 570, -1, +1);   // QTY 1

            // ---- Position in booth ----                                   (CHECK - not placed yet)
            double x = 0, z = 0;
            var doc = NewDoc(p1);
            AddCopy(doc, p1, x, 0, z);
            AddCopy(doc, p2, x, 1140, z);
            AddCopy(doc, p2, x, 2 * 1140, z);
            AddCopy(doc, p3, x, 3 * 1140, z);
            // ---- Export DWG ----
            var path = _cfg["FolderPathConfig:DrawingPath"].ToString();
            if (!Directory.Exists(path + "/PaintBooth drawing"))
                Directory.CreateDirectory(path + "/PaintBooth drawing");
            string dwgFilePath = $"{path}/PaintBooth drawing/WaterScreenPanels {DateTime.Now:hh - mm}.dwg";
            new WriteAutodesk(new WriteAutodeskParams(doc), dwgFilePath).DoWork();

            Put("Water_Screen_Panel", doc);
        }
        private void DeliveryPipes()
        {
            var dlp = new DeliveryPipe(_cfg);
            var pipe3 = dlp.DrawDeliveryPipe(_m, "DeliveryPipe3", 1516, 88.9, 683);   // 1215-005-15-104, QTY 1

            // ---- Position in booth ----                       (CHECK - not placed yet)
            double x = 0, y = 0, z = 0;                          // top flange face, on pipe axis

            var doc = NewDoc(pipe3);
            AddCopy(doc, pipe3, x, y, z);
            Put("Delivery_Pipe", doc);

            var pipe4 = dlp.DrawDeliveryPipe4(_m, "DeliveryPipe4", 430, 279, 95.25);   // 1215-005-15-105, 65NB SS304, QTY 1

            // ---- Position in booth ----   (CHECK - not placed yet)
            double x4 = 0, y4 = 0, z4 = 0;   // bottom flange face, on pipe axis

            var doc4 = NewDoc(pipe4);
            AddCopy(doc4, pipe4, x4, y4, z4);
            Put("Delivery_Pipe4", doc4);

        }
        private void DrainPipes()
        {
            var drp = new DrainPipe(_cfg);
            var pipe = drp.DrawDrainPipe(_m, "DrainPipe", 1988, 1845, 100);   // 1215-005-15-108, 50NB SS304, QTY 1

            // ---- Position in booth ----   (CHECK - not placed yet)
            double x = 0, y = 0, z = 0;     // top flange face, on pipe axis

            var doc = NewDoc(pipe);
            AddCopy(doc, pipe, x, y, z);
            Put("Drain_Pipe", doc);
        }
    }
}