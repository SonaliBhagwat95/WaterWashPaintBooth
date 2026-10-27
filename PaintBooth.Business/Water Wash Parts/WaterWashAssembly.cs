using devDept.Eyeshot;
using devDept.Eyeshot.Entities;
using devDept.Eyeshot.Translators;
using devDept.Geometry;
using global::PaintBooth.Models;
using Microsoft.Extensions.Configuration;
using PaintBooth.Business.Water_Wash_Parts.Water_Trough;
using System.Text;
using static PaintBooth.Business.SheetProfile;
using static PaintBooth.Business.Water_Wash_Parts.AssemblyHelper;
using BasePlate1Part = PaintBooth.Business.BasePlate1;
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
    private double bpTan, bpYWall, bpZDrainTop;    // sloped tank base plate (set in TankBaseSupports)

    /// <summary>Top face Z of the sloped tank base plate at width position y.</summary>
    private double TankPlateTopZ(double y) => bpZDrainTop + (bpYWall - y) * bpTan;

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


        bool hasCurtain = string.Equals(WaterCurtain?.Trim(), "Yes", StringComparison.OrdinalIgnoreCase);

        // ---------- chamber structure - always ----------
        PQSections();
        BasePlateReturn();        // sets retZ
        TankBaseSupports();       // needs retZ + wallIn - sets base-plate line
        BasePlate1();
        DistributionPlate();
        ReturnTroughBracket();
        FrontPlate();
        TurnbuckleSupports();     // stands on base plate - must come after TankBaseSupports
        PipeSupportBracket();
        BasePlatePipeSupport();
        TieBar();
        Eliminators();
        DeflectorGusset();
        DeflectorPlate();
        EntrainmentPlate();
        BasePlateReturnTrough();
        GratingSupports();
        AngleSupports();
        Gratings();
        WCTopPanels();
        PumpSump();
        PumpBase();
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

    //private void BasePlateReturn()
    //{
    //    var part = new BasePlateReturn(_cfg).DrawBasePlateReturn(_m, zBottom);

    //    double gap = 20, partD = 131 + 2 * 1.6;
    //    retX = _m.D + _m.D3 - WallT - gap;
    //    retZ = zBottom - 460;
    //    double[] retY = { 800 };                              // (CHECK)

    //    var doc = NewDoc(part);
    //    foreach (double y in retY)
    //        AddCopy(doc, part, retX, y + partD / 2, retZ, rotZDeg: 180);
    //    Put("BasePlate_Return", doc);
    //}

    //private void BasePlate1()
    //{
    //    var part = new BasePlate1(_cfg).DrawBasePlate1(_m);

    //    double depth = 2318, T = 1.6;
    //    double slope = Math.Atan2(211 - 30, 682);
    //    bpX = wallIn;
    //    bpZ = retZ + 368 - 30 - (bpX - retX) * Math.Tan(slope) + T / Math.Cos(slope);

    //    var doc = NewDoc(part);
    //    AddCopy(doc, part, bpX, bpY0 + depth, bpZ, rotZDeg: 180);
    //    Put("BasePlate_1", doc);
    //}
    private void BasePlate1()
    {
        const double FullLen = 2318;   // fixed plate length
        const double MinPiece = 1;      // ignore leftovers smaller than this

        double W = _m.W;            // <-- booth width (same property as DistributionPlate)
        double T = 1.6;

        // ---- position (same as before, doesn't depend on plate length) ----
        double slope = Math.Atan2(211 - 30, 682);
        bpX = wallIn;
        bpZ = retZ + 368 - 30 - (bpX - retX) * Math.Tan(slope) + T / Math.Cos(slope);

        var bp = new BasePlate1(_cfg);
        DesignDocument doc = null;
        double yStart = 0;
        int idx = 1;

        while (W - yStart > MinPiece)
        {
            double len = Math.Min(FullLen, W - yStart);   // 2318, 2318, ..., remainder

            var part = bp.DrawBasePlate1(_m, $"BasePlate{idx}", len);

            if (doc == null) doc = NewDoc(part);

            // rotZ 180 flips extrusion to -Y, so offset by +len to land on [yStart, yStart+len]
            AddCopy(doc, part, bpX, bpY0 + yStart + len, bpZ, rotZDeg: 180);

            yStart += len;
            idx++;
        }

        if (doc != null)
            Put("BasePlate_1", doc);
    }
   
    private void DistributionPlate()
    {
        const double FullLen = 2318;      // fixed plate length
        const double SlotFromLeft = 5 + 1128;  // slot position from plate's left end
        const double MinPiece = 1;         // ignore leftovers smaller than this

        double W = _m.W;                   // <-- booth width (use your actual property)

        // ---- corner position (same as before, doesn't depend on plate length) ----
        double a1 = Math.Atan2(211 - 30, 682);
        double a2 = a1 - (Math.PI - 154 * Math.PI / 180.0);
        double legEndX = 682 + 120 * Math.Cos(a2);
        double legTopZ = 211 + 120 * Math.Sin(a2) + 25 / Math.Cos(a2);
        cornerX = bpX - legEndX;
        cornerZ = bpZ + legTopZ;

        double t = 1.6;
        var d = new DistributionPlate1(_cfg);

        DesignDocument doc = null;
        double yStart = 0;
        int idx = 1;

        while (W - yStart > MinPiece)
        {
            double len = Math.Min(FullLen, W - yStart);   // 2318, 2318, ..., remainder

            var plate = d.DrawDistributionPlate(_m, $"DistributionPlate{idx}", len, SlotFromLeft);

            double x = cornerX - t / 2 + d.HookTopX;
            double z = cornerZ + t / 2 - d.HookTopZ;

            if (doc == null) doc = NewDoc(plate);

            // rotZ 180 flips the extrusion to -Y, so offset by +len to land on [yStart, yStart+len]
            AddCopy(doc, plate, x, bpY0 + yStart + len, z, rotZDeg: 180);

            yStart += len;
            idx++;
        }

        if (doc != null)
            Put("Distribution_Plate", doc);
    }
    private void ReturnTroughBracket()
    {
        new ReturnTroughBracket(_cfg).DrawReturnTroughBracket(_m, 2390);      // DWG only for now
    }

   
    private void FrontPlate()
    {
        const double FullLen = 1158;   // fixed plate length
        const double MinPiece = 1;      // ignore leftovers smaller than this

        double W = _m.W;            // <-- booth width (same property as the others)

        double x = _m.D, z = zBottom;   // (CHECK)

        var fp = new FrontPlate(_cfg);
        DesignDocument doc = null;
        double yStart = 0;
        int idx = 203;                  // keeps your FrontPlate_203, _204, ... naming

        while (W - yStart > MinPiece)
        {
            double len = Math.Min(FullLen, W - yStart);   // 1158, 1158, ..., remainder

            var part = fp.DrawFrontPlate(_m, $"FrontPlate_{idx}", len);

            if (doc == null) doc = NewDoc(part);

            AddCopy(doc, part, x, yStart, z);             // no rotation -> place at yStart

            yStart += len;
            idx++;
        }

        if (doc != null)
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

    //private void Eliminators()
    //{
    //    var part = new Eliminator(_cfg).DrawEliminator(_m, "Eliminator1", 1150);

    //    double slopeDeg = 9;                                  // (CHECK)
    //    double sA = Math.Sin(slopeDeg * Math.PI / 180.0), cA = Math.Cos(slopeDeg * Math.PI / 180.0);
    //    double rotY = 90 - slopeDeg;
    //    double H = 1028, T = 1.6, W = 1150, elY = 0;          // (CHECK elY)

    //    double restX = wallIn - 9;                            // (CHECK)
    //    double restZ = psZ + 3 + 3;                           // top face of 90° angle
    //    double elX = restX - (T * sA + H * cA);
    //    double elZ = restZ - (-T * cA + H * sA);

    //    var bottom = NewDoc(part);
    //    AddCopy(bottom, part, elX, elY, elZ, rotYDeg: rotY);
    //    Put("Eliminator1", bottom);

    //    var upper = NewDoc(part);
    //    AddCopy(upper, part, restX, elY + W, restZ + 197, rotYDeg: rotY, rotZDeg: 180);   // middle, reversed
    //    AddCopy(upper, part, elX, elY, elZ + 197 + 508, rotYDeg: rotY);                   // top
    //    Put("Eliminator_Upper", upper);
    //}

    private void Eliminators()
    {
        const double FullLen = 1150;   // fixed eliminator width
        const double MinPiece = 1;      // ignore leftovers smaller than this (see note)

        double boothW = _m.W;       // <-- booth width (same property as the others)

        // ---- position (same as before, doesn't depend on piece width) ----
        double slopeDeg = 9;                                  // (CHECK)
        double sA = Math.Sin(slopeDeg * Math.PI / 180.0), cA = Math.Cos(slopeDeg * Math.PI / 180.0);
        double rotY = 90 - slopeDeg;
        double H = 1028, T = 1.6, elY = 0;                    // (CHECK elY)

        double restX = wallIn - 9;                            // (CHECK)
        double restZ = psZ + 3 + 3;                           // top face of 90° angle
        double elX = restX - (T * sA + H * cA);
        double elZ = restZ - (-T * cA + H * sA);

        var el = new Eliminator(_cfg);
        DesignDocument bottom = null, upper = null;
        double yStart = 0;
        int idx = 1;

        while (boothW - yStart > MinPiece)
        {
            double len = Math.Min(FullLen, boothW - yStart);   // 1150, 1150, ..., remainder

            var part = el.DrawEliminator(_m, $"Eliminator{idx}", len);

            if (bottom == null) bottom = NewDoc(part);
            if (upper == null) upper = NewDoc(part);

            double y = elY + yStart;

            // bottom row
            AddCopy(bottom, part, elX, y, elZ, rotYDeg: rotY);

            // middle row, reversed (rotZ 180 -> offset by +len)
            AddCopy(upper, part, restX, y + len, restZ + 197, rotYDeg: rotY, rotZDeg: 180);

            // top row
            AddCopy(upper, part, elX, y, elZ + 197 + 508, rotYDeg: rotY);

            yStart += len;
            idx++;
        }

        if (bottom != null) Put("Eliminator1", bottom);
        if (upper != null) Put("Eliminator_Upper", upper);
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
    //private void DeflectorPlate()
    //{
    //    var part = new DeflectorPlate(_cfg).DrawDeflectorPlate(_m, "DeflectorPlate1", 2318);

    //    double a = 25 * Math.PI / 180.0, t = 1.6, over = 2, len = 2318, y0 = 0;   // (CHECK y0)
    //    double x = gx + over * Math.Cos(a) - (t / 2) * Math.Sin(a);
    //    double z = gz - over * Math.Sin(a) - (t / 2) * Math.Cos(a);

    //    var doc = NewDoc(part);
    //    AddCopy(doc, part, x, y0 + len, z, rotZDeg: 180);
    //    Put("Deflector_Plate1", doc);
    //}
    private void DeflectorPlate()
    {
        const double FullLen = 2318;   // fixed plate length
        const double MinPiece = 1;      // ignore leftovers smaller than this

        double W = _m.W;            // <-- booth width (same property as the others)

        // ---- position (same as before, doesn't depend on plate length) ----
        double a = 25 * Math.PI / 180.0, t = 1.6, over = 2, y0 = 0;   // (CHECK y0)
        double x = gx + over * Math.Cos(a) - (t / 2) * Math.Sin(a);
        double z = gz - over * Math.Sin(a) - (t / 2) * Math.Cos(a);

        var dp = new DeflectorPlate(_cfg);
        DesignDocument doc = null;
        double yStart = 0;
        int idx = 1;

        while (W - yStart > MinPiece)
        {
            double len = Math.Min(FullLen, W - yStart);   // 2318, 2318, ..., remainder

            var part = dp.DrawDeflectorPlate(_m, $"DeflectorPlate{idx}", len);

            if (doc == null) doc = NewDoc(part);

            // rotZ 180 flips extrusion to -Y, so offset by +len to land on [yStart, yStart+len]
            AddCopy(doc, part, x, y0 + yStart + len, z, rotZDeg: 180);

            yStart += len;
            idx++;
        }

        if (doc != null)
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
        var part = new BasePlateReturnTrough(_cfg).DrawBasePlateReturnTrough(_m, new double[] { 127, 587 });

        double botW = 196, lenTop = 1870;

        // ---- centres along width, measured from drain-end wall (client elevation) ----   (CHECK exact dims)
        double[] dFromDrain = { 830, 2300, 3095 };

        // ---- across depth: 45° cut end against back wall, square end toward booth ----    (CHECK)
        double xEnd = wallIn;

        var doc = NewDoc(part);
        foreach (double d in dFromDrain)
        {
            double yc = bpYWall - d;
            if (yc - botW / 2 < 0) continue;             // narrow booth: skip ones past pump-end wall

            double z = TankPlateTopZ(yc);                // stands on sloped base plate
            AddCopy(doc, part, xEnd - lenTop, yc + botW / 2, z, rotZDeg: -90);
        }
        Put("BasePlate_ReturnTrough", doc);
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

        //// ---- Export DWG (local trough, as before) ----
        //var path = _cfg["FolderPathConfig:DrawingPath"].ToString();
        //if (!Directory.Exists(path + "/PaintBooth drawing"))
        //    Directory.CreateDirectory(path + "/PaintBooth drawing");
        //string dwgFilePath = $"{path}/PaintBooth drawing/WaterTrough {DateTime.Now:hh - mm}.dwg";
        //new WriteAutodesk(new WriteAutodeskParams(doc), dwgFilePath).DoWork();

        
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

    private void TurnbuckleSupports()
    {
        var ts = new TurnbuckleSupport(_cfg);

        double topZ = zBottom - 215;              // distributor plate underside
        double x = _m.D + 20 + 50;             // under the plate lip

        double firstY = 400, pitch = 1000;        // (CHECK pitch vs client GA)
        double[] dSup = { 302, 302 + 1118, 302 + 1118 + 600, 302 + 1118 + 600 + 839, 302 + 1118 + 600 + 839 + 839 };
        var supY = dSup.Select(d => bpYWall - d).Where(y => y > 100).ToArray();   // skip past pump-end wall
        //double[] supY = { firstY, firstY + pitch };

        DesignDocument doc = null;
        foreach (double y in supY)
        {
            double footZ = TankPlateTopZ(y);      // bottom angles rest on sloped base plate
            double L = topZ - footZ;          // different for each support
            if (L < TurnbuckleSupport.MinL || L > TurnbuckleSupport.MaxL)
                System.Diagnostics.Debug.WriteLine($"Turnbuckle at Y={y}: L={L:0} outside {TurnbuckleSupport.MinL}-{TurnbuckleSupport.MaxL}");

            var part = ts.DrawTurnbuckleSupport(_m, $"TurnbuckleSupport_{y:0}", L);
            doc ??= NewDoc(part);
            AddCopy(doc, part, x, y, footZ);
        }

        if (doc != null) Put("Turnbuckle_Support", doc);
    }

    private void TankBaseSupports()
    {
        // ---- sloped base plate: falls 0.5° toward drain end ----              (client elevation)
        double slopeDeg = 0.5;
        double tanS = Math.Tan(slopeDeg * Math.PI / 180.0);
        double plateT = 3;                         // base plate thk
        double hEnd = 38 - plateT;               // plate underside at drain-end wall (38 = to plate top)

        double yWall = _m.W;                         // drain-end inner wall (drain at high Y)
        double extraPumpEnd = 0;                     // our tank ends at booth width (no pump compartment)
        double yPump = yWall - (_m.W + extraPumpEnd);// pump-end inner wall (= 0)

        double first = 538, pitch = 1000, chW = 75;  // S at 538, then every 1000 (R, Q, P)
        //double x0 = _m.D;                         // channel runs across tank depth       (CHECK)
        //double len = wallIn - _m.D;
        double x0  = WallT;                          // from tank front wall (booth entry) ...
        double len = wallIn - x0;                    // ... to back wall - full tank depth
        // ---- U-channel supports P / Q / R / S ----
        var ts = new TankBaseSupport(_cfg);
        DesignDocument doc = null;

        for (double d = first; yWall - d - chW / 2 >= yPump + 100; d += pitch)   // keep 100 clear of pump-end wall
        {
            double hL = hEnd + (d + chW / 2) * tanS;                 // leg toward pump end - taller
            double hR = hEnd + (d - chW / 2) * tanS;                 // leg toward drain
            System.Diagnostics.Debug.WriteLine($"Tank support d={d}: {hL:0.0} / {hR:0.0}");

            var part = ts.DrawChannel(_m, hL, hR, len);
            doc ??= NewDoc(part);
            AddCopy(doc, part, x0, yWall - d - chW / 2, retZ);       // stands on tank bottom
        }

        // ---- sloped base plate 3 thk on the support lips ----
        bpTan = tanS;
        bpYWall = yWall;
        double zDrain = retZ + hEnd;                   // plate underside at drain end
        bpZDrainTop = zDrain + plateT;                 // plate TOP at drain end (= retZ + 38)
        double zPump = zDrain + (yWall - yPump) * tanS;

        var plateProfile = new LinearPath(new[]
        {
            new Point3D(x0, yWall, zDrain),
            new Point3D(x0, yWall, zDrain + plateT),
            new Point3D(x0, yPump, zPump  + plateT),
            new Point3D(x0, yPump, zPump),
            new Point3D(x0, yWall, zDrain)
        });
        var plate = new devDept.Eyeshot.Entities.Region(plateProfile).ExtrudeAsBrep(new Vector3D(len, 0, 0));

        doc ??= new DesignDocument { Units = linearUnitsType.Millimeters };
        doc.Entities.Add(plate, System.Drawing.Color.SaddleBrown);

        Put("Tank_Base_Support", doc);
    }

   
    // ---- grating layout: 660 gratings with 5 gaps, ends take the remainder ----
    private (double L, double y0, double z0, List<double> joints, List<(double start, double len)> segs) GratingLayout()
    {
        double L = _m.W - 5;                                   // client 4000 → 3995
        double y0 = (_m.W - L) / 2;
        double z0 = zBottom - 30 - 40;                          // support box bottom: grating 30 + box 40 below tank top

        int g = Math.Max(1, (int)Math.Floor((L + 5) / 666));    // gratings that fit (6 on 4000)
        double mid = 661, gap = 5;
        double e = (L - (g - 2) * mid - (g - 1) * gap) / 2;      // end segments (663 on 4000)
        if (g == 1) e = L;

        var segs = new List<(double, double)>();
        var joints = new List<double>();
        double y = 0;
        for (int k = 0; k < g; k++)
        {
            double len = (k == 0 || k == g - 1) ? e : mid;
            segs.Add((y, len));
            y += len;
            if (k < g - 1) { joints.Add(y + gap / 2); y += gap; }
        }
        return (L, y0, z0, joints, segs);
    }

    private void GratingSupports()
    {
        var lay = GratingLayout();
        var part = new GratingSupport(_cfg).DrawGratingSupport(_m, "GratingSupport", lay.L, lay.joints.ToArray());

        // client plan: cross supports bolt to tank front wall (booth entry), long member 665 in   (CHECK)
        double xFront = WallT;                                 // inner face of tank front wall at booth start

        var doc = NewDoc(part);
        // turned 180°: local X = 675 (cross support far plate) lands on front wall, flat faces into booth
        AddCopy(doc, part, xFront + 675, lay.y0 + lay.L, lay.z0, rotZDeg: 180);
        Put("Grating_Support", doc);
    }

    private void Gratings()
    {
        var lay = GratingLayout();
        var g = new Grating(_cfg).DrawGrating(_m, "Grating");     // 650 x 660 x 30, QTY 6

        double xFront = WallT;                                      // same as GratingSupports()
        double gW = 650, gL = 660;
        double xFlat = xFront + 675;                               // inner face of vertical flat (after 180° turn)

        var doc = NewDoc(g);
        foreach (var (start, len) in lay.segs)
        {
            double x = xFlat - gW;                                  // back edge against the flat
            double y = lay.y0 + start + (len - gL) / 2;             // centred between separators
            double z = lay.z0 + 40;                                 // on top of 75x40 box / cross supports
            AddCopy(doc, g, x, y, z);
        }
        Put("Grating", doc);
    }
    private void AngleSupports()
    {
        var lay = GratingLayout();
        var asp = new AngleSupport(_cfg);

        double xFront = WallT;
        double xc = xFront + 675 - 75 / 2.0;                // centred under long box (after 180° turn)
        double zTop = lay.z0;                                 // underside of Grating Support 1

        // joints in world Y (frame turned 180°), numbered from pump end like the client (1..5)
        var ys = lay.joints.Select(j => lay.y0 + lay.L - j).OrderBy(y => y).ToList();

        DesignDocument doc = null;
        for (int i = 0; i < ys.Count; i++)
        {
            double y = ys[i];
            double zFloor = TankPlateTopZ(y);                   // sloped base plate top here
            double angleH = zTop - zFloor - AngleSupport.PlateT;
            System.Diagnostics.Debug.WriteLine($"Angle support {i + 1}: Y={y:0}, angle {angleH:0}");   // client: 355, 360 …

            var part = asp.DrawAngleSupport(_m, $"AngleSupport_{i + 1}", angleH);
            doc ??= NewDoc(part);
            AddCopy(doc, part, xc, y, zFloor);
        }
        if (doc != null) Put("Angle_Support", doc);
    }

    private void PumpSump()
    {
        var part = new PumpSump(_cfg).DrawPumpSump(_m, "PumpSump");

        double tankOuterY = 0;                 // outer face of tank pump-end wall in OUR model   (CHECK - measure)
        double y0 = tankOuterY - 750;          // sump's 750 edge touches the tank wall

        var doc = NewDoc(part);
        AddCopy(doc, part, 0, y0, retZ);
        Put("Pump_Sump", doc);
    }
    private void PumpBase()
    {
        var part = new PumpBaseFrame(_cfg).DrawPumpBaseFrame(_m, "PumpBaseFrame");

        // ---- suction flange position (same numbers as PumpSump) ----
        double tankOuterY = 0, sumpY0 = tankOuterY - 750;
        double inner = 498, wall = 38, Dout = inner + 2 * wall;
        double xFl = (Dout - wall) - 312 + 600;              // flange face, world X (sump at X = 0)
        double yAx = sumpY0 + wall + 325;                     // pipe axis, world Y

        // ---- frame: 450 along X after 90° turn, centred on pipe axis, on floor ----   (CHECK gap / height)
        double gap = 50;                                       // flange face → start of frame
        double z = retZ;                                     // floor = tank bottom level

        var doc = NewDoc(part);
        AddCopy(doc, part, xFl + gap + 450, yAx - 130, z, rotZDeg: 90);
        Put("Pump_Base_Frame", doc);
    }

    // ---- add here ----
    private List<double> BasePlateCutoutGlobalY()
    {
        const double FullLen = 2318;
        const double MinPiece = 1;

        double W = _m.W;
        var ys = new List<double>();
        double yStart = 0;

        while (W - yStart > MinPiece)
        {
            double len = Math.Min(FullLen, W - yStart);

                foreach (double yc in BasePlate1Part.CutoutCentres(len))
                    ys.Add(bpY0 + yStart + len - yc);   // rotZ 180 -> local y flips

            yStart += len;
        }
        return ys;
    }
    private void BasePlateReturn()
    {
        var part = new BasePlateReturn(_cfg).DrawBasePlateReturn(_m, zBottom);

        double gap = 20, partD = 131 + 2 * 1.6;
        retX = _m.D + _m.D3 - WallT - gap;
        retZ = zBottom - 460;

        var doc = NewDoc(part);
        foreach (double y in BasePlateCutoutGlobalY())
            AddCopy(doc, part, retX, y + partD / 2, retZ, rotZDeg: 180);   // centred on cutout
        Put("BasePlate_Return", doc);
    }

        private static List<string> WrapNote(string note, int maxChars)
        {
            var lines = new List<string>();

            foreach (var part in note.Split('\n'))
            {
                string indent = new string(' ', part.Length - part.TrimStart().Length);
                var words = part.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
                var sb = new StringBuilder(indent);

                foreach (var w in words)
                {
                    if (sb.Length > indent.Length && sb.Length + 1 + w.Length > maxChars)
                    {
                        lines.Add(sb.ToString());
                        sb.Clear().Append(indent + " ");   // continuation indent
                    }
                    if (sb.Length > indent.Length && sb[sb.Length - 1] != ' ') sb.Append(' ');
                    sb.Append(w);
                }
                lines.Add(sb.ToString());
            }
            return lines;
        }
    }
}