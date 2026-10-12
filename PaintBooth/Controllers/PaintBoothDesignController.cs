using devDept.Eyeshot;
using devDept.Eyeshot.Entities;
using devDept.Eyeshot.Translators;
using devDept.Geometry;
using Microsoft.AspNetCore.Mvc;
using PaintBooth.Business;
using PaintBooth.Database;
using PaintBooth.Model;
using PaintBooth.Models;
using PaintBooth.Repositories.Interface;
using PaintBooth.Service;
using System.Drawing;
using System.IO.Compression;
using System.Runtime.Intrinsics.Arm;
using Xbim.Ifc2x3.GeometricModelResource;

namespace PaintBooth.Controllers
{
    public class PaintBoothDesignController(IPaintBoothRepository _repo, AppDbContext _context, IConfiguration _configuration, IExceptionRepository _Ex) : Controller
    {
        private int userId => HttpContext.Session.GetInt32("UserId") ?? 0;

        // ── GET /PaintBoothDesign/Index ───────────────────────────
        public IActionResult Index()
        {
            var list = _repo.GetAll();
            return View(list);
        }

        // ── GET /PaintBoothDesign/Create ──────────────────────────
        public IActionResult Create()
        {
            try
            {
                var model = new PaintBoothModel
                {
                    D1 = 1200,
                    D2 = 700,
                    D3 = 800,
                    W1 = 500,
                    W2 = 500,
                    H1 = 700,
                    H2 = 1000,
                    HingedDoorHeight = 2100,
                    VelocityofBlower = (double)0.5m,
                    VelocityofFilterFrame = (double)0.5m,
                    DoorHeight = 1180,
                    DoorWidth = 580,
                    ServiceDoorLocation = "RightSide",
                    BlowerOrientation = "90"
                };
                return View(model);
            }
            catch (Exception ex)
            {
                _Ex.SaveException("PaintBoothDesignController", "Create", ex.Message, userId);
                return RedirectToAction(nameof(Index));
            }
        }

        // ── GET /PaintBoothDesign/Edit?salesNo=ENQ-001 ────────────────
        public IActionResult Edit(string salesNo)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(salesNo))
                    return RedirectToAction(nameof(Index));

                var existing = _repo.GetBySalesNo(salesNo);
                int componentId = _repo.FetchComponentId(existing.EnquiryId);
                string PaintboothType = _repo.FetchPaintboothType(existing.EnquiryId);
                ComponentTable WatchGlass = _repo.GetWatchGlassDetails(componentId);
                var blowerDetails = _repo.GetBlowerDetails(existing.EnquiryId);
                var wcRows = _repo.GetWaterColumnDetails(existing.EnquiryId);
                var motor = _repo.GetMotorBySalesNo(salesNo); // add this repo method
                                                              // Serialize rows back to JSON for the hidden field
                var wcRowsJson = System.Text.Json.JsonSerializer.Serialize(
                    wcRows.Select(x => new WaterColumnRowDto
                    {
                        ItemNumber = x.ItemNumber,
                        Quantity = x.Quantity,
                        IsSelected = x.IsSelected
                    })
                );
                var ductRows = _repo.GetExhaustDuctingByEnquiry(existing.EnquiryId);

                var ductJson = System.Text.Json.JsonSerializer.Serialize(
                    ductRows.Select(x => new ExhaustDuctingModel
                    {
                        BendType = x.BendType,
                        BendDirection = x.BendDirection,
                        DuctLength = x.DuctLength,
                        DuctWidth = x.DuctWidth,
                        DuctHeight = x.DuctHeight,
                        DuctThickness = x.DuctThickness,
                        DuctNetWeight = x.DuctNetWeight ?? 0,
                        DuctGrossWeight = x.DuctGrossWeight ?? 0
                    })
                );


                Console.WriteLine($"WC Rows JSON: {wcRowsJson}");
                if (existing == null)
                    return RedirectToAction(nameof(Index));

                var model = new PaintBoothModel
                {
                    PaintBoothID = existing.PaintBoothID,
                    SalesNO = salesNo,
                    D1 = existing.D1,
                    D2 = existing.D2,
                    D3 = existing.D3,
                    W1 = existing.W1,
                    W2 = existing.W2,
                    W3 = existing.W3,
                    H1 = existing.H1,
                    H2 = existing.H2,
                    D = existing.D,
                    W = existing.W,
                    H = existing.H,
                    CrossSectionalArea = (double)existing.CrossSectionalAreaOfBlower,
                    VelocityofBlower = existing.VelocityofBlower,
                    NoOfBlowers=existing.NoOfBlowers,
                    CapacityofEachBlower= existing.CapacityPerBlower,
                    CapacityofBlower = (double)existing.CapacityofBlowerincubicm,
                    CapacityofBlowerinH = (double)existing.CapacityofBlowerinHr,
                    CapacityofBlowerRoundOf = (double)existing.RoundingCapacity,
                    VelocityofFilterFrame = (double)0.5m,
                    BetweenCenterDistance = existing.BetweenCenterDistance,
                    HingedDoorHeight = 2100,
                    DoorHeight = 1180,
                    DoorWidth = 580,
                    ServiceDoorLocation = "RightSide",
                    BlowerOrientation = "90",
                    ExhaustDuctingListJson = ductJson,
                    ExhaustWidth = ductRows.Any() ? (double)ductRows.First().DuctWidth : 0,
                    ExhaustDuctHeight = ductRows.Any() ? (double)ductRows.First().DuctHeight : 0,
                    ExhaustThickness = ductRows.Any() ? (double)ductRows.First().DuctThickness : 0,
                    Mountingtype = blowerDetails.MountingTypes,
                    Drivetype = blowerDetails.DriveTypes,
                    WaterColumn = (double?)blowerDetails?.WaterColumn ?? 0,
                    BlowerHpCalculation = (double?)blowerDetails?.BlowerHpCalculation ?? 0,
                    BlowerItemCode = blowerDetails?.BlowerItemCode ?? "",
                    WaterColumnRowsJson = wcRowsJson,
                    MotorTypes = motor?.MotorTypes ?? "",
                    MotorCatalogID = motor.MotorCatalogID,
                    chkWatchDoor = WatchGlass?.chkWatchDoor ?? false,
                    WatchGlassBottomDistance = (double)(WatchGlass?.WatchGlassBottomDistance ?? 0),
                    WatchglassWidth = (double)(WatchGlass?.WatchglassWidth ?? 0),
                    WatchglassHeight = (double)(WatchGlass?.WatchglassHeight ?? 0),
                    PaintBoothType = PaintboothType
                };
                return View("Create", model);
            }
            catch (Exception ex)
            {
                _Ex.SaveException("PaintBoothDesignController", "Edit", ex.Message, userId);
                return RedirectToAction(nameof(Index));
            }
        }

        // ── POST /PaintBoothDesign/Delete ─────────────────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Delete(string salesNo)
        {
            if (string.IsNullOrWhiteSpace(salesNo))
                return RedirectToAction(nameof(Index));

            int enquiryId = _repo.GetEnquiryID(salesNo);
            _repo.DeletePaintBooth(enquiryId);

            TempData["Success"] = $"Design for {salesNo} deleted successfully.";
            return RedirectToAction(nameof(Index));
        }
        #region Search 

        // ── AJAX: Enquiry code autocomplete ───────────────────────
        [HttpPost]
        public JsonResult SearchEnquiryCode(string enquiryCode)
        {
            try
            {
                var results = _repo.SearchEnquiryCodes(enquiryCode ?? "");
                return Json(new { success = true, results });
            }
            catch (Exception ex)
            {
                _Ex.SaveException("PaintBoothDesignController", "SearchEnquiryCode", ex.Message, userId);
                return Json(new { success = false, message = "Something went wrong. Please try again." });
            }
        }
        // ── AJAX: Search — loads component + setting data ─────────
        [HttpPost]
        public JsonResult Search(string enquiryCode)
        {
            try
            {
                var data = _repo.GetEnquiryData(enquiryCode);

                if (data == null)
                    return Json(new { error = "Enquiry not found." });

                return Json(new
                {
                    length = data.Length,
                    width = data.Width,
                    height = data.Height,
                    extractionChamberHeight = data.ExtractionChamberHeight,
                    motorTypes = data.MotorTypes,
                    panelWidth = data.PanelWidth,
                    panelHeight = data.PanelHeight,
                    subTypeOfDraftType = data.SubTypeOfDraftType,
                    category = data.Category
                });
            }
            catch (Exception ex)
            {
                _Ex.SaveException("PaintBoothDesignController", "Search", ex.Message, userId);
                return Json(new { success = false, message = "Something went wrong. Please try again." });
            }
        }
        [HttpGet]
        public IActionResult WaterColumnCalculations(string category = "")
        {
            try
            {
                var pressureDropData = _repo.GetAallWaterColumnDetails();
                // Items to exclude for Water wash paint booth
                var waterWashExclude = new[] { 1, 2, 8 };

                var filtered = pressureDropData
                    .Where(x => category == "Water wash paint booth"
                                ? !waterWashExclude.Contains(x.ItemNumber)
                                : true)
                    .Select(x => new PaintBooth.Models.PressureDrop
                    {
                        ItemNumber = x.ItemNumber,
                        Description = x.Description,
                        PressureDrop_mm = x.PressureDrop_mm,
                    }).ToList();

                return Json(new { pressureDropDetails = filtered });
            }
            catch (Exception ex)
            {
                _Ex.SaveException("PaintBoothDesignController", "WaterColumnCalculations", ex.Message, userId);
                return Json(new { success = false, message = "Something went wrong. Please try again." });
            }
        }
        [HttpGet]
        public JsonResult GetMotorDetails(PaintBoothModel model)
        {
            try
            {
                decimal BlowerHpCalculation = (decimal)model.BlowerHpCalculation;
                var records = _repo.GetMotorDetails(BlowerHpCalculation);
                return Json(records);
            }
            catch (Exception ex)
            {
                _Ex.SaveException("PaintBoothDesignController", "GetMotorDetails", ex.Message, userId);
                return Json(new { success = false, message = "Something went wrong. Please try again." });
            }
        }
        #endregion

        #region Variables declarations
        public static Dictionary<string, List<DesignDocument>> designdictionary = new();
        public static List<string> AllFilesPath = new List<string>();
        public static List<string> developmentpath = new List<string>();

        static PaintBoothModel paintBoothModel = new PaintBoothModel();


        List<string> filePaths = new List<string>();
        List<DesignDocument> standardFrameDrawing = new List<DesignDocument>();

        #endregion
        public IActionResult SavePaintBoothDetails(PaintBoothModel model)
        {
            try
            {
                // ── Fix 1: use model.SalesNO directly, not undefined enquiry variable ──
                string EnquiryNo = model.SalesNO;
                HttpContext.Session.SetString("SalesNo", EnquiryNo);
                HttpContext.Session.SetInt32("PaintboothDepth", (int)(double)model.D);
                HttpContext.Session.SetInt32("PaintboothHeight", (int)(double)model.H);
                HttpContext.Session.SetInt32("PaintboothWidth", (int)(double)model.W);
                HttpContext.Session.SetInt32("D3Panel", (int)(double)model.D3);

                int enquiryId = _repo.GetEnquiryID(EnquiryNo);
                int ComponentID = _repo.FetchComponentId(enquiryId);
                string CatagoryType = _repo.FetchCatagoryType(ComponentID);

                int PlenumHeight = 0; string filterType = "";
                var PaintBoothTypefromEnquiry = _repo.FetchPaintBoothType(model.SalesNO);
                if (PaintBoothTypefromEnquiry != "1")
                {
                    filterType = _repo.GetFilterType(enquiryId);
                }
                if (PaintBoothTypefromEnquiry == "3" || PaintBoothTypefromEnquiry == "4" || PaintBoothTypefromEnquiry == "5" || PaintBoothTypefromEnquiry == "8")
                {
                    PlenumHeight = int.Parse(_repo.FetchPlenumHeight(EnquiryNo));
                    HttpContext.Session.SetInt32("PlenumHeight", PlenumHeight);
                    model.PlenumHeight = PlenumHeight;
                }
                if (model.MotorCatalogID.HasValue)
                {
                    int selectedMotorId = model.MotorCatalogID.Value;
                }

                _repo.SaveMotorDeatils((int)enquiryId, (int)model.MotorCatalogID, model.MotorItemCode);
                // ── Save water column row details ──────────────────────────
                _repo.SaveWaterColumnDetails((int)enquiryId, model.WaterColumnRowsJson);
                _repo.SaveBlowerDetails((int)enquiryId, model);
                // ── Save exhaust ducting rows ──────────────────────────────
                _repo.SaveExhaustDucting(enquiryId, model.SalesNO, model, userId);

                decimal RatedOutputHP = _repo.FetchRatedOutputHP((int)model.MotorCatalogID);
                string MotorTypes = _repo.GetMotorTypes((int)model.MotorCatalogID);
                model.RatedOutputHP = RatedOutputHP;
                model.MotorTypes = MotorTypes;
                HttpContext.Session.SetString("DraftSubType", PaintBoothTypefromEnquiry);

                var paintBoothModel = _repo.GetSettingDetailsByCode(EnquiryNo);
                if (paintBoothModel.Settings.Count == 0)
                {
                    return NotFound("No settings found for the given SalesNo.");
                }
                // Assuming you want the first setting for the PaintBoothDesign
                //var setting = paintBoothModel.Settings.First();
                var paintBoothSetting = paintBoothModel.Settings.FirstOrDefault(x => x.SettingType == "Paintbooth");
                var extractionSetting = new SettingItemViewModel();

                if (PaintBoothTypefromEnquiry != "4" && PaintBoothTypefromEnquiry != "5")
                {
                    extractionSetting = paintBoothModel.Settings.FirstOrDefault(x => x.SettingType == "Extraction");
                }
                // Create and populate the PaintBoothDesign object
                PaintBoothDesignBusiness panel = new PaintBoothDesignBusiness(_context, _configuration)
                {
                    SheetThickness = (double)paintBoothSetting.SheetThickness, // Use setting values
                    SettingStandardBend1 = (double)paintBoothSetting.StandardBend1,
                    SettingStandardBend2 = (double)paintBoothSetting.StandardBend2,
                    PitchDistance = (double)paintBoothSetting.PitchDistance,
                    PanelWidth = (double)paintBoothSetting.PanelWidth,
                    PanelHeight = (double)paintBoothSetting.PanelHeight,
                    SlotDimention = paintBoothSetting.SlotDimensions,
                    SettingH = (double)paintBoothSetting.H,
                    SettingW = (double)paintBoothSetting.W,
                    SettingT = (double)paintBoothSetting.T,
                    Materials = paintBoothSetting.Materials,
                    Section = paintBoothSetting.Section,
                    D3 = model.D3,
                    EnquiryID = enquiryId,
                    LightTypes = paintBoothSetting.LightTypes,
                    LuxLevel = paintBoothSetting.LuxLevel,
                    Lumens = paintBoothSetting.Lumens,
                    ECSheetThickness = (double)extractionSetting.SheetThickness == null ? 0 : (double)extractionSetting.SheetThickness,
                    ECMaterials = extractionSetting.Materials == null ? "0" : extractionSetting.Materials,
                    ECSlotDimention = extractionSetting.SlotDimensions == null ? "" : extractionSetting.SlotDimensions,
                    ECStandardBend1 = (double)extractionSetting.StandardBend1 == null ? 0 : (double)extractionSetting.StandardBend1,
                    ECStandardBend2 = (double)extractionSetting.StandardBend2 == null ? 0 : (double)extractionSetting.StandardBend2,
                    ECPitchDistance = (double)extractionSetting.PitchDistance == null ? 0 : (double)extractionSetting.PitchDistance,
                    plenumHeight = PlenumHeight,
                    paintboothType = PaintBoothTypefromEnquiry
                };
                model.PitchDistance = (decimal)paintBoothSetting.PitchDistance;
                HttpContext.Session.SetInt32("settingPanelWidth", (int)paintBoothSetting.PanelWidth);
                HttpContext.Session.SetInt32("settingPanelHeight", (int)paintBoothSetting.PanelHeight);
                _repo.UpdateTubelightDetails(model, paintBoothSetting.LuxLevel, paintBoothSetting.Lumens);

                string selectedLocation = model.ServiceDoorLocation;

                // Check if the record already exists
                if (!_repo.Exists(enquiryId))
                {
                    int PaintID = _repo.SavePaintBooth(model, enquiryId);
                }
                else
                {
                    _repo.UpdateBetweenCenterDistance(enquiryId, (decimal?)model.BetweenCenterDistance, model);
                }
                int componentID = _repo.FetchComponentId((int)enquiryId);
                double ExtractionC_Height = int.Parse(_repo.FetchExtractionHeight(componentID));

                _repo.UpdateWatchGlassDetails(componentID, model);
                ComponentTable DoorType = null;
                DoorType = _repo.FetchDoorType(componentID);

                #region DoorModel Values Assigning
                DoorDimensionsModel doorModel = new();
                doorModel.doorType = DoorType.DoorSubType;
                doorModel.doorSubType = DoorType.TypeOfHingedDoor;
                doorModel.sideDoorLocation = DoorType.ComponentEntry;
                doorModel.doorWidth = (double)model.HingedDoorWidth;
                doorModel.doorHeight = (double)model.HingedDoorHeight;
                doorModel.TypeOfHinges = DoorType.TypeOfHinges;


                _repo.UpdateDoorDetails(componentID, model);
                #endregion

                #region All Panels methods
                // DELETE OLD PANEL DETAILS FIRST
                panel.DeleteOldPanelDetails((int)enquiryId);
                PaintBoothService service = new PaintBoothService(_context, _configuration);
                if (!DoorType.ComponentEntry.Contains("Side") || DoorType.ComponentEntry == "")
                {
                    service.LeftRightPanels(model, panel);
                }
                else
                {
                    model.PanelWidth = paintBoothSetting.PanelWidth;
                    model.PanelHeight = (double)paintBoothSetting.PanelHeight;
                    service.LeftRightPanelsForSideDoor(model, panel, doorModel, PaintBoothTypefromEnquiry);
                }
                if (PaintBoothTypefromEnquiry == "1")
                {
                    service.TopPanels(model, panel, PaintBoothTypefromEnquiry);
                }
                if (PaintBoothTypefromEnquiry != "4" && PaintBoothTypefromEnquiry != "5" && PaintBoothTypefromEnquiry != "6" && PaintBoothTypefromEnquiry != "7")
                {
                    service.D3Panels(model, panel, ExtractionC_Height, PaintBoothTypefromEnquiry);
                    service.CreateLoft(model, panel, ExtractionC_Height, PaintBoothTypefromEnquiry,model.NoOfBlowers);
                    if(CatagoryType != "Water wash paint booth")
                    {
                        service.FiltersAndBaffles(model, panel, ExtractionC_Height, PaintBoothTypefromEnquiry);
                    }
                    else
                    {
                        #region PQ Sections
                        PSection p = new(_configuration);

                        double baseFrameH = panel.Section == "Bend Section" ? panel.SettingH : 75;
                        double zBottom = panel.SheetThickness - baseFrameH;   // underside of base frame

                        DesignDocument pSectionDoc = p.DrawPSection(model, zBottom);

                        double qTotal = (double)model.D + (double)model.D3;
                        double qSegment = 2390;   // or (double)paintBoothSetting.PanelHeight if 2390 comes from Panel Settings

                        DesignDocument qSectionLeftDoc = p.DrawQSection(model, zBottom, qTotal, qSegment, false);
                        DesignDocument qSectionRightDoc = p.DrawQSection(model, zBottom, qTotal, qSegment, true);
                        DesignDocument BackqSectionDoc = p.DrawbackQSection(model, zBottom, qTotal);

                        service.designdictionary["PSection"] = new List<DesignDocument> { pSectionDoc };

                        service.designdictionary["QSection_Left"] = new List<DesignDocument> { qSectionLeftDoc };
                        service.designdictionary["QSection_Right"] = new List<DesignDocument> { qSectionRightDoc };
                        service.designdictionary["BackQSection"] = new List<DesignDocument> { BackqSectionDoc };
                        #endregion

                        #endregion

                        #region Base Plate Return
                        

                        BasePlateReturn bReturn = new(_configuration);
                        DesignDocument baseFrameReturn = bReturn.DrawBasePlateReturn(model, zBottom);

                        // ---- Position in booth ----
                        double W = (double)model.W;                 // booth width along Y (2500)
                        double wallT = 38;                              // tank wall
                        double gap = 20;                              // leg clearance from wall       (CHECK)
                        double partD = 131 + 2 * 1.6;                   // part depth 134.2

                        double retX = model.D + model.D3 - wallT - gap;  // vertical leg just inside tank end wall
                        double retZ = zBottom - 460;                     // tank floor                    (CHECK)
                                             // tank floor                    (CHECK)

                        double[] retY = { 800};     // 3 Nos, centred in 3 equal bays across W

                        var baseReturns = new List<DesignDocument>();
                        foreach (double y in retY)
                            baseReturns.Add(CopyMoved(baseFrameReturn, retX, y + partD / 2, retZ, rotZDeg: 180));

                        service.designdictionary["BasePlate_Return"] = baseReturns;

                        #endregion
                       
                        #region Base Plate 1
                        BasePlate1 b = new(_configuration);
                        DesignDocument baseFrame1 = b.DrawBasePlate1(model);

                        double bpDepth = 2318;                                // same as Depth in DrawBasePlate1
                        double bpY0 = 0;                                   // Y where the plate starts   (CHECK)
                        double T = 1.6;                                 // return top plate thickness
                        double slope = Math.Atan2(211 - 30, 682);           // 14.86°
                        double wallIn = model.D + model.D3 - wallT;          // inner face of tank end wall

                        double bpX = wallIn;                                  // lip end against wall
                        double bpZ = retZ + 368 - 30
                                   - (bpX - retX) * Math.Tan(slope)
                                   + T / Math.Cos(slope);                     // sits on return top flange

                        DesignDocument baseFrame1Placed = CopyMoved(baseFrame1, bpX, bpY0 + bpDepth, bpZ, rotZDeg: 180);

                        service.designdictionary["BasePlate_1"] = new List<DesignDocument> { baseFrame1Placed };
                        #endregion
                      
                        #region DistributionPlate1
                        DistributionPlate1 d = new(_configuration);
                        DesignDocument plate1 = d.DrawDistributionPlate(model, "DistributionPlate1", 2318, 5 + 1128);
                        DesignDocument plate2 = d.DrawDistributionPlate(model, "DistributionPlate2", 1678, 5 + 1130);
                        // ---- Base Plate 1: top corner at the end of the short 120 leg (world) ----
                        double bpA1 = Math.Atan2(211 - 30, 682);                      // 14.86°
                        double bpA2 = bpA1 - (Math.PI - 154 * Math.PI / 180.0);       // -11.14°
                        double legEndX = 682 + 120 * Math.Cos(bpA2);                    // local x of leg end (~799.7)
                        double legTopZ = 211 + 120 * Math.Sin(bpA2) + 25 / Math.Cos(bpA2); // flange top at leg end (~213.3)

                        double cornerX = bpX - legEndX;                                 // after Base Plate 1's 180° turn
                        double cornerZ = bpZ + legTopZ;

                        // ---- Hook top sits on that corner: up-leg outside the end face, flat on top ----
                        double tD = 1.6;
                        double dpX = cornerX - tD / 2 + d.HookTopX;
                        double dpZ = cornerZ + tD / 2 - d.HookTopZ;
                        double dpY = bpY0 + 2318;                                       // same Y span as Base Plate 1

                        DesignDocument plate1Placed = CopyMoved(plate1, dpX, dpY, dpZ, rotZDeg: 180);

                        service.designdictionary["Distribution_Plate1"] = new List<DesignDocument> { plate1Placed };
                        //service.designdictionary["DistributionPlate2"] = new List<DesignDocument> { plate2 };
                        #endregion

                        #region ReturnTroughBracket
                        ReturnTroughBracket r = new(_configuration);
                        DesignDocument rBracket = r.DrawReturnTroughBracket(model,2390);
                       
                        //service.designdictionary["ReturnTroughBracket"] = new List<DesignDocument> { rBracket };
                        #endregion

                        #region Deflector Gusset
                        var dg = new DeflectorGusset(_configuration);
                        DesignDocument DeflectorGusset_218 =   dg.DrawDeflectorGusset(model, "DeflectorGusset_218", +1);
                        DesignDocument DeflectorGusset_219 =   dg.DrawDeflectorGusset(model, "DeflectorGusset_219", -1);

                        //service.designdictionary["DeflectorGusset_218"] = new List<DesignDocument> { DeflectorGusset_218 };
                        //service.designdictionary["DeflectorGusset_219"] = new List<DesignDocument> { DeflectorGusset_219 };
                        #endregion

                        #region Eliminator 201
                        var el = new Eliminator(_configuration);
                        DesignDocument Eliminator1 = el.DrawEliminator(model, "Eliminator1", 1150);//1150 is Extrude length
                        DesignDocument Eliminator2 = el.DrawEliminator(model, "Eliminator2", 510);

                        //service.designdictionary["Eliminator1"] = new List<DesignDocument> { Eliminator1 };
                        //service.designdictionary["Eliminator2"] = new List<DesignDocument> { Eliminator2 };
                        #endregion

                        #region FrontPlate
                        var fp = new FrontPlate(_configuration);
                        DesignDocument frontPlate_203 = fp.DrawFrontPlate(model, "FrontPlate_203", 1158);
                        DesignDocument frontPlate_204 = fp.DrawFrontPlate(model, "FrontPlate_204", 518);

                        // ---- Position (from client RHS view) ----
                        double fpX = model.D;          // plate front face X  (scaled from RHS view ≈1020 from end wall)   (CHECK)
                        double fpZ = zBottom;           // plate bottom Z      (145 tank top -> entrainment bottom, minus 69) (CHECK)
                        double[] fpY = { 0, 1158/*, 2316*/ };    // Y start of each plate, QTY 3 Nos side by side                    (CHECK)

                        // all copies in ONE document, so the GA merge doesn't add its X offset
                        var frontPlates = new DesignDocument();
                        frontPlates.Units = frontPlate_203.Units;
                        foreach (double y in fpY)
                        {
                            foreach (Entity e in frontPlate_203.Entities)
                            {
                                Entity c = (Entity)e.Clone();
                                c.Translate(fpX, y, fpZ);
                                frontPlates.Entities.Add(c);
                            }
                        }
                        service.designdictionary["Front_Plate"] = new List<DesignDocument> { frontPlates };
                        //service.designdictionary["frontPlate_204"] = new List<DesignDocument> { frontPlate_204 };
                        #endregion

                        #region Pipe Support Bracket
                        var psb = new PipeSupportBracket(_configuration);
                        DesignDocument bracket = psb.DrawPipeSupportBracket(model, 300);

                        double brT = 3;
                        double brX = cornerX - 1.6 - brT / 2;     // inner face on Distribution Plate hook (1.6) outside base plate end
                        double brZ = cornerZ + 15;                // bend ~15 above base plate flange top        (CHECK)
                        double brLen = 300;
                        double[] brY = { 400/*, 1600, 2800*/ };        // start Y of each bracket, 3 Nos (one per pipe support)  (CHECK)

                        var brackets = new DesignDocument();
                        brackets.Units = bracket.Units;

                        foreach (double y in brY)
                        {
                            foreach (Entity e in bracket.Entities)
                            {
                                Entity c = (Entity)e.Clone();
                                c.Translate(brX, y, brZ);
                                brackets.Entities.Add(c);
                            }
                        }

                        service.designdictionary["PipeSupport_Bracket"] = new List<DesignDocument> { brackets };

                        #region Base Plate Pipe Support
                        var bps = new BasePlatePipeSupport(_configuration);
                        DesignDocument pipeSupport = bps.DrawBasePlatePipeSupport(model);
                        double psTopL = 300;
                        double ang = bps.PipeDeg * Math.PI / 180.0;
                        double ux = Math.Cos(ang), uz = Math.Sin(ang);           // up the slope in world (after 180° turn)
                        double nx = -Math.Sin(ang), nz = Math.Cos(ang);
                        //double off = 3.0 / 2 + 19.05 / 2;                        // bracket half-thk + pipe radius
                        double off = 3.0 / 2 + bps.PipeOD / 2;      // bracket half-thk + pipe radius
                        double axX = brX + nx * off;                             // world line the pipe axis must follow
                        double axZ = brZ + nz * off;

                        double psX = wallIn - 3;                                     // vertical leg outer face on wall
                        double wX = psX - bps.AxisX;                            // where local axis point lands in X
                        double wZ = axZ + (wX - axX) / ux * uz;                 // its Z on the world axis line
                        double psZ = wZ - bps.AxisZ;

                        var pipeSupports = new DesignDocument();
                        pipeSupports.Units = pipeSupport.Units;

                        foreach (double y in brY)                            // same Y as the 126° brackets
                        {
                            double psY = y + psTopL;                         // 180° turn: centres pipe on bracket (Y = y + 150)
                            foreach (Entity e in pipeSupport.Entities)
                            {
                                Entity c = (Entity)e.Clone();
                                c.Rotate(Math.PI, Vector3D.AxisZ, Point3D.Origin);
                                c.Translate(psX, psY, psZ);
                                pipeSupports.Entities.Add(c);
                            }
                        }

                        service.designdictionary["BasePlate_PipeSupport"] = new List<DesignDocument> { pipeSupports };
                        #endregion

                        #endregion

                        #region EntrainmentPlate 214
                        var ep = new EntrainmentPlate(_configuration);
                        DesignDocument EntrainmentPlate1 = ep.DrawEntrainmentPlate(model, "EntrainmentPlate1", 1152);
                        DesignDocument EntrainmentPlate2 = ep.DrawEntrainmentPlate(model, "EntrainmentPlate2", 518);

                        //service.designdictionary["frontPlate_214"] = new List<DesignDocument> { EntrainmentPlate1 };
                        //service.designdictionary["frontPlate_215"] = new List<DesignDocument> { EntrainmentPlate2 };
                        #endregion

                        #region BasePlateReturnThrough 212
                        var bprt = new BasePlateReturnTrough(_configuration);
                        double[] holeYs = { 127, 587 };
                        DesignDocument basePlateReturnThrough = bprt.DrawBasePlateReturnTrough(model, holeYs);
                        //service.designdictionary["basePlateReturnThrough_212"] = new List<DesignDocument> { basePlateReturnThrough };

                        #endregion

                    }
                    service.backPanels(model, panel, ExtractionC_Height, PaintBoothTypefromEnquiry);
                }


                // ── Top filter frames — Types 2, 6, 7, 9 have filters on top instead of solid panels ──
                // Type 2: Filters On Top or Door
                // Type 6: With Filters on Top & With Civil
                // Type 7: With Filters on Top & Without Civil
                // Type 9: Semi Down Draft With Filters on Top
                if (PaintBoothTypefromEnquiry != "1")
                {
                    if (filterType == "Ceiling Filters")
                    {
                        service.TopFilterFrames(model, panel);
                    }
                }
                if (PaintBoothTypefromEnquiry == "4")
                {
                    service.CivilTank(model, panel);
                }
                service.TopStructureFrame(model, panel);
                service.BaseStructureFrame(model, panel, PaintBoothTypefromEnquiry);
                double SameSpaceBetweenDoors = 0;
                if (PaintBoothTypefromEnquiry != "1" && PaintBoothTypefromEnquiry != "9" && PaintBoothTypefromEnquiry != "8")
                {
                    if (DoorType.ComponentEntry == "Side")
                    {
                        service.FrontPanels(model, panel, PaintBoothTypefromEnquiry);
                    }
                    else
                    {
                        SameSpaceBetweenDoors = service.FrontDoorsType(model, panel, doorModel);

                    }
                }

                if (PaintBoothTypefromEnquiry == "3" || PaintBoothTypefromEnquiry == "4" || PaintBoothTypefromEnquiry == "5" || PaintBoothTypefromEnquiry == "8")
                {
                    EnquiryMaster FASDoor = _repo.FetchMaintenanceDoorDetails((int)enquiryId);
                    service.FASPanelsFrontAndBack(model, panel, PlenumHeight, FASDoor);
                    service.FASPanelsRightAndLeft(model, panel, PlenumHeight, FASDoor);
                    service.FASPanelsForTop(model, panel, PlenumHeight);
                }
              

                #region WriteFile
                //string BaseFilePath = "C:/Bullows/Paintbooth_Drawing";
                string BaseFilePath = _configuration["FolderPathConfig:DrawingPath"].ToString();
                if (!Directory.Exists(BaseFilePath))
                {
                    Directory.CreateDirectory(BaseFilePath);
                }
                DesignDocument combinedDrawing = CombineAssemblies2(service.designdictionary, (double)paintBoothSetting.PanelHeight, (double)paintBoothSetting.PanelWidth, PaintBoothTypefromEnquiry, ExtractionC_Height, SameSpaceBetweenDoors, doorModel);


                string combinedDwgFilePath = BaseFilePath + "/GA3D.dwg";
                WriteAutodeskParams autoCombined = new WriteAutodeskParams(combinedDrawing);
                WriteAutodesk dwgWriterCombined = new WriteAutodesk(autoCombined, combinedDwgFilePath);
                dwgWriterCombined.DoWork();

                //making pdf file format 
                string pdfFilePath = BaseFilePath + "/GA3D.pdf";
                Write3DPdfParams pdf = new Write3DPdfParams(combinedDrawing);
                Write3DPDF pdf1 = new Write3DPDF(pdf, pdfFilePath);
                pdf1.DoWork();

                //STEP file format
                string StepFilePath = BaseFilePath + "/GA3D_STEP.step";

                WriteSTEP wastep = new WriteSTEP(combinedDrawing, StepFilePath);
                wastep.DoWork();

                #endregion

                #region Views
                //Added Views here
                DesignDocument designDocument = combinedDrawing;

                PaintBoothclass paintBoothClass = panel.detailsdrawing(designDocument, model, PaintBoothTypefromEnquiry);

                PaintBoothclass standardframe = panel.StandardFrameWithoutDrawing(2, EnquiryNo);

                string filepathforstandardframe = standardframe.lstpath;
                DesignDocument standardframedrawing = new DesignDocument();
                standardframedrawing = standardframe.drawing;
                WriteAutodeskParams framedw = new WriteAutodeskParams(standardframedrawing);
                WriteAutodesk dwgWriterframe = new WriteAutodesk(framedw, filepathforstandardframe);
                dwgWriterframe.DoWork();
                standardFrameDrawing.Add(standardframedrawing);
                #endregion

                AllFilesPath = new List<string>(filePaths)
                {
                    combinedDwgFilePath,
                    pdfFilePath,
                   //filepathforLoft,
                   //filepathforstandardframe,
                };
                AllFilesPath.Add(paintBoothClass.lstpath);
                // Store the values needed for FilterFrameCalculations in session
                HttpContext.Session.SetString("FilterArea", model.FilterArea.ToString());
                HttpContext.Session.SetString("FF_W", model.W.ToString());
                HttpContext.Session.SetString("FF_H", model.H.ToString());
                HttpContext.Session.SetString("FF_D", model.D.ToString());
                HttpContext.Session.SetInt32("FF_Lights", model.Lights);
                return RedirectToAction("FilterFrameCalculations");
            }
            catch (Exception ex)
            {
                _Ex.SaveException("PaintBoothDesignController", "SavePaintBoothDetails", ex.Message, userId);
                return Json(new { success = false, message = ex.Message });
            }
        }


        private DesignDocument CombineAssemblies2(Dictionary<string, List<DesignDocument>> designDocument, double panelheight, double panelWidth, string DraftSubType, double extractionCHeight, double SameSpaceBetweenDoors, DoorDimensionsModel doorModel)
        {
            try
            {
                int? settingPanelWidth = HttpContext.Session.GetInt32("settingPanelWidth");
                int? settingPanelHeight = HttpContext.Session.GetInt32("settingPanelHeight");
                double? PaintboothHeight = HttpContext.Session.GetInt32("PaintboothHeight");
                double? PaintboothWidth = HttpContext.Session.GetInt32("PaintboothWidth");
                double? PaintBoothDepth = HttpContext.Session.GetInt32("PaintboothDepth");
                double? D3Panel = HttpContext.Session.GetInt32("D3Panel");

                #region Layers
                var combinedDrawing = new DesignDocument();
                Layer mylayer = new Layer("bendlayer");
                Layer OuterDoorFrame = new Layer("OuterDoorFrame", Color.Gray);
                Layer OuterRectangleLayer = new Layer("OuterRectangleLayer", Color.BlueViolet);
                Layer DoorFrameLayer = new Layer("DoorFrame", Color.BurlyWood);
                Layer MetalSheetLayer = new Layer("MetalSheet", Color.FloralWhite);
                Layer MainDoorFrameLayer = new Layer("MainDoorFrame", Color.DarkMagenta);
                mylayer.Color = Color.FromArgb(165, 82, 165);
                combinedDrawing.Layers.Add(mylayer);
                combinedDrawing.Layers.Add(OuterDoorFrame);
                combinedDrawing.Layers.Add(OuterRectangleLayer);
                combinedDrawing.Layers.Add(DoorFrameLayer);
                combinedDrawing.Layers.Add(MetalSheetLayer);
                combinedDrawing.Layers.Add(MainDoorFrameLayer);
                combinedDrawing.Layers.Add("HingeBackPlate", Color.BurlyWood);
                combinedDrawing.Layers.Add("SlidingTrack", Color.DarkGray);
                combinedDrawing.Layers.Add(new Layer("CivilTank", Color.WhiteSmoke));
                combinedDrawing.Layers.Add(new Layer("ZigZag", Color.GreenYellow));
                combinedDrawing.Layers.Add(new Layer("Rectangle", Color.White));
                combinedDrawing.Layers.Add(new Layer("SmallRectangle", Color.Gray));
                combinedDrawing.Units = linearUnitsType.Millimeters;
                #endregion

                foreach (var kvp in designDocument)
                {
                    if (kvp.Key == "rightSideDrawings" || kvp.Key == "leftSideDrawings")
                    {
                        int panelcount = PaintBoothService.noOfPanelsforD;
                        double zOffeset = 0;
                        if (Math.Ceiling(PaintBoothService.totalPanelsforD) == PaintBoothService.noOfPanelsforD + 1)
                        {
                            if (DraftSubType == "7" || DraftSubType == "5" || DraftSubType == "4" || DraftSubType == "6")
                                panelcount += 3;
                            else
                                panelcount++;
                        }
                        else if (Math.Floor(PaintBoothService.totalPanelsforD) == PaintBoothService.noOfPanelsforD)
                        {
                            if (DraftSubType == "7" || DraftSubType == "5" || DraftSubType == "4" || DraftSubType == "6")
                                panelcount += 2;
                            else
                                panelcount++;
                        }
                        if (Math.Ceiling(PaintBoothService.totalPanelsforH) == PaintBoothService.noOfPanelsforH + 1)
                            zOffeset = panelheight;
                        else
                            zOffeset = panelheight;

                        AssemblyValueModel assembly = new()
                        {
                            combinedDrawing = combinedDrawing,
                            drawings = kvp.Value,
                            panelcount = panelcount,
                            xoffset = 0,
                            yOffset = 0,
                            Zoffset = zOffeset,
                            paintboothSide = "BottomRightLeftPanels"
                        };
                        AddDrawingsToAssembly(assembly, 0);
                    }
                    else if (kvp.Key == "TopAssembly")
                    {
                        int panelcount = PaintBoothService.noOfPanelsforD;
                        double Yoffset = 0;
                        if (Math.Floor(PaintBoothService.totalPanelsforD) == PaintBoothService.noOfPanelsforD + 1)
                            panelcount++;
                        if (Math.Ceiling(PaintBoothService.totalPanelsforW) == PaintBoothService.noOfPanelsforW + 1)
                        {
                            Yoffset = panelheight;
                            panelcount++;
                        }
                        else if (Math.Ceiling(PaintBoothService.totalPanelsforW) == PaintBoothService.noOfPanelsforW)
                            Yoffset = panelheight;

                        AssemblyValueModel assembly = new()
                        {
                            combinedDrawing = combinedDrawing,
                            drawings = kvp.Value,
                            panelcount = panelcount,
                            xoffset = DraftSubType == "5" || DraftSubType == "4" ? (double)D3Panel : 0,
                            yOffset = Yoffset,
                            Zoffset = 0,
                            paintboothSide = kvp.Key
                        };
                        AddDrawingsToAssembly(assembly, 0);
                    }
                    else if (kvp.Key == "FilterAssembly")
                    {
                        AssemblyValueModel assembly = new()
                        {
                            combinedDrawing = combinedDrawing,
                            drawings = kvp.Value,
                            panelcount = 1,
                            //xoffset = DraftSubType == "5" /*|| DraftSubType == "4"*/ ? (double)D3Panel : 0,
                            xoffset =  0,
                            yOffset = 0,
                            Zoffset = 0,
                            paintboothSide = "FilterAssembly"
                        };
                        AddDrawingsToAssembly(assembly, 0);
                    }
                    else if (kvp.Key == "rightSideDoorDrawings" || kvp.Key == "leftSideDoorDrawings")
                    {
                        #region Old code
                        //double zOffeset = 0;
                        //if (Math.Ceiling(PaintBoothService.totalPanelsforH) == PaintBoothService.noOfPanelsforH + 1)
                        //    zOffeset = panelheight;
                        //else
                        //    zOffeset = panelheight;

                        //AssemblyValueModel assembly = new()
                        //{
                        //    combinedDrawing = combinedDrawing,
                        //    drawings = kvp.Value,
                        //    panelcount = 0,
                        //    xoffset = 0,
                        //    yOffset = 0,
                        //    Zoffset = zOffeset,
                        //    paintboothSide = "rightSideDoorDrawings"
                        //};
                        //AddDrawingsToAssembly(assembly, doorModel.RemainingSpanceinDepth); 
                        #endregion


                        double zOffeset = panelheight;
                        int panelsPerRow = (int)PaintBoothService.noOfPanelsInSideDoor
                                         + (PaintBoothService.smallPanelsWidthDoors > 0 ? 1 : 0);
                        //double xAfterDoor = (double)PaintBoothDepth - PaintBoothService.RemainingSpanceinD;

                        AssemblyValueModel assembly = new()
                        {
                            combinedDrawing = combinedDrawing,
                            drawings = kvp.Value,
                            panelcount = panelsPerRow,
                            xoffset = 0,
                            yOffset = 0,   // not used — Y is baked into drawing via PanelLengthForSideDoors
                            Zoffset = zOffeset,
                            paintboothSide = "rightSideDoorDrawings"
                        };
                        AddDrawingsToAssembly(assembly, doorModel.RemainingSpanceinDepth);
                    }
                    else if (kvp.Key == "rightSideDoorDrawings2" || kvp.Key == "leftSideDoorDrawings2")
                    {
                        //double zOffeset = panelheight;
                        //int panelsPerRow = (int)PaintBoothService.noOfPanelsInSideDoor
                        //                 + (PaintBoothService.smallPanelsWidthDoors > 0 ? 1 : 0);

                        //// X start = past the component entry span
                        //double xAfterDoor = PaintBoothService.RemainingSpanceinD + doorModel.doorWidth;

                        //AssemblyValueModel assembly = new()
                        //{
                        //    combinedDrawing = combinedDrawing,
                        //    drawings = kvp.Value,
                        //    panelcount = panelsPerRow,
                        //    xoffset = xAfterDoor,   // ✅ offset past the door
                        //    yOffset = 0,
                        //    Zoffset = zOffeset,
                        //    paintboothSide = "rightSideDoorDrawings"  // reuse same tiling logic
                        //};
                        //AddDrawingsToAssembly(assembly, doorModel.RemainingSpanceinDepth);

                        double zOffeset = panelheight;
                        int panelsPerRow = (int)PaintBoothService.noOfPanelsInSideDoor
                                         + (PaintBoothService.smallPanelsWidthDoors > 0 ? 1 : 0);

                        // ✅ Must match exactly: RemainingSpanceinD + (model.Width + 500)
                        // which is the same as: model.D - RemainingSpanceinD
                        // double xAfterDoor = (double)PaintBoothDepth - PaintBoothService.RemainingSpanceinD;
                        double xAfterDoor = PaintBoothService.RemainingSpanceinD + doorModel.doorWidth;
                        AssemblyValueModel assembly = new()
                        {
                            combinedDrawing = combinedDrawing,
                            drawings = kvp.Value,
                            panelcount = panelsPerRow,
                            xoffset = xAfterDoor,   // ✅ exact mirror start point
                            yOffset = 0,
                            Zoffset = zOffeset,
                            paintboothSide = "rightSideDoorDrawings"
                        };
                        AddDrawingsToAssembly(assembly, doorModel.RemainingSpanceinDepth);
                    }

                    else if (kvp.Key == "frontDoorsDrawingList")
                    {
                        double smallPanelCount = PaintBoothService.smallPanelsWidthFrontDoors > 0 ? 1 : 0;
                        int panelcount = (int)(PaintBoothService.noOfPanelsInFrontDoor + smallPanelCount);
                        double zOffeset = 0;
                        if (Math.Ceiling(PaintBoothService.totalPanelsforH) == PaintBoothService.noOfPanelsforH + 1)
                            zOffeset = panelheight;
                        else
                            zOffeset = panelheight;

                        AssemblyValueModel assembly = new()
                        {
                            combinedDrawing = combinedDrawing,
                            drawings = kvp.Value,
                            panelcount = panelcount,
                            xoffset = 0,
                            yOffset = 0,
                            Zoffset = zOffeset,
                            paintboothSide = "frontDoorsDrawingList"
                        };
                        AddDrawingsToAssembly(assembly, SameSpaceBetweenDoors);
                    }
                    else if (kvp.Key == "D3PanelRightDrawings" || kvp.Key == "D3PanelLeftDrawings")
                    {
                        AssemblyValueModel assembly = new()
                        {
                            combinedDrawing = combinedDrawing,
                            drawings = kvp.Value,
                            panelcount = 0,
                            xoffset = (double)PaintBoothDepth,
                            yOffset = 0,
                            Zoffset = 0,
                            paintboothSide = "D3BothSidePanels"
                        };
                        AddDrawingsToAssembly(assembly, 0);
                    }
                    else if (kvp.Key == "D3PanelRightDrawingsBeforeD" || kvp.Key == "D3PanelLeftDrawingsBeforeD")
                    {
                        AssemblyValueModel assembly = new()
                        {
                            combinedDrawing = combinedDrawing,
                            drawings = kvp.Value,
                            panelcount = 0,
                            xoffset = (double)-D3Panel,
                            yOffset = 0,
                            Zoffset = 0,
                            paintboothSide = "D3BothSidePanelsBeforeD"
                        };
                        AddDrawingsToAssembly(assembly, 0);
                    }
                    else if (kvp.Key == "ComponantEntrySideDoor" || kvp.Key == "ComponantEntryFrontDoor")
                    {
                        AssemblyValueModel assembly = new()
                        {
                            combinedDrawing = combinedDrawing,
                            drawings = kvp.Value,
                            panelcount = 0,
                            xoffset = 0,
                            yOffset = 0,
                            Zoffset = 0,
                            paintboothSide = "ComponantEntrySideDoor"
                        };
                        AddDrawingsToAssembly(assembly, 0);
                    }
                    else if (kvp.Key == "panelsAboveEntryDoors")
                    {


                        AssemblyValueModel assembly = new()
                        {
                            combinedDrawing = combinedDrawing,
                            drawings = kvp.Value,
                            panelcount = 0,
                            xoffset = 0,
                            yOffset = 0,
                            Zoffset = 0,
                            paintboothSide = "panelsAboveEntryDoors"
                        };
                        AddDrawingsToAssembly(assembly, 0);

                    }
                    else if (kvp.Key == "backDrawingList" /*|| kvp.Key == "frontDrawingList"*/ || kvp.Key == "BackPanelsDownDraft")
                    {
                        int panelcount = PaintBoothService.noOfPanelsForBackSide;


                        int Yoffset = 0;
                        double zOffeset = 0;
                        if (Math.Ceiling(PaintBoothService.TotalBackPanels) == PaintBoothService.noOfPanelsForBackSide + 1)
                        {
                            Yoffset = (int)panelWidth;
                            panelcount++;
                        }
                        else if (Math.Ceiling(PaintBoothService.TotalBackPanels) == PaintBoothService.noOfPanelsForBackSide)
                            Yoffset = (int)panelWidth;

                        if (Math.Ceiling(PaintBoothService.totalPanelsforH) == PaintBoothService.noOfPanelsforH + 1)
                            zOffeset = panelheight;
                        else if (Math.Ceiling(PaintBoothService.totalPanelsforH) == PaintBoothService.noOfPanelsforH)
                            zOffeset = panelheight;

                        AssemblyValueModel assembly = new()
                        {
                            combinedDrawing = combinedDrawing,
                            drawings = kvp.Value,
                            panelcount = panelcount,
                            xoffset = 0,
                            yOffset = Yoffset,
                            //Zoffset = zOffeset,
                            Zoffset = DraftSubType == "8" || DraftSubType == "9" || DraftSubType == "1" || DraftSubType == "2" || DraftSubType == "3" ? 0 : zOffeset,
                            paintboothSide = "BackAndFrontPanels"
                        };
                        if (DraftSubType == "4" || DraftSubType == "5" || DraftSubType == "6" || DraftSubType == "7")
                            assembly.xoffset = (double)PaintBoothDepth;
                        AddDrawingsToAssembly(assembly, 0);
                    }
                    else if (kvp.Key == "frontDrawingList")
                    {
                        int panelcount = PaintBoothService.noOfPanelsForBackSide;


                        int Yoffset = 0;
                        double zOffeset = 0;
                        if (Math.Ceiling(PaintBoothService.TotalBackPanels) == PaintBoothService.noOfPanelsForBackSide + 1)
                        {
                            Yoffset = (int)panelWidth;
                            panelcount++;
                        }
                        else if (Math.Ceiling(PaintBoothService.TotalBackPanels) == PaintBoothService.noOfPanelsForBackSide)
                            Yoffset = (int)panelWidth;

                        if (Math.Ceiling(PaintBoothService.totalPanelsforH) == PaintBoothService.noOfPanelsforH + 1)
                            zOffeset = panelheight;
                        else if (Math.Ceiling(PaintBoothService.totalPanelsforH) == PaintBoothService.noOfPanelsforH)
                            zOffeset = panelheight;

                        AssemblyValueModel assembly = new()
                        {
                            combinedDrawing = combinedDrawing,
                            drawings = kvp.Value,
                            panelcount = panelcount,
                            xoffset = 0,
                            yOffset = Yoffset,
                            Zoffset = zOffeset,
                            paintboothSide = "FrontPanels"
                        };

                        AddDrawingsToAssembly(assembly, 0);
                    }

                    else if (kvp.Key == "panelsAboveFrontEntryDoors")
                    {
                        int panelcount = (int)PaintBoothService.noOfPanelsAboveCompEntryDoor;
                        int Yoffset = 0;
                        double zOffeset = doorModel.doorHeight;
                        if (Math.Ceiling(PaintBoothService.totalPanelsAboveCompEntryDoor) == PaintBoothService.noOfPanelsAboveCompEntryDoor + 1)
                        {
                            Yoffset = (int)panelWidth;
                            panelcount++;
                        }
                        else if (Math.Ceiling(PaintBoothService.totalPanelsAboveCompEntryDoor) == PaintBoothService.noOfPanelsAboveCompEntryDoor)
                            Yoffset = (int)panelWidth;

                        if (Math.Ceiling(PaintBoothService.totalPanelsforH) == PaintBoothService.noOfPanelsforH + 1)
                            zOffeset = doorModel.doorHeight;

                        AssemblyValueModel assembly = new()
                        {
                            combinedDrawing = combinedDrawing,
                            drawings = kvp.Value,
                            panelcount = panelcount,
                            xoffset = 0,
                            yOffset = Yoffset,
                            Zoffset = zOffeset,
                            paintboothSide = "panelsAboveFrontEntryDoors"
                        };
                        AddDrawingsToAssembly(assembly, doorModel.yOffsetForFrontDoors);
                    }
                    else if (kvp.Key == "backDrawingListBeforeExtractionC")//back side panels above Extraction chambers
                    {

                        // panels per width row = full panels + 1 small (if exists)
                        int panelsPerRow = PaintBoothService.noOfPanelsForBackSide
                                         + (PaintBoothService.smallPanelWidthForBackSide > 0 ? 1 : 0);

                        int Yoffset = 0;
                        double zOffeset = extractionCHeight;

                        if (Math.Ceiling(PaintBoothService.TotalBackPanels) == PaintBoothService.noOfPanelsForBackSide + 1)
                            Yoffset = (int)panelWidth;
                        else if (Math.Ceiling(PaintBoothService.TotalBackPanels) == PaintBoothService.noOfPanelsForBackSide)
                            Yoffset = (int)panelWidth;

                        AssemblyValueModel assembly = new()
                        {
                            combinedDrawing = combinedDrawing,
                            drawings = kvp.Value,
                            panelcount = panelsPerRow,
                            xoffset = (double)-D3Panel,
                            yOffset = 0,
                            Zoffset = zOffeset,
                            paintboothSide = "backSidePanelsAboveEC"
                        };
                        AddDrawingsToAssembly(assembly, 0);

                    }
                    else if (kvp.Key == "FilterDrawings")
                    {
                        int yOffset = 600;
                        int zOffset = PaintBoothDesignBusiness.selectedBaffleHeight;
                        int panelCount = PaintBoothDesignBusiness.bafflePanelCount;

                        AssemblyValueModel assembly = new()
                        {
                            combinedDrawing = combinedDrawing,
                            drawings = kvp.Value,
                            panelcount = panelCount,
                            xoffset = 0,
                            yOffset = yOffset,
                            Zoffset = zOffset,
                            paintboothSide = "BackAndFrontPanels"
                        };
                        AddDrawingsToAssembly(assembly, 0);
                    }

                    // ══════════════════════════════════════════════════════
                    // FAS BACK PANEL — fixed X=0, tiles Y, fixed Z=boothH
                    // ══════════════════════════════════════════════════════
                    else if (kvp.Key == "backDrawingListInTopSide")
                    {
                        int panelcount = PaintBoothService.noOfPanelsForBackSide;
                        int Yoffset = 0;
                        if (Math.Ceiling(PaintBoothService.TotalBackPanels) == PaintBoothService.noOfPanelsForBackSide + 1)
                        {
                            Yoffset = (int)panelWidth;
                            panelcount++;
                        }
                        else if (Math.Ceiling(PaintBoothService.TotalBackPanels) == PaintBoothService.noOfPanelsForBackSide)
                            Yoffset = (int)panelWidth;

                        AssemblyValueModel assembly = new()
                        {
                            combinedDrawing = combinedDrawing,
                            drawings = kvp.Value,
                            panelcount = panelcount,
                            xoffset = 0,                        // back face X=0
                            yOffset = Yoffset,
                            Zoffset = (double)PaintboothHeight, // ← top of booth
                            paintboothSide = "FASBackFrontPanels"      // ← new dedicated side
                        };
                        AddDrawingsToAssembly(assembly, 0);
                    }

                    // ══════════════════════════════════════════════════════
                    // FAS FRONT PANEL — fixed X=D, tiles Y, fixed Z=boothH
                    // ══════════════════════════════════════════════════════
                    else if (kvp.Key == "FrontDrawingListInTopSide")
                    {
                        int panelcount = PaintBoothService.noOfPanelsForBackSide;
                        int Yoffset = 0;
                        if (Math.Ceiling(PaintBoothService.TotalBackPanels) == PaintBoothService.noOfPanelsForBackSide + 1)
                        {
                            Yoffset = (int)panelWidth;
                            panelcount++;
                        }
                        else if (Math.Ceiling(PaintBoothService.TotalBackPanels) == PaintBoothService.noOfPanelsForBackSide)
                            Yoffset = (int)panelWidth;

                        AssemblyValueModel assembly = new()
                        {
                            combinedDrawing = combinedDrawing,
                            drawings = kvp.Value,
                            panelcount = panelcount,
                            xoffset = 0,                        // ← back to 0, panels already at correct X internally
                            yOffset = Yoffset,
                            Zoffset = (double)PaintboothHeight,
                            paintboothSide = "FASBackFrontPanels"
                        };
                        AddDrawingsToAssembly(assembly, 0);
                    }

                    // ══════════════════════════════════════════════════════
                    // FAS RIGHT PANEL — tiles X, fixed Y=0, fixed Z=boothH
                    // ══════════════════════════════════════════════════════
                    else if (kvp.Key == "RightPanelsListInTopSideForFAS")
                    {
                        int panelcount = PaintBoothService.noOfPanelsforD;
                        if (Math.Ceiling(PaintBoothService.totalPanelsforD) == PaintBoothService.noOfPanelsforD + 1)
                            panelcount++;
                        else if (Math.Floor(PaintBoothService.totalPanelsforD) == PaintBoothService.noOfPanelsforD)
                            panelcount++;

                        AssemblyValueModel assembly = new()
                        {
                            combinedDrawing = combinedDrawing,
                            drawings = kvp.Value,
                            panelcount = panelcount,
                            xoffset = 0,
                            yOffset = 0,                        // right side Y=0
                            Zoffset = (double)PaintboothHeight, // ← top of booth
                            paintboothSide = "FASRightLeftPanels"      // ← new dedicated side
                        };
                        AddDrawingsToAssembly(assembly, 0);
                    }

                    // ══════════════════════════════════════════════════════
                    // FAS LEFT PANEL — tiles X, fixed Y=W, fixed Z=boothH
                    // ══════════════════════════════════════════════════════
                    else if (kvp.Key == "LeftPanelsListInTopSideForFAS")
                    {
                        int panelcount = PaintBoothService.noOfPanelsforD;
                        if (Math.Ceiling(PaintBoothService.totalPanelsforD) == PaintBoothService.noOfPanelsforD + 1)
                            panelcount++;
                        else if (Math.Floor(PaintBoothService.totalPanelsforD) == PaintBoothService.noOfPanelsforD)
                            panelcount++;

                        AssemblyValueModel assembly = new()
                        {
                            combinedDrawing = combinedDrawing,
                            drawings = kvp.Value,
                            panelcount = panelcount,
                            xoffset = 0,
                            yOffset = 0,  // left side Y=W
                            Zoffset = (double)PaintboothHeight, // ← top of booth
                            paintboothSide = "FASRightLeftPanels"      // ← new dedicated side
                        };
                        AddDrawingsToAssembly(assembly, 0);
                    }

                    else if (kvp.Key == "TopAssemblyForFAS")
                    {
                        int panelcount = PaintBoothService.noOfPanelsforD;
                        double Yoffset = 0;
                        if (Math.Floor(PaintBoothService.totalPanelsforD) == PaintBoothService.noOfPanelsforD + 1)
                            panelcount++;
                        if (Math.Ceiling(PaintBoothService.totalPanelsforW) == PaintBoothService.noOfPanelsforW + 1)
                        {
                            Yoffset = panelheight;
                            panelcount++;
                        }
                        else if (Math.Ceiling(PaintBoothService.totalPanelsforW) == PaintBoothService.noOfPanelsforW)
                            Yoffset = panelheight;

                        AssemblyValueModel assembly = new()
                        {
                            combinedDrawing = combinedDrawing,
                            drawings = kvp.Value,
                            panelcount = panelcount,
                            xoffset = 0,
                            yOffset = Yoffset,
                            Zoffset = 0,
                            paintboothSide = "TopAssemblyForFAS"
                        };
                        AddDrawingsToAssembly(assembly, 0);
                    }
                    else if (kvp.Key == "FrontECBackDrawings")
                    {
                        int panelcount = PaintBoothService.noOfPanelsForBackSide;
                        int Yoffset = 0;
                        double zOffeset = 0;
                        if (Math.Ceiling(PaintBoothService.TotalBackPanels) == PaintBoothService.noOfPanelsForBackSide + 1)
                        { Yoffset = (int)panelWidth; panelcount++; }
                        else if (Math.Ceiling(PaintBoothService.TotalBackPanels) == PaintBoothService.noOfPanelsForBackSide)
                        { Yoffset = (int)panelWidth; }

                        if (Math.Ceiling(PaintBoothService.totalPanelsforH) == PaintBoothService.noOfPanelsforH + 1)
                            zOffeset = panelheight;
                        else
                            zOffeset = panelheight;

                        AssemblyValueModel assembly = new()
                        {
                            combinedDrawing = combinedDrawing,
                            drawings = kvp.Value,
                            panelcount = panelcount,
                            xoffset = -(double)D3Panel,
                            yOffset = Yoffset,
                            //Zoffset = zOffeset,
                            Zoffset = DraftSubType == "8" || DraftSubType == "9" ? 0 : zOffeset,
                            paintboothSide = "BackAndFrontPanels"
                        };
                        AddDrawingsToAssembly(assembly, 0);
                    }
                    else if (kvp.Key == "FrontECInnerDrawings")
                    {
                        //    int panelcount = PaintBoothService.noOfPanelsForBackSide;
                        //    int Yoffset = 0;
                        //    double zOffeset = extractionCHeight;
                        //    if (Math.Ceiling(PaintBoothService.TotalBackPanels) == PaintBoothService.noOfPanelsForBackSide + 1)
                        //    {
                        //        Yoffset = (int)panelWidth;
                        //        panelcount++;
                        //    }
                        //    else if (Math.Ceiling(PaintBoothService.TotalBackPanels) == PaintBoothService.noOfPanelsForBackSide)
                        //        Yoffset = (int)panelWidth;

                        //    AssemblyValueModel assembly = new()
                        //    {
                        //        combinedDrawing = combinedDrawing,
                        //        drawings = kvp.Value,
                        //        panelcount = panelcount,
                        //        xoffset = (double)D3Panel,
                        //        yOffset = Yoffset,
                        //        Zoffset = zOffeset,
                        //        paintboothSide = "backDrawingListInTopSide"
                        //    };
                        //    AddDrawingsToAssembly(assembly, 0);
                        int panelsPerRow = PaintBoothService.noOfPanelsForBackSide
                                             + (PaintBoothService.smallPanelWidthForBackSide > 0 ? 1 : 0);

                        int Yoffset = 0;
                        double zOffeset = extractionCHeight;

                        if (Math.Ceiling(PaintBoothService.TotalBackPanels) == PaintBoothService.noOfPanelsForBackSide + 1)
                            Yoffset = (int)panelWidth;
                        else if (Math.Ceiling(PaintBoothService.TotalBackPanels) == PaintBoothService.noOfPanelsForBackSide)
                            Yoffset = (int)panelWidth;

                        AssemblyValueModel assembly = new()
                        {
                            combinedDrawing = combinedDrawing,
                            drawings = kvp.Value,
                            panelcount = panelsPerRow,        // ✅ correct row width
                            xoffset = (double)D3Panel,
                            yOffset = 0,
                            Zoffset = zOffeset,
                            paintboothSide = "backSidePanelsAboveEC"
                        };
                        AddDrawingsToAssembly(assembly, 0);

                    }
                    else if (kvp.Key == "FrontFilterDrawings")
                    {
                        int yOffset = 600;
                        int zOffset = PaintBoothDesignBusiness.selectedBaffleHeight;
                        int panelCount = PaintBoothDesignBusiness.bafflePanelCount;

                        AssemblyValueModel assembly = new()
                        {
                            combinedDrawing = combinedDrawing,
                            drawings = kvp.Value,
                            panelcount = panelCount,
                            xoffset = -(double)D3Panel,
                            yOffset = yOffset,
                            Zoffset = zOffset,
                            paintboothSide = "BackAndFrontPanels"
                        };
                        AddDrawingsToAssembly(assembly, 0);
                    }
                    else
                    {
                        AssemblyValueModel assembly = new()
                        {
                            combinedDrawing = combinedDrawing,
                            drawings = kvp.Value,
                            panelcount = -1,
                            xoffset = 0,
                            yOffset = 0,
                            Zoffset = 0
                        };
                        AddDrawingsToAssembly(assembly, 0);
                    }
                }
                return combinedDrawing;
            }
            catch (Exception ex)
            {
                _Ex.SaveException("PaintBoothDesignController", "CombineAssemblies2", ex.Message, userId);
                throw;
            }
        }

        #region Assembly calculations
        private void AddDrawingsToAssembly(AssemblyValueModel model, double DoorWidthForHinge)
        {
            try
            {
                if (!model.combinedDrawing.Blocks.Any(b => b.Name.Contains("Hinge")))
                {
                    foreach (var d in model.drawings)
                    {
                        foreach (var blk in d.Blocks)
                        {
                            if (!model.combinedDrawing.Blocks.Contains(blk.Name))
                                model.combinedDrawing.Blocks.Add(blk);
                        }
                    }
                }
                double x = 0;
                int i = 0, j = 1;
                double z = 0;
                double y = 0;
                double? PaintboothWidth = HttpContext.Session.GetInt32("PaintboothWidth");
                double? D3Panel = HttpContext.Session.GetInt32("D3Panel");
                double? PaintBoothDepth = HttpContext.Session.GetInt32("PaintboothDepth");

                foreach (var drawing in model.drawings)
                {
                    string uniqueBlockName = "Block_" + Guid.NewGuid();

                    if (model.paintboothSide == "BottomRightLeftPanels")
                    {
                        if (i == model.panelcount * j)
                        {
                            x = 0;
                            z = model.Zoffset * j;
                            j++;
                        }
                    }
                    else if (model.paintboothSide == "D3BothSidePanels")
                    {
                        x = model.xoffset;
                    }
                    else if (model.paintboothSide == "D3BothSidePanelsBeforeD")
                    {
                        x = model.xoffset;
                    }
                    else if (model.paintboothSide == "frontDoorsDrawingList")
                    {
                        x = 0;
                        if (i % 2 == 0)
                        {
                            y = 0;
                            if (i > 0)
                                z += model.Zoffset;
                        }
                        else
                            y = (double)PaintboothWidth - DoorWidthForHinge;
                    }
                    else if (model.paintboothSide == "rightSideDoorDrawings")
                    {


                        double fullPanelWidth = HttpContext.Session.GetInt32("settingPanelWidth") ?? 0;
                        double smallWidth = PaintBoothService.smallPanelsWidthDoors;
                        bool isAfterDoor = model.xoffset > 0;  // ✅ detect if this is the mirrored span

                        int posInRow = (model.panelcount > 0) ? (i % model.panelcount) : i;
                        int rowIndex = (model.panelcount > 0) ? (i / model.panelcount) : 0;

                        y = 0;
                        z = model.Zoffset * rowIndex;

                        if (isAfterDoor && smallWidth > 0)
                        {
                            // ✅ First slot is small panel, rest are full width
                            if (posInRow == 0)
                                x = model.xoffset;                                        // small panel starts at xoffset
                            else
                                x = model.xoffset + smallWidth + (fullPanelWidth * (posInRow - 1)); // full panels after small
                        }
                        else
                        {
                            // ✅ Before door — all full width (original behaviour)
                            x = model.xoffset + (fullPanelWidth * posInRow);
                        }
                    }
                    else if (model.paintboothSide == "TopAssembly")
                    {
                        if (i == 0)
                            x = model.xoffset;
                        if (i == model.panelcount * j)
                        {
                            x = model.xoffset;
                            y = model.yOffset * j;
                            j++;
                        }
                    }
                    else if (model.paintboothSide == "TopAssemblyForFAS")
                    {
                        if (i == 0)
                            x = model.xoffset;
                        if (i == model.panelcount * j)
                        {
                            x = model.xoffset;
                            y = model.yOffset * j;
                            j++;
                        }
                    }
                    else if (model.paintboothSide == "FilterAssembly")
                    {
                        if (i == 0)
                            x = model.xoffset;
                        if (i == model.panelcount * j)
                        {
                            x = model.xoffset;
                            y = 710.0 * j;
                            j++;
                        }
                    }
                    else if (model.paintboothSide == "BackAndFrontPanels")
                    {
                        if (i == 0 || i == (model.panelcount * j) + 1)
                            y = 0;
                        else
                            y += model.yOffset;

                        if (i == model.panelcount * j)
                        {
                            z = model.Zoffset * j;
                            y = 0;
                            j++;
                        }
                        x = 0;
                    }
                    else if (model.paintboothSide == "FrontPanels")
                    {
                        if (i == 0 || i == (model.panelcount * j) + 1)
                            y = 0;
                        else
                            y += model.yOffset;

                        if (i == model.panelcount * j)
                        {
                            z = model.Zoffset * j;
                            y = 0;
                            j++;
                        }
                        x = 0;
                    }
                    // ── NEW: FAS Back/Front panels ────────────────────────
                    // X fixed, Z fixed at boothHeight, Y tiles across W
                    else if (model.paintboothSide == "FASBackFrontPanels")
                    {
                        x = model.xoffset;          // fixed — 0 for back, D for front
                        z = model.Zoffset;          // fixed — always boothHeight, never * j

                        if (i == 0)
                            y = 0;
                        else if (i == model.panelcount * j)
                        {
                            y = 0;
                            j++;
                        }
                        else
                            y += model.yOffset;
                    }

                    // ── NEW: FAS Right/Left panels ────────────────────────
                    // Y fixed, Z fixed at boothHeight, X tiles via x += drawingWidth
                    else if (model.paintboothSide == "FASRightLeftPanels")
                    {
                        y = model.yOffset;          // fixed — 0 for right, W for left
                        z = model.Zoffset;          // fixed — always boothHeight, never * j

                        if (i == 0)
                            x = model.xoffset;      // start X at xoffset (0)
                                                    // x accumulates via x += drawingWidth at bottom of loop
                    }

                    else if (model.paintboothSide == "panelsAboveFrontEntryDoors")
                    {
                        if (i == 0 || i == (model.panelcount * j) + 1)
                        {
                            y = DoorWidthForHinge;
                            z = model.Zoffset;
                        }
                        else
                            y += model.yOffset;

                        if (i == model.panelcount * j)
                        {
                            z = model.Zoffset * j;
                            y = 0;
                            j++;
                        }
                        x = 0;
                    }
                    else if (model.paintboothSide == "FrontDrawingListInTopSide")
                    {
                        x = model.xoffset;
                        y = model.yOffset * i;
                        z = model.Zoffset;
                    }
                    else if (model.paintboothSide == "backDrawingListInTopSide")
                    {
                        x = model.xoffset;
                        y = model.yOffset * i;
                        z = model.Zoffset;
                    }
                    else if (model.paintboothSide == "backSidePanelsAboveEC")
                    {

                        double yStep = HttpContext.Session.GetInt32("settingPanelWidth") ?? 0;
                        double zStep = HttpContext.Session.GetInt32("settingPanelHeight") ?? 0;

                        x = model.xoffset;

                        int posInRow = (model.panelcount > 0) ? (i % model.panelcount) : i;
                        int rowIndex = (model.panelcount > 0) ? (i / model.panelcount) : 0;

                        y = model.yOffset + (yStep * posInRow);   // ✅ tiles across W
                        z = model.Zoffset + (zStep * rowIndex);   // ✅ steps up per row
                    }
                    else if (model.paintboothSide == "TopRightLeftPanelsforFAS" && i == 0)
                    {
                        x = model.xoffset;
                        z = model.Zoffset;
                    }
                    else if (model.yOffset != 0 && model.Zoffset == -1)
                    {
                        y = model.yOffset * i;
                        x = 0;
                    }
                    else if (model.paintboothSide == "ComponantEntrySideDoor")
                    {
                        x = y = z = 0;
                    }
                    else if (model.paintboothSide == "panelsAboveEntryDoors")
                    {
                        x = y = z = 0;

                        //double xStep = HttpContext.Session.GetInt32("settingPanelWidth") ?? 0;
                        //double zStep = HttpContext.Session.GetInt32("settingPanelHeight") ?? 0;

                        //y = model.yOffset;   // fixed Y — 0 for right side, W for left side

                        //int posInRow = (model.panelcount > 0) ? (i % model.panelcount) : i;
                        //int rowIndex = (model.panelcount > 0) ? (i / model.panelcount) : 0;

                        //x = 0;                        // ✅ tiles across X (depth)
                        //z = model.Zoffset + (zStep * rowIndex);      // ✅ steps up per row

                    }

                    Block blk = new Block(uniqueBlockName);
                    blk.Entities.AddRange(drawing.Entities);
                    model.combinedDrawing.Blocks.Add(blk);

                    double drawingWidth = CalculateDrawingWidth(drawing);
                    BlockReference blkReference = new BlockReference(x, y, z, blk.Name, 0);
                    model.combinedDrawing.Entities.Add(blkReference);

                    x += drawingWidth;
                    i++;
                }
            }
            catch (Exception ex)
            {
                throw;
            }
        }
        private double CalculateDrawingWidth(DesignDocument drawing)
        {
            double minX = double.MaxValue;
            double maxX = double.MinValue;

            foreach (var entity in drawing.Entities)
            {
                if (entity.BoxMin.X < minX) minX = entity.BoxMin.X;
                else if (entity.BoxMax.X > maxX) maxX = entity.BoxMax.X;

            }

            return maxX - minX;
        }
        #endregion

        public IActionResult FilterFrameCalculations()
        {

            try
            {
                var model = new PaintBoothModel
                {
                    FilterArea = double.TryParse(
                                 HttpContext.Session.GetString("FilterArea"),
                                 out var fa) ? fa : 0,
                    W = double.TryParse(
                                 HttpContext.Session.GetString("FF_W"),
                                 out var w) ? w : 0,
                    H = double.TryParse(
                                 HttpContext.Session.GetString("FF_H"),
                                 out var h) ? h : 0,
                    D = double.TryParse(
                                 HttpContext.Session.GetString("FF_D"),
                                 out var d) ? d : 0,
                    Lights = HttpContext.Session.GetInt32("FF_Lights") ?? 0
                };

                //ViewBag.ActivePage = "PaintboothDesign";
                return View(model);
            }
            catch (Exception ex)
            {
                _Ex.SaveException("PaintBoothDesignController", "FilterFrameCalculations", ex.Message, userId);
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost]

        public IActionResult FilterFrameBox(PaintBoothModel model)
        {
            string tempFolderPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            try
            {
                int? settingPanelWidth = HttpContext.Session.GetInt32("settingPanelWidth");
                int? settingPanelHeight = HttpContext.Session.GetInt32("settingPanelHeight");

                if (settingPanelWidth == null || settingPanelHeight == null)
                    return BadRequest("Session expired. Please go back and search again.");

                double W = (double)settingPanelHeight.Value;
                double H = model.H;
                double FilterAreaValue = model.FilterArea;
                double filterBaseArea = W * H;

                if (filterBaseArea <= FilterAreaValue)
                    return RedirectToAction("Index");

                string salesNo = HttpContext.Session.GetString("SalesNo") ?? "Drawing";
                string zipFileName = $"{salesNo}.zip";

                Directory.CreateDirectory(tempFolderPath);
                string zipFilePath = Path.Combine(tempFolderPath, zipFileName);

                using (var zip = ZipFile.Open(zipFilePath, ZipArchiveMode.Create))
                {
                    foreach (var filePath in AllFilesPath)
                    {
                        if (string.IsNullOrWhiteSpace(filePath)) continue;
                        if (!System.IO.File.Exists(filePath)) continue;
                        zip.CreateEntryFromFile(filePath, Path.GetFileName(filePath));
                    }
                }

                byte[] zipBytes = System.IO.File.ReadAllBytes(zipFilePath);
                System.IO.File.Delete(zipFilePath);

                return File(zipBytes, "application/zip", zipFileName);
            }
            catch (Exception ex)
            {
                _Ex.SaveException("PaintBoothDesignController", "FilterFrameBox", ex.Message, userId);
                return RedirectToAction(nameof(Index));
            }
            finally
            {
                if (Directory.Exists(tempFolderPath))
                    Directory.Delete(tempFolderPath, recursive: true);
            }
        }

        private static DesignDocument CopyMoved(DesignDocument src, double dx, double dy, double dz,
                                        double rotZDeg = 0)
        {
            var dst = new DesignDocument();
            dst.Units = src.Units;

            foreach (Entity e in src.Entities)
            {
                Entity c = (Entity)e.Clone();
                if (rotZDeg != 0)
                    c.Rotate(rotZDeg * Math.PI / 180.0, Vector3D.AxisZ, Point3D.Origin);  // turn first, about part origin
                c.Translate(dx, dy, dz);                                                   // then move into the booth
                dst.Entities.Add(c);                                                       // keeps BurlyWood / Gray
            }
            return dst;
        }
    }

    public class AssemblyValueModel
    {
        public DesignDocument combinedDrawing { get; set; }
        public List<DesignDocument> drawings { get; set; }
        public double xoffset { get; set; }
        public double yOffset { get; set; }
        public double Zoffset { get; set; }
        public int panelcount { get; set; }
        public string paintboothSide { get; set; }

    }

}
