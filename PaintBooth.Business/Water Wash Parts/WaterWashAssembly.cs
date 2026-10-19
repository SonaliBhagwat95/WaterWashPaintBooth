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
        private double trX, trY, trZ;          // trough local origin placed in booth
        private double hdrX, hdrY0, hdrZ;      // header (item 6): axis X, left flange face Y, centre Z
        private const double TroughH = 340;    // side plate height (local Z 0..340)
        private double chamberH;

        public WaterWashAssembly(IConfiguration configuration) { _cfg = configuration; }
        public void Build(PaintBoothModel model, PaintBoothDesignBusiness panel,
                          Dictionary<string, List<DesignDocument>> designDictionary,
                          double extractionH,string WaterCurtain)
        {
            _m = model;
            _dict = designDictionary;
            chamberH = extractionH;

            double baseFrameH = panel.Section == "Bend Section" ? panel.SettingH : 75;
            zBottom = panel.SheetThickness - baseFrameH;          // underside of base frame
            wallIn = _m.D + _m.D3 - WallT;                        // inner face of tank end wall
                                                                  // ================= LAYOUT - adjust positions HERE =================   (CHECK from GA)
                                                                  //hdrX = _m.D + _m.D3 - 1067;   // side view: back of chamber -> header / delivery axis
            hdrX = _m.D - 138;
            hdrZ = zBottom + 3370;        // side view: tank top -> header centre
            hdrY0 = 140;                   // elevation: header left flange face along width
            trY = 40;                    // elevation: trough start along width

            //DeliveryPipes();
            //HeaderPipe();
            //PQSections();
            //BasePlateReturn();
            //BasePlate1();
            //DistributionPlate();
            //ReturnTroughBracket();
            //FrontPlate();
            //PipeSupportBracket();
            //BasePlatePipeSupport();
            //TieBar();
            //Eliminators();
            //DeflectorGusset();
            //DeflectorPlate();
            //EntrainmentPlate();
            //BasePlateReturnTrough();
            //WaterTrough();          // sets trX, trY, trZ
            //DrainPipes();           // uses them - must come after
            //WaterScreenPanels();
            //WCTopPanels();

            bool hasCurtain = string.Equals(WaterCurtain?.Trim(), "Yes", StringComparison.OrdinalIgnoreCase);

            // ---------- chamber structure - always ----------
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
            WCTopPanels();

            // ---------- water curtain - pipes, trough, screen panels ----------
            if (hasCurtain)
            {
                DeliveryPipes();
                HeaderPipe();
                WaterTrough();          // sets trX, trY, trZ
                DrainPipes();           // uses them - must come after
                WaterScreenPanels();
            }
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
            //AddCopy(doc, pipe, tx + pipeX, ty + total - 100, tz + p3z);
            AddCopy(doc, pipe, tx + pipeX, ty + 100, tz + p3z);

            // ---- Export DWG (local trough, as before) ----
            var path = _cfg["FolderPathConfig:DrawingPath"].ToString();
            if (!Directory.Exists(path + "/PaintBooth drawing"))
                Directory.CreateDirectory(path + "/PaintBooth drawing");
            string dwgFilePath = $"{path}/PaintBooth drawing/WaterTrough {DateTime.Now:hh - mm}.dwg";
            new WriteAutodesk(new WriteAutodeskParams(doc), dwgFilePath).DoWork();

            
            // ---- Place trough in booth: just under chamber top, on booth side ----   (CHECK)
            double topGap = 50;                       // chamber top -> top of trough
            double frontGap = 20;                     // clear of chamber front panel
                                                      // ---- Place trough (from LAYOUT) ----                          (CHECK)
            trX = hdrX + 73;                  // Plate 1 outer face 73 on chamber side of pipe axis
            trZ = hdrZ - 180 - TroughH;       // trough top 180 below pipe centre

            var placed = NewDoc(doc);
            AddCopy(placed, doc, trX, trY + total, trZ, rotZDeg: 180);

            Put("Water_Trough", placed);
        }
        
        private void WaterScreenPanels()
        {
            var wsp = new WaterScreenPanel(_cfg);
            var p1 = wsp.DrawWaterScreenPanel(_m, "WaterScreenPanel1", 1140, 1130, +1, -1);   // QTY 1
            var p2 = wsp.DrawWaterScreenPanel(_m, "WaterScreenPanel2", 1140, 1130, -1, -1);   // QTY 2
            var p3 = wsp.DrawWaterScreenPanel(_m, "WaterScreenPanel3", 572, 570, -1, +1);   // QTY 1

            // ---- Position: hang from trough, lean out toward booth (client side view) ----   (CHECK)
            double panelH = 2348;                           // panel length (use the height inside DrawWaterScreenPanel)
            double tilt = 2.5;                            // deg, bottom leans toward booth (-X)
            double t = tilt * Math.PI / 180.0;

            // panel origin assumed at its BOTTOM edge, panel built upward (+Z)
            //double x = trX - 221 - panelH * Math.Sin(t);    // so the TOP lands on the trough clips
            double x = trX - 221 - panelH * Math.Sin(t);
            double z = trZ - panelH * Math.Cos(t);          // top edge at trough underside

            var doc = NewDoc(p1);
            AddCopy(doc, p1, x, trY + 0, z, rotYDeg: tilt);
            AddCopy(doc, p2, x, trY + 1140, z, rotYDeg: tilt);
            AddCopy(doc, p2, x, trY + 2 * 1140, z, rotYDeg: tilt);
            AddCopy(doc, p3, x, trY + 3 * 1140, z, rotYDeg: tilt);
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
            var pipe3 = dlp.DrawDeliveryPipe(_m, "DeliveryPipe3", 1516, 88.9, 683);   // item 4: 1215-005-15-104
            var pipe4 = dlp.DrawDeliveryPipe4(_m, "DeliveryPipe4", 430, 279, 95.25);   // item 5: 1215-005-15-105

            // ---------- local frame of the delivery line ----------
            // origin = bottom flange face of pipe 3 (50NB end, joins item 3 below), on pipe axis
            double L3 = 1516 + 88.9 + 683;   // 2288
            double gasket = 3;                    // between pipe 3 top flange & pipe 4 bottom flange   (CHECK)

            // ---------- position in booth (move the whole line with these) ----------   (CHECK - not placed yet)
            // ---------- position in booth (from LAYOUT) ----------
            double x = hdrX;
            double y = hdrY0 - 289 - 279;              // axis 568 before header flange
            double z = hdrZ - (L3 + gasket + 430);     // pipe 4 leg centre = header centre
            double rotZ = 90;                          // pipe 4 leg turns toward +Y (along header)
       

            var doc = NewDoc(pipe3);
            AddCopy(doc, pipe3, x, y, z + L3, rotZDeg: rotZ);   // item 4 - top flange face at 2288
            AddCopy(doc, pipe4, x, y, z + L3 + gasket, rotZDeg: rotZ);   // item 5 - sits on pipe 3 flange

            var pipe2 = dlp.DrawDeliveryPipe2(_m, "DeliveryPipe2", 300);          // item 3: 1215-005-15-103
            AddCopy(doc, pipe2, x, y, z - gasket, rotZDeg: rotZ);                 // hangs below pipe 3

            var path = _cfg["FolderPathConfig:DrawingPath"].ToString();
            if (!Directory.Exists(path + "/PaintBooth drawing"))
                Directory.CreateDirectory(path + "/PaintBooth drawing");
            string dwgFilePath = $"{path}/PaintBooth drawing/DeliveryPipes {DateTime.Now:hh - mm}.dwg";
            new WriteAutodesk(new WriteAutodeskParams(doc), dwgFilePath).DoWork();

            Put("Delivery_Pipe", doc);
        }
        private void DrainPipes()
        {
            var drp = new DrainPipe(_cfg);
            var pipe = drp.DrawDrainPipe(_m, "DrainPipe", 1988, 1845, 100);   // 1215-005-15-108, 50NB SS304, QTY 1

            // ---- Position in booth ----   (CHECK - not placed yet)
            // top flange face = bottom flange face of trough drain stub
            double stubL = 300;                  // PipeWithFlange length (DrawPipeWithFlange(_m, 300))
            double gasket = 3;                    // (CHECK)
            double x = trX - 70;                    // stub after 180° turn
            double y = trY + 3996 - 100;            // stub 100 from far end
            double z = trZ + 38 - stubL - gasket;   // under the stub's bottom flange

            var doc = NewDoc(pipe);
            AddCopy(doc, pipe, x, y, z);
            Put("Drain_Pipe", doc);
        }

        private void HeaderPipe()
        {
            var dp = new DistributionPipe(_cfg);
            double[] sockY = { 251, 251 + 800, 251 + 1600, 251 + 2400, 251 + 3200 };   // (CHECK from item 6)
            var hdr = dp.DrawDistributionPipe(_m, "DistributionPipe", 3800, sockY);  // item 6: 1215-005-15-106
            var flow = new FlowPipe(_cfg).DrawFlowPipe(_m, "FlowPipe", 330, 30);      // item 7: 1215-005-15-107

            var doc = NewDoc(hdr);
            AddCopy(doc, hdr, hdrX, hdrY0, hdrZ);                 // first flange face at hdrY0, axis at hdrX / hdrZ

            // flow pipes screwed into sockets, thread engaged 20 into socket          (CHECK)
            double flowZ = hdrZ + dp.SocketBottom + 20 - 330;     // bottom end of flow pipe
            foreach (double y in sockY)
                AddCopy(doc, flow, hdrX, hdrY0 + y, flowZ);

            Put("Distribution_Pipe", doc);
        }

        private void WCTopPanels()
        {
            var tp = new WCTopPanel(_cfg);
            double depth = _m.D3;                     // 1067 - full chamber depth
            double wallT = 38;
            double outerW = _m.W + 2 * wallT;          // 2500 → 2576

            double w1 = Math.Min(2038, outerW);        // first panel - standard 2038
            double w2 = outerW - w1;                   // last panel - remaining (538 here)

            var p1 = tp.DrawWCTopPanel(_m, "WCTopPanel1", depth, w1, mirror: false);   // 1215 005 05 109

            // ---- Position: on top of washing chamber ----                (CHECK)
            double x = _m.D;
            double y0 = -wallT;                        // outer face of chamber side wall
            double z = chamberH;

            var doc = NewDoc(p1);
            AddCopy(doc, p1, x, y0, z);                // Panel 1 - delivery-line end

            if (w2 > 0)
            {
                var p2 = tp.DrawWCTopPanel(_m, "WCTopPanel2", depth, w2, mirror: true);  // 1215 005 05 110
                AddCopy(doc, p2, x, y0 + w1, z);       // last panel - remaining width
            }

            Put("WC_Top_Panel", doc);
            // ---- C frames on each opening (welded to panel in shop) ----   1215 005 05 111
            var cf = new CFrameTopPanel(_cfg).DrawCFrame(_m, "CFrameTopPanel");

            var centres = new List<double>();
            double m1 = w1 - 398 - 200 - 800;
            if (m1 >= 300) centres.Add(y0 + m1 + 400);                     // panel 1 cutout
            if (w2 > 0) centres.Add(y0 + w1);                           // joint opening (398 + 398)
            double m2 = w2 - 398 - 200 - 800;
            if (m2 >= 300) centres.Add(y0 + w1 + 398 + 200 + 400);         // last panel cutout

            double fx = x + depth / 2;                                     // centred across chamber depth
            foreach (double yc in centres)
                AddCopy(doc, cf, fx, yc, z);                               // sits on panel top face
        }
    }
}