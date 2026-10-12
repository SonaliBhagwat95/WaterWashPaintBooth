

using ClosedXML.Excel;
using DocumentFormat.OpenXml.Office.CustomUI;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PaintBooth.Database;
using PaintBooth.Model;
using PaintBooth.Models;
using PaintBooth.Models.ERP;
using PaintBooth.Repositories.Interface;
using PaintBooth.Service;

namespace PaintBooth.Controllers
{
    [Authorize]
    public class CostingController(ICostingRepository _repo, IExceptionRepository _Ex,FrappeOpportunityService _service) : Controller
    {
        private int userId => HttpContext.Session.GetInt32("UserId") ?? 0;
        #region Raw materials Costing

        // ── GET /Costing/Index ────────────────────────────────────
        // Shows grid of all enquiries that have costing data saved
        public IActionResult Index()
        {
            //ViewBag.ActivePage = "Raw Material Costing";
            var list = _repo.GetAll();
            return View(list);
        }

        // ── GET /Costing/Create ───────────────────────────────────
        // Opens a blank costing form — user searches enquiry code
        public IActionResult Create()
        {
            ViewBag.ActivePage = "Raw Material Costing";
            return View(new CostingModel());
        }

        // ── GET /Costing/Edit?enquiryCode=ENQ-001 ─────────────────
        // Opens costing form pre-filled with existing data
        public IActionResult Edit(string enquiryCode)
        {
            var userId = HttpContext.Session.GetInt32("UserId") ?? 0;
            try
            {
                if (string.IsNullOrWhiteSpace(enquiryCode))
                    return RedirectToAction(nameof(Index));

                ViewBag.ActivePage = "Raw Material Costing";

                var model = _repo.GetByEnquiryCode(enquiryCode);
                if (model == null) return NotFound();
                // ── Load additional RM rows ──────────────────────────
                var enquiryId = _repo.GetEnquiryIdByCode(enquiryCode);
                model.AdditionalRMList = _repo.GetAdditionalRM(enquiryId);
                model.AddRMWeight = (double)model.AdditionalRMList.Sum(x => x.TotalWeight);
                model.AddRMCost = (double)model.AdditionalRMList.Sum(x => x.Amount);

                // Store in session so Save and Export know which enquiry
                HttpContext.Session.SetString("EnquiryCode", enquiryCode);

                return View("Create", model);
            }
            catch (Exception ex)
            {
                _Ex.SaveException("CostingController", "Edit", ex.Message, userId);
                return RedirectToAction(nameof(Index));
            }
        }

        // ── POST /Costing/SaveCosting ─────────────────────────────
        // Saves TotalCostOfPanels / Frames / RM to EnquiryMaster
        // Then redirects back to Index with success message
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult SaveCosting(CostingModel model)
        {

            try
            {
                string enquiryCode = HttpContext.Session.GetString("EnquiryCode");

                if (string.IsNullOrWhiteSpace(enquiryCode))
                {
                    TempData["Error"] = "Session expired — please search the enquiry again.";
                    return View("Create", model);
                }

                _repo.SaveCostOFFrame(enquiryCode, model);
                // ── Save additional RM rows ──────────────────────────
                var enquiryId = _repo.GetEnquiryIdByCode(enquiryCode);
                _repo.DeleteAdditionalRM(enquiryId);

                if (model.AdditionalRMList != null && model.AdditionalRMList.Any())
                {
                    var rows = model.AdditionalRMList.Select(item => new CostingAdditionalRM
                    {
                        EnquiryId = enquiryId,
                        Description = item.Description,
                        ItemCode = item.ItemCode,        // ← NEW

                        Qty = item.Qty,
                        UnitWeight = item.UnitWeight,
                        TotalWeight = item.TotalWeight,
                        Rate = item.Rate,
                        Amount = item.Amount,
                        CreatedAt = DateTime.Now
                    }).ToList();

                    _repo.SaveAdditionalRM(rows);
                }

                // Store RM values in session for ExportFISheet
                HttpContext.Session.SetString("RM_WeightPanels", model.RMWeightOfPanels.ToString());
                HttpContext.Session.SetString("RM_PricePanels", model.TotalPriceofPanels.ToString());
                HttpContext.Session.SetString("RM_WeightFrame", model.TotalWeightOfFrame.ToString());
                HttpContext.Session.SetString("RM_PriceFrame", model.TotalPriceofFrame.ToString());
                HttpContext.Session.SetString("RM_TotalCost", model.TotalCostOfRawMaterials.ToString());
                HttpContext.Session.SetString("RM_AddWeight", model.AddRMWeight.ToString());
                HttpContext.Session.SetString("RM_AddCost", model.AddRMCost.ToString());


                TempData["Success"] =
                    $"Costing for {enquiryCode} saved successfully.";

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _Ex.SaveException("CostingController", "SaveCosting", ex.Message, userId);
                TempData["Error"] = "Something went wrong: " + ex.Message;
                return View("Create", model);
            }
        }

        // ── POST /Costing/Delete ──────────────────────────────────
        // Soft-delete — resets cost fields to 0 on EnquiryMaster
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Delete(string enquiryCode)
        {
            if (string.IsNullOrWhiteSpace(enquiryCode))
                return RedirectToAction(nameof(Index));

            // Reset costs to zero (soft delete approach — enquiry stays)
            _repo.SaveCostOFFrame(enquiryCode, new CostingModel
            {
                TotalPriceofPanels = 0,
                TotalPriceofFrame = 0,
                TotalCostOfRawMaterials = 0
            });

            TempData["Success"] = $"Costing for {enquiryCode} has been removed.";
            return RedirectToAction(nameof(Index));
        }

        // ─────────────────────────────────────────────────────────
        // AJAX ENDPOINTS
        // ─────────────────────────────────────────────────────────

        // ── POST: Enquiry code autocomplete ──────────────────────
        [HttpPost]
        public JsonResult SearchEnquiryCode(string enquiryCode)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            try
            {
                var results = _repo.SearchEnquiryCodes(enquiryCode ?? "");

            if (results == null || !results.Any())
                return Json(new { success = false, message = "No enquiry codes found." });

            return Json(new { success = true, results });
            }
            catch (Exception ex)
            {
                _Ex.SaveException("CostingController", "SearchEnquiryCode", ex.Message, (int)userId);
                return Json(new { success = false, message = "Something went wrong. Please try again." });
            }
        }

        // ── POST: Load full costing data when enquiry is searched ─
        //[HttpPost]
        //public IActionResult SearchByEnquiryID(string EnquiryCode)
        //{
        //    try
        //    {
        //        if (string.IsNullOrWhiteSpace(EnquiryCode))
        //            return View("Create", new CostingModel());

        //        var model = _repo.GetByEnquiryCode(EnquiryCode);

        //        // ── Load additional RM rows ──────────────────────────
        //        var enquiryId = _repo.GetEnquiryIdByCode(EnquiryCode);
        //        model.AdditionalRMList = _repo.GetAdditionalRM(enquiryId);
        //        model.AddRMWeight = (double)model.AdditionalRMList.Sum(x => x.TotalWeight);
        //        model.AddRMCost = (double)model.AdditionalRMList.Sum(x => x.Amount);
        //        HttpContext.Session.SetString("EnquiryCode", EnquiryCode);

        //        ViewBag.ActivePage = "Raw Material Costing";
        //        return View("Create", model);
        //    }
        //    catch (Exception ex)
        //    {
        //        _Ex.SaveException("CostingController", "SearchByEnquiryID", ex.Message,userId);
        //        TempData["Error"] = ex.Message;
        //        return View("Create", new CostingModel());
        //    }
        //}
        [HttpPost]
        public IActionResult SearchByEnquiryID(string EnquiryCode)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(EnquiryCode))
                    return View("Create", new CostingModel());

                var model = _repo.GetByEnquiryCode(EnquiryCode);

                var enquiryId = _repo.GetEnquiryIdByCode(EnquiryCode);

                model.AdditionalRMList = _repo.GetAdditionalRM(enquiryId);
                model.AddRMWeight = (double)model.AdditionalRMList.Sum(x => x.TotalWeight);
                model.AddRMCost = (double)model.AdditionalRMList.Sum(x => x.Amount);
                // ────────────────────────────────────────────────────────────────

                HttpContext.Session.SetString("EnquiryCode", EnquiryCode);
                ViewBag.ActivePage = "Raw Material Costing";
                return View("Create", model);
            }
            catch (Exception ex)
            {
                _Ex.SaveException("CostingController", "SearchByEnquiryID", ex.Message, userId);
                TempData["Error"] = ex.Message;
                return View("Create", new CostingModel());
            }
        }
        // ─────────────────────────────────────────────────────────
        // EXPORT FI SHEET (Excel)
        // ─────────────────────────────────────────────────────────
        [HttpGet]
        public IActionResult ExportFISheet()
        {
            try
            {
                string enquiryCode = HttpContext.Session.GetString("EnquiryCode");

                if (string.IsNullOrWhiteSpace(enquiryCode))
                    return BadRequest("No enquiry in session. Please open a costing record first.");

                // Read RM summary values from session
                var rmModel = new RawMaterialsValues
                {
                    RMWeightOfPanels = (double)ParseSession("RM_WeightPanels"),
                    TotalPriceofPanels = (double)ParseSession("RM_PricePanels"),
                    TotalWeightOfFrame = (double)ParseSession("RM_WeightFrame"),
                    TotalPriceofFrame = (double)ParseSession("RM_PriceFrame"),
                    TotalCostOfRawMaterials = (double)ParseSession("RM_TotalCost"),
                    AdditionalRMWeight = (double)ParseSession("RM_AddWeight"),  // ← new
                    AdditionalRMCost = (double)ParseSession("RM_AddCost"),    // ← new
                    TotalWeight = (double)ParseSession("RM_WeightPanels")
                                             + (double)ParseSession("RM_WeightFrame")
                };

                var model = _repo.GetByEnquiryCode(enquiryCode);

                using var wb = new XLWorkbook();
                BuildFISheetForBooth(wb, model, enquiryCode, rmModel);
                BuildFrameSheet(wb, model);

                using var ms = new MemoryStream();
                wb.SaveAs(ms);
                ms.Position = 0;

                string fileName =
                    $"FI_Sheet_{enquiryCode}_{DateTime.Now:yyyyMMdd_HHmm}.xlsx";

                return File(ms.ToArray(),
                    "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                    fileName);
            }
            catch (Exception ex)
            {
                _Ex.SaveException("CostingController", "ExportFISheet", ex.Message,userId);
                return BadRequest("Export failed: " + ex.Message);
            }
        }
        #endregion

        #region BO Calculations
        public IActionResult BOCalculations_Index()
        {
            //ViewBag.ActivePage = "BO Calculations";
            var list = _repo.GetAllBO();
            return View(list);
        }

        [HttpGet]
        public async Task<IActionResult> BOCalculations_Create(string enquiryCode = null)
        {
            // ViewBag.ActivePage = "BO Calculations";

            // No enquiry code → return blank form
            if (string.IsNullOrWhiteSpace(enquiryCode))
                return View(new BOCalculationModel());

            // Enquiry code provided → load data and return filled form
            try
            {
                (decimal totalPriceOfFrame, decimal totalPriceOfPanels) =
                    _repo.GetCostOFFrame(enquiryCode);

                _repo.UpdateTotalPriceOfFrame(
                    enquiryCode, totalPriceOfFrame, totalPriceOfPanels);

                var frameDetails = _repo.GetAllFrameDetails(enquiryCode);
                foreach (var frame in frameDetails)
                    frame.AreaOfPaintArrestedFilter =
                        (frame.Width * frame.Height / 1000000m) * frame.Quantity;

                // Code merged by Ajay
                var tubeLights = _repo.GetAlllightDetails(enquiryCode);
                var exhaustDucts = _repo.GetAllExhaustDuctingDetails(enquiryCode);
                var paintBooth = _repo.GetAllBlowerDetails(enquiryCode);

                // ── Auto-fetch rates from Item API ──────────────────
                // ── Auto-fetch rates from Item API ──────────────────
                await PopulateRatesFromApi(tubeLights,
                    x => x.ItemCode,
                    (x, rate) => x.Rate = rate);

                await PopulateRatesFromApi(exhaustDucts,
                    x => x.ItemCode,
                    (x, rate) => x.Rate = rate);

                await PopulateRatesFromApi(paintBooth,
                    x => x.ItemCode,
                    (x, rate) => x.Rate = rate);

                await PopulateRatesFromApi(frameDetails,
                    x => x.ItemCode,
                    (x, rate) => x.Rate = (double)rate); // MaterialModel.Rate is double, cast here

                var savedBO = _repo.GetSavedBODetails(enquiryCode);
                // ── Load previously saved item rates ──────────────────
                var savedRates = _repo.GetSavedBOItemRates(enquiryCode);

                // ── Load additional BO rows ──────────────────────────
                var enquiryId = _repo.GetEnquiryIdByCode(enquiryCode);
                var additionalBOList = _repo.GetAdditionalBO(enquiryId);
                // ────────────────────────────────────────────────────

                foreach (var r in savedRates)
                    System.Diagnostics.Debug.WriteLine($"  {r.ItemType} row {r.RowIndex} rate {r.Rate}");
                var model = new BOCalculationModel
                {
                    EnquiryCode = enquiryCode,
                    TotalPriceOfFrame = totalPriceOfFrame,
                    TotalPriceOfPanels = totalPriceOfPanels,
                    FrameDetails = frameDetails,
                    PaintBooth = _repo.GetAllBlowerDetails(enquiryCode),
                    TubeLightDetails = _repo.GetAlllightDetails(enquiryCode),
                    ExhaustDuctDetails = _repo.GetAllExhaustDuctingDetails(enquiryCode),
                    // ── Pre-fill saved values if editing ──────────────
                    BoughtOut = savedBO?.TotalPriceOfBoughtOut ?? 0,
                    IsBOApproved = savedBO?.IsBOApproved ?? false,
                    SavedRates = savedRates,
                    // ── Additional BO ────────────────────────────────
                    AdditionalBOList = additionalBOList,
                    AddBOWeight = (double)additionalBOList.Sum(x => x.TotalWeight),
                    AddBOCost = (double)additionalBOList.Sum(x => x.Amount)
                    // ────────────────────────────────────────────────
                };

                return View(model);
            }
            catch (Exception ex)
            {
                _Ex.SaveException("CostingController", "BOCalculations_Create", ex.Message, userId);
                TempData["Error"] = ex.Message;
                return RedirectToAction("BOCalculations_Create");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult SavePriceDetails(BOCalculationModel model)
        {
            try
            {
                //// ── Read userId from session in controller ──────────
                //int userId = HttpContext.Session.GetInt32("UserId") ?? 0;
                // Save BO summary (BoughtOut + IsBOApproved)
                _repo.SavePriceDetailsValues(model, model.EnquiryCode, userId);

                // ── Save individual item rates ──────────────────────
                _repo.SaveBOItemRates(model.EnquiryCode, model.ItemRatesJson, userId);

                // ── Save additional BO rows ──────────────────────────
                var enquiryId = _repo.GetEnquiryIdByCode(model.EnquiryCode);
                _repo.DeleteAdditionalBO(enquiryId);

                if (model.AdditionalBOList != null && model.AdditionalBOList.Any())
                {
                    var rows = model.AdditionalBOList.Select(item => new BOAdditionalItems
                    {
                        EnquiryId = enquiryId,
                        Description = item.Description,
                        Qty = item.Qty,
                        ItemCode = item.ItemCode,        // ← NEW
                        UnitWeight = item.UnitWeight,
                        TotalWeight = item.TotalWeight,
                        Rate = item.Rate,
                        Amount = item.Amount,
                        CreatedAt = DateTime.Now
                    }).ToList();

                    _repo.SaveAdditionalBO(rows);
                }
                // ────────────────────────────────────────────────────

                TempData["Success"] = "Price details saved successfully.";
                return RedirectToAction("BOCalculations_Index");
            }
            catch (Exception ex)
            {
                _Ex.SaveException("CostingController", "SavePriceDetails", ex.Message, userId);
                TempData["Error"] = "Something went wrong: " + ex.Message;
                return RedirectToAction("BOCalculations_Create", new { enquiryCode = model.EnquiryCode });
            }
        }
        // Code Added BY Ajay 
        // Generic helper — works for any list of items with ItemCode + Rate
        // NEW — works with ANY type, no interface required
        private async Task PopulateRatesFromApi<T>(
            IEnumerable<T> items,
            Func<T, string> getItemCode,
            Action<T, decimal> setRate)
        {
            foreach (var item in items)
            {
                var itemCode = getItemCode(item);
                if (string.IsNullOrWhiteSpace(itemCode)) continue;

                var apiItem = await _service.GetItemByCodeAsync(itemCode);
                setRate(item, apiItem?.last_purchase_rate ?? 0);
            }
        }
       
        [HttpGet]
        public async Task<IActionResult> GetItemRate(string itemCode)
        {
            var item = await _service.GetItemByCodeAsync(itemCode);
            if (item == null)
                return Json(new { success = false, message = "Item not found" });

            return Json(new { success = true, rate = item.last_purchase_rate });
        }
        #endregion


        #region Price Bid

        // ── GET /Costing/PriceBID_Index ───────────────────────────────
        public IActionResult PriceBID_Index()
        {
            // ViewBag.ActivePage = "Price BID";
            var list = _repo.GetAllPriceBID();
            return View(list);
        }

        [HttpGet]
        public IActionResult PriceBID_Create(string salesNO = null)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            try
            {
                ViewBag.ActivePage = "Price BID";

            if (string.IsNullOrWhiteSpace(salesNO))
                return View(new PriceBIDModel());

            // ── Edit mode — load ALL saved values ────────────────────
            var model = _repo.FetchPriceBidRecoreds(salesNO) ?? new PriceBIDModel();
            model.SalesNO = salesNO;

            //// Load RM and BO costs from EnquiryMaster / PriceDetailsTable
            //(decimal priceOfFrame, decimal priceOfPanels) = _repo.GetCostOFFrame(salesNO);

            //model.PriceOfRM = priceOfPanels;    // RM cost
            //model.PriceOfBO = priceOfFrame;     // BO cost
            //model.LabourCost = (decimal)Math.Round((double)(model.PriceOfRM * 35m / 80m));

            // Load payment terms
            var enquiryId = _repo.GetEnquiryIdBySalesNo(salesNO);
            var payTerms = _repo.GetAllPaymentTerms(enquiryId);
            if (payTerms != null)
            {
                model.AdvancedPercentage = payTerms.AdvancedPercentage;
                model.DAPPercentage = payTerms.DAPPercentage;
                model.SUPPLYBEFOREDISPATCHPercentage = payTerms.SUPPLYBEFOREDISPATCHPercentage;
                model.SUPPLYAFTERDISPATCHPercentage = payTerms.SUPPLYAFTERDISPATCHPercentage;
                model.INSTALLATIONPercentage = payTerms.INSTALLATIONPercentage;
                model.COMMISSIONINGPercentage = payTerms.COMMISSIONINGPercentage;
                model.DeliverySupplyperiod = payTerms.DeliverySupplyperiod;
                model.EAndCPeriod = payTerms.EAndCPeriod;
            }
                // ── Load saved E&C calculation ───────────────────────
                var encData = _repo.GetENCCalculation(salesNO);
                if (encData != null)
                    model.EAndCCost = encData.FinalValue;
                // ────────────────────────────────────────────────────
                // NegotiationRate default
                if (model.NegotiationRate == 0) model.NegotiationRate = 10;

            return View("PriceBID_Create", model);
            }
            catch (Exception ex)
            {
                _Ex.SaveException("PriceBIDController", "PriceBID_Create", ex.Message, (int)userId);
                return RedirectToAction(nameof(Index));
            }
        }

        // ── POST /Costing/FetchPriceBIDData ──────────────────────────
        // ADD one line to store enquiryCode in session — this is the
        // safety fallback for UpdatePriceDetails if hidden field fails

        [HttpPost]
        public IActionResult FetchPriceBIDData(string enquiryCode)
        {
            try
            {
                // ── Store in session as safety backup ─────────────────
                HttpContext.Session.SetString("PriceBIDEnquiryCode", enquiryCode ?? "");

                var priceBIDData = _repo.GetPriceBIDData(enquiryCode);
                var totalWeights = _repo.CalculateTotalWeightByEnquiryCode(enquiryCode);

                if (priceBIDData == null || !priceBIDData.Any())
                    return Json(new { success = false, message = "Costing Approval Pending..." });

                return Json(new { success = true, data = priceBIDData, totalWeights });
            }
            catch (Exception ex)
            {
                _Ex.SaveException("PriceBIDController", "FetchPriceBIDData", ex.Message, userId);
                TempData["ErrorMessage"] = ex.Message;
                return Json(new { success = false, message = ex.Message });
            }
        }

        // ── POST /Costing/UpdatePriceDetails ─────────────────────────
       
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult UpdatePriceDetails(PriceBIDModel model)
        {
            try
            {
                //int userId = HttpContext.Session.GetInt32("UserId") ?? 0;
                // ── SalesNO null safety ───────────────────────────────
                // 1st: from model binding (hidden field in form)
                // 2nd: from session (set when FetchPriceBIDData was called)
                if (string.IsNullOrWhiteSpace(model.SalesNO))
                    model.SalesNO = HttpContext.Session.GetString("PriceBIDEnquiryCode");

                if (string.IsNullOrWhiteSpace(model.SalesNO))
                {
                    TempData["ErrorMessage"] = "Enquiry code is missing. Please search again.";
                    return RedirectToAction("PriceBID_Index");
                }

                _repo.UpdatePriceDetails(model);
                _repo.SavePaymentTermsValues(model, model.SalesNO, userId);

                TempData["SuccessMessage"] = $"Price BID for {model.SalesNO} saved successfully.";
                return RedirectToAction("PriceBID_Index");
            }
            catch (Exception ex)
            {
                _Ex.SaveException("CostingController", "UpdatePriceDetails", ex.Message, userId);
                TempData["ErrorMessage"] = "Something went wrong: " + ex.Message;
                return RedirectToAction("PriceBID_Create", new { salesNO = model?.SalesNO });

            }
        }


        [HttpPost]
        public IActionResult SaveENCCalculation(string salesNO, string rowsJson,
                                         decimal margin, decimal subTotal,
                                         decimal finalValue)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(salesNO))
                    return Json(new { success = false, message = "SalesNO missing." });

                // ── Get enquiryId from SalesNO ───────────────────────
                var enquiryId = _repo.GetEnquiryIdBySalesNo(salesNO);

                _repo.SaveENCCalculation(salesNO, enquiryId, rowsJson,
                                          margin, subTotal, finalValue);

                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                _Ex.SaveException("CostingController", "SaveENCCalculation", ex.Message, userId);
                return Json(new { success = false, message = ex.Message });
            }
        }
        [HttpGet]
        public IActionResult GetENCCalculation(string salesNO)
        {
            try
            {
                var data = _repo.GetENCCalculation(salesNO);
                if (data == null)
                    return Json(new { success = false });

                return Json(new
                {
                    success = true,
                    rowsJson = data.RowsJson,
                    margin = data.ContractorMargin,
                    subTotal = data.SubTotal,
                    finalValue = data.FinalValue
                });
            }
            catch (Exception ex)
            {
                _Ex.SaveException("CostingController", "GetENCCalculation", ex.Message, userId);
                return Json(new { success = false });
            }
        }
        #endregion

        #region Proposal

        // ─────────────────────────────────────────────────────────
        // PRICE BID
        // ─────────────────────────────────────────────────────────
        //public IActionResult PriceBID()
        //{
        //    ViewBag.ActivePage = "Price BID";
        //    return View();
        //}

        //[HttpPost]
        //public IActionResult FetchPriceBIDData(string enquiryCode)
        //{
        //    try
        //    {
        //        var data = _repo.GetPriceBIDData(enquiryCode);
        //        var totalWeights = _repo.CalculateTotalWeightByEnquiryCode(enquiryCode);

        //        if (data == null || !data.Any())
        //            return Json(new { success = false, message = "Costing Approval Pending..." });

        //        return Json(new { success = true, data, totalWeights });
        //    }
        //    catch (Exception ex)
        //    {
        //        return Json(new { success = false, message = ex.Message });
        //    }
        //}

        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public IActionResult UpdatePriceDetails(PriceBIDModel model)
        //{
        //    try
        //    {
        //        _repo.UpdatePriceDetails(model);
        //        _repo.SavePaymentTermsValues(model, model.SalesNO);
        //        TempData["Success"] = "Price bid details saved successfully.";
        //        return RedirectToAction("PriceBID", new { SalesNO = model.SalesNO });
        //    }
        //    catch
        //    {
        //        TempData["Error"] = "Something went wrong!";
        //        return RedirectToAction("PriceBID", new { SalesNO = model?.SalesNO });
        //    }
        //}

        // ─────────────────────────────────────────────────────────
        // PROPOSAL
        // ─────────────────────────────────────────────────────────
        public IActionResult ProposalIndex()
        {
            ViewBag.ActivePage = "Proposal Approval";
            var list = _repo.GetAllProposals();
            return View(list);
        }

        // ── GET /Costing/ProposalForm ──────────────────────────────
        // Blank form — user types SalesNO and submits
        public IActionResult ProposalCreate()
        {
            ViewBag.ActivePage = "Proposal Approval";
            return View(new ProposalPriceBIDViewModel());
        }
       
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ShowProposalForm(string SalesNO)
        {
            try
            {
                var viewModel = new ProposalPriceBIDViewModel();

                if (!string.IsNullOrWhiteSpace(SalesNO))
                {
                    viewModel.ProposalDetails = _repo.GetAllDetails(SalesNO);

                    if (viewModel.ProposalDetails != null)
                    {
                        viewModel.ProposalDetails.TypeOfPaintName =
                            viewModel.ProposalDetails.TypeOfPaint switch
                            {
                                "1" => "Water Based",
                                "2" => "PU Paint",
                                "3" => "Epoxy",
                                _ => viewModel.ProposalDetails.TypeOfPaint
                            };
                        viewModel.ProposalDetails.DraftType=
                        viewModel.ProposalDetails.DraftType switch
                        {
                            "1" => viewModel.ProposalDetails.DraftType = "Side Draught",
                            "2" => viewModel.ProposalDetails.DraftType = "Down Draught",
                            "3" => viewModel.ProposalDetails.DraftType = "Semi Down Draught",
                            _ => viewModel.ProposalDetails.DraftType = viewModel.ProposalDetails.DraftType
                        };
                        viewModel.ProposalDetails.SubTypeOfDraftType =
                        viewModel.ProposalDetails.SubTypeOfDraftType switch
                        {
                            "1" => viewModel.ProposalDetails.SubTypeOfDraftType = "Front Open",
                            "2" => viewModel.ProposalDetails.SubTypeOfDraftType = "Negative Pressurized Booth",
                            "3" => viewModel.ProposalDetails.SubTypeOfDraftType = "Positive Pressurized Booth",
                            "4" => viewModel.ProposalDetails.SubTypeOfDraftType = "With FAS & Civil",
                            "5" => viewModel.ProposalDetails.SubTypeOfDraftType = "With FAS & Without Civil",
                            "8" => viewModel.ProposalDetails.SubTypeOfDraftType = "With FAS",
                            "9" => viewModel.ProposalDetails.SubTypeOfDraftType = "With Filters on Top",
                            _ => viewModel.ProposalDetails.SubTypeOfDraftType = viewModel.ProposalDetails.SubTypeOfDraftType
                        };

                        viewModel.ProposalDetails.ComponantHandlingName =
                            viewModel.ProposalDetails.ComponentHandling == "1"
                                ? "Applicable" : "Not-Applicable";

                        viewModel.ProposalDetails.CycleTime =
                            CalculateCycleTime(480,
                                viewModel.ProposalDetails.ProductionRequirement);

                        viewModel.PriceBIDDetails =
                            _repo.FetchPriceBidRecoreds(SalesNO);


                        viewModel.MaterialList =
                            _repo.GetMaterialsValue(SalesNO);
                       
                        viewModel.BlowerDetails =
                            _repo.GetBlowerMounting(viewModel.ProposalDetails.EnquiryID);
                        //sampling platform and ladder
                        bool isSamplingProvided = _repo.IsSamplingPlatformProvided(viewModel.ProposalDetails.EnquiryID);
                        viewModel.samplingPlatformAndLadder = isSamplingProvided ? "Provided" : "Not-Provided";
                        // ── Exhaust ducting details for Technical Spec row 9 ──
                        viewModel.ExhaustDuctings =
                            _repo.GetExhaustDuctings(SalesNO, viewModel.ProposalDetails.EnquiryID);
                        viewModel.ExhaustDuctingSummary =
                            ExhaustDuctingSummary.Build(viewModel.ExhaustDuctings);


                        viewModel.exhaustPrice = _repo.GetExhaustDuctPrice(SalesNO,viewModel.ProposalDetails.EnquiryID);

                        viewModel.PaymentTerms =
                            _repo.GetAllPaymentTerms(
                                viewModel.ProposalDetails.EnquiryID);
                        viewModel.salesEngineer = _repo.GetSalesDetails(viewModel.ProposalDetails.SalesEngineerId);
                    }
                }

                return View("ProposalCreate", viewModel);
            }
            catch (Exception ex)
            {
                _Ex.SaveException("CostingController", "ShowProposalForm", ex.Message, userId);
                TempData["Error"] = ex.Message;
                return View("ProposalCreate", new ProposalPriceBIDViewModel());

            }
        }

        private decimal CalculateCycleTime(int shiftMinutes, int productionPerShift)
        {
            if (productionPerShift == 0) return 0;
            return Math.Round((decimal)shiftMinutes / productionPerShift, 2);
        }
        #endregion

        #region Materials
        // ── GET /Costing/Material ─────────────────────────────────
        public IActionResult Material(int id = 0)
        {
            ViewBag.getmaterial = _repo.GetAllMaterial();
            if (id > 0)
            {
                var data = _repo.GetMaterialById(id);
                if (data == null) return RedirectToAction(nameof(Material));
                return View(data);
            }
            return View(new MaterialModel());
        }

        // ── POST /Costing/SaveMaterials ───────────────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult SaveMaterials(MaterialModel model)
        {
            int userId = HttpContext.Session.GetInt32("UserId") ?? 0;
            _repo.SaveMaterial(model, userId);
            TempData["Success"] = model.MOCID > 0
                ? $"{model.MOCName} updated successfully."
                : $"{model.MOCName} added successfully.";
            return RedirectToAction(nameof(Material));
        }

        // ── POST /Costing/DeleteMaterial ─────────────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteMaterial(int id)
        {
            int userId = HttpContext.Session.GetInt32("UserId") ?? 0;
            _repo.DeleteMaterial(id, userId);
            TempData["Success"] = "Material deleted successfully.";
            return RedirectToAction(nameof(Material));
        }
        #endregion

        #region Excel

      
        private decimal ParseSession(string key)
        {
            return decimal.TryParse(
                HttpContext.Session.GetString(key), out var val) ? val : 0;
        }

        //private decimal CalculateCycleTime(int shiftMinutes, int productionPerShift)
        //{
        //    if (productionPerShift == 0) return 0;
        //    return Math.Round((decimal)shiftMinutes / productionPerShift, 2);
        //}

        private string GetPanelPrefix(string panelPosition)
        {
            if (string.IsNullOrEmpty(panelPosition)) return "";
            if (panelPosition.StartsWith("PARight")) return "PA Right";
            if (panelPosition.StartsWith("PALeft")) return "PA Left";
            if (panelPosition.StartsWith("PATop")) return "PA Top";
            if (panelPosition.StartsWith("PAFront")) return "PA Front";
            if (panelPosition.StartsWith("ECRight")) return "EC Right";
            if (panelPosition.StartsWith("ECLeft")) return "EC Left";
            if (panelPosition.StartsWith("ECRear")) return "EC Rear";
            if (panelPosition == "TopStructureFrame") return "Top Structure Frame";
            if (panelPosition == "BaseStructureFrame") return "Base Structure Frame";
            if (panelPosition == "MetalBaffle") return "Metal Baffle";
            return panelPosition;
        }

        // ─────────────────────────────────────────────────────────
        // EXCEL BUILDERS (same logic as old controller — unchanged)
        // ─────────────────────────────────────────────────────────
        private void BuildFISheetForBooth(XLWorkbook wb, CostingModel model,
                                           string enquiryCode, RawMaterialsValues rmModel)
        {
            var ws = wb.Worksheets.Add("FI SHEET FOR BOOTH");
            var headerBg = XLColor.FromArgb(0x40, 0x40, 0x40);
            var subHdrBg = XLColor.FromArgb(0xD9, 0xD9, 0xD9);
            var sectionBg = XLColor.FromArgb(0xBD, 0xD7, 0xEE);
            var white = XLColor.White;
            var black = XLColor.Black;

            ws.Column(1).Width = 8; ws.Column(2).Width = 20;
            ws.Column(3).Width = 35; ws.Column(4).Width = 18;
            ws.Column(5).Width = 35; ws.Column(6).Width = 8;
            ws.Column(7).Width = 8; ws.Column(8).Width = 8;
            ws.Column(9).Width = 8; ws.Column(10).Width = 8;
            ws.Column(11).Width = 12; ws.Column(12).Width = 12;
            ws.Column(13).Width = 12; ws.Column(14).Width = 25;

            int row = 1;
            WriteHeaderBlock(ws, ref row, enquiryCode);
            WriteColumnHeaders(ws, ref row, headerBg, white);

            var panelGroups = model.DetailsList
                .Where(d => d.PanelPosition != "TopStructureFrame"
                         && d.PanelPosition != "BaseStructureFrame")
                .GroupBy(d => GetPanelPrefix(d.PanelPosition)).ToList();

            int sectionNo = 1;
            foreach (var group in panelGroups)
            {
                WriteSectionHeader(ws, ref row, sectionNo++, group.Key, sectionBg, black);
                int itemNo = 1;
                foreach (var panel in group)
                    WritePanelRow(ws, ref row, itemNo++, panel, subHdrBg);
                row++;
            }

            WriteTotalsRow(ws, ref row, headerBg, white, rmModel);
            WriteAbbreviations(ws, ref row);
        }

        private void BuildFrameSheet(XLWorkbook wb, CostingModel model)
        {
            var ws = wb.Worksheets.Add("FRAME DETAILS");
            var headerBg = XLColor.FromArgb(0x40, 0x40, 0x40);
            var white = XLColor.White;

            ws.Column(1).Width = 8; ws.Column(2).Width = 30;
            ws.Column(3).Width = 15; ws.Column(4).Width = 15;
            ws.Column(5).Width = 20; ws.Column(6).Width = 15;

            int row = 1;
            string[] hdrs = { "Item", "Frame Position", "Frame Width",
                               "Frame Length", "Size", "Weight(Kg)" };
            for (int c = 0; c < hdrs.Length; c++)
                StyleHeader(ws.Cell(row, c + 1), headerBg, white, hdrs[c]);
            row++;

            int i = 1;
            foreach (var frame in model.FilteredDetailsList)
            {
                ws.Cell(row, 1).Value = i++;
                ws.Cell(row, 2).Value = frame.PanelPosition;
                ws.Cell(row, 3).Value = frame.FrameWidth;
                ws.Cell(row, 4).Value = frame.FrameHeight;
                ws.Cell(row, 5).Value = frame.Section ?? "";
                ws.Cell(row, 6).Value = Math.Round(frame.PanelWeight, 3);
                var r = ws.Range(row, 1, row, 6);
                r.Style.Font.FontName = "Arial"; r.Style.Font.FontSize = 10;
                r.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                r.Style.Border.InsideBorder = XLBorderStyleValues.Thin;
                r.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                ws.Cell(row, 2).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                row++;
            }
            row++;
            ws.Cell(row, 2).Value = "Total Frame Weight";
            ws.Cell(row, 6).Value = model.TotalWeightOfFrame;
            ws.Cell(row, 2).Style.Font.Bold = true;
            ws.Cell(row, 6).Style.Font.Bold = true;
        }

        private void WriteHeaderBlock(IXLWorksheet ws, ref int row, string enquiryCode)
        {
            SetCell(ws, row, 1, "Sheet No :-", true); SetCell(ws, row, 3, "2");
            SetCell(ws, row, 12, "Prepared By :-", true); SetCell(ws, row, 13, "GDM"); row++;
            SetCell(ws, row, 1, "Title Of Sheet :-", true); SetCell(ws, row, 3, "Fabrication");
            SetCell(ws, row, 4, "Offer No."); SetCell(ws, row, 5, enquiryCode);
            SetCell(ws, row, 10, "DATE"); SetCell(ws, row, 11, DateTime.Now.ToString("dd/MM/yyyy"));
            SetCell(ws, row, 13, "ABK"); row++;
            SetCell(ws, row, 1, "Department :-", true);
            SetCell(ws, row, 3, "Engineering Department"); SetCell(ws, row, 4, "Po No. :-"); row++;
            SetCell(ws, row, 1, "Project Name :-", true); SetCell(ws, row, 3, "Booth");
            SetCell(ws, row, 4, "So No. :-"); SetCell(ws, row, 10, "DATE");
            SetCell(ws, row, 11, DateTime.Now.ToString("dd/MM/yyyy"));
            SetCell(ws, row, 12, "Revision :-"); SetCell(ws, row, 13, "R0"); row++;
            row++;
        }

        private void WriteColumnHeaders(IXLWorksheet ws, ref int row,
                                         XLColor bg, XLColor fg)
        {
            string[] hdrs = { "S. No","Item Code","Item Description","Material",
                               "Technical Specifications","SOS","DS","Qty","QTY",
                               "UOM","WPU","TW","","TSA","Remarks" };
            for (int c = 0; c < hdrs.Length; c++)
                StyleHeader(ws.Cell(row, c + 1), bg, fg, hdrs[c]);
            row++;
            ws.Cell(row, 11).Value = "(kg.)";
            ws.Cell(row, 12).Value = "(kg.)";
            ws.Cell(row, 14).Value = "(m²)";
            ws.Row(row).Style.Fill.BackgroundColor = bg;
            ws.Row(row).Style.Font.FontColor = fg;
            ws.Row(row).Style.Font.FontName = "Arial";
            ws.Row(row).Style.Font.FontSize = 9;
            ws.Row(row).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            row++;
        }

        private void WriteSectionHeader(IXLWorksheet ws, ref int row,
                                         int sectionNo, string title,
                                         XLColor bg, XLColor fg)
        {
            ws.Cell(row, 1).Value = sectionNo.ToString();
            ws.Cell(row, 3).Value = title.ToUpper();
            var range = ws.Range(row, 1, row, 14);
            range.Style.Fill.BackgroundColor = bg;
            range.Style.Font.Bold = true;
            range.Style.Font.FontName = "Arial";
            range.Style.Font.FontSize = 10;
            row++;
        }

        private void WritePanelRow(IXLWorksheet ws, ref int row,
                                    int itemNo, PaintBoothModel panel,
                                    XLColor altBg)
        {
            double w = panel.StandardPanelWidthForD > 0
                          ? panel.StandardPanelWidthForD : panel.FrameWidth;
            double h = panel.EqualPanelWidthByH > 0
                          ? panel.EqualPanelWidthByH : panel.FrameHeight;
            string size = (w > 0 && h > 0)
                          ? $"Size :- {w}x{h}x{panel.SheetThickness}" : "";

            ws.Cell(row, 1).Value = itemNo;
            ws.Cell(row, 3).Value = panel.DisplayPanelPosition;
            ws.Cell(row, 4).Value = $"G.I. {panel.SheetThickness}mm Thk";
            ws.Cell(row, 5).Value = size;
            ws.Cell(row, 6).Value = "FI";
            ws.Cell(row, 9).Value = panel.NoofPanels;
            ws.Cell(row, 10).Value = "Nos";
            ws.Cell(row, 11).Value = Math.Round(panel.PanelWeight, 3);
            ws.Cell(row, 12).Value = Math.Round(panel.PanelWeight * panel.NoofPanels, 3);

            var range = ws.Range(row, 1, row, 14);
            range.Style.Font.FontName = "Arial"; range.Style.Font.FontSize = 10;
            range.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
            range.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            range.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            range.Style.Border.InsideBorder = XLBorderStyleValues.Thin;
            range.Style.Border.OutsideBorderColor = XLColor.FromArgb(0xBF, 0xBF, 0xBF);
            range.Style.Border.InsideBorderColor = XLColor.FromArgb(0xBF, 0xBF, 0xBF);
            ws.Cell(row, 3).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
            ws.Cell(row, 5).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
            if (itemNo % 2 == 0)
                range.Style.Fill.BackgroundColor = XLColor.FromArgb(0xF2, 0xF2, 0xF2);
            row++;
        }

        private void WriteTotalsRow(IXLWorksheet ws, ref int row,
                                     XLColor bg, XLColor fg,
                                     RawMaterialsValues rm)
        {
            row++; // blank spacer

            var summaryRows = new (string Label, decimal Value)[]
            {
                ("TOTAL WEIGHT",              (decimal) rm.TotalWeight),
                ("RM Weight of Panels",         (decimal) rm.RMWeightOfPanels),
                ("Total Weight of Frame",        (decimal) rm.TotalWeightOfFrame),
                ("Total Price of Panels",        (decimal) rm.TotalPriceofPanels),
                ("Total Price of Frame",             (decimal) rm.TotalPriceofFrame),
                ("Total Cost of Raw Materials",   (decimal)rm.TotalCostOfRawMaterials)
            };

            foreach (var item in summaryRows)
            {
                ws.Cell(row, 3).Value = item.Label;
                ws.Cell(row, 12).Value = item.Value;
                ws.Cell(row, 3).Style.Font.Bold = true;
                ws.Cell(row, 3).Style.Font.FontName = "Arial";
                ws.Cell(row, 3).Style.Font.FontSize = 10;
                ws.Cell(row, 12).Style.Font.FontName = "Arial";
                ws.Cell(row, 12).Style.Font.FontSize = 10;
                ws.Cell(row, 12).Style.NumberFormat.Format = "#,##0.000";
                row++;
            }
        }

        private void WriteAbbreviations(IXLWorksheet ws, ref int row)
        {
            row += 2;
            ws.Cell(row, 1).Value = "Abbreviations used";
            ws.Cell(row, 1).Style.Font.Bold = true;
            row++;
            var items = new[]
            {
                ("WPU – Weight Per Unit",    "FI – Fabricated Item"),
                ("TW – Total Weight",         "BO – Brought out Item"),
                ("APU – Area Per Unit",       "DS – Drawing Size"),
                ("TSA – Total Surface Area",  "UOM – Unit of Measurement"),
                ("SOS – Source of Supply",    "")
            };
            foreach (var (left, right) in items)
            {
                ws.Cell(row, 1).Value = left; ws.Cell(row, 5).Value = right;
                ws.Cell(row, 1).Style.Font.FontName = "Arial";
                ws.Cell(row, 1).Style.Font.FontSize = 9;
                ws.Cell(row, 5).Style.Font.FontName = "Arial";
                ws.Cell(row, 5).Style.Font.FontSize = 9;
                row++;
            }
        }

        private void StyleHeader(IXLCell cell, XLColor bg, XLColor fg, string value)
        {
            cell.Value = value;
            cell.Style.Fill.BackgroundColor = bg;
            cell.Style.Font.FontColor = fg;
            cell.Style.Font.Bold = true;
            cell.Style.Font.FontName = "Arial";
            cell.Style.Font.FontSize = 10;
            cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            cell.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
            cell.Style.Alignment.WrapText = true;
            cell.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            cell.Style.Border.OutsideBorderColor = XLColor.White;
        }

        private void SetCell(IXLWorksheet ws, int row, int col,
                              string value, bool bold = false)
        {
            var cell = ws.Cell(row, col);
            cell.Value = value;
            cell.Style.Font.FontName = "Arial";
            cell.Style.Font.FontSize = 10;
            cell.Style.Font.Bold = bold;
        }

        #endregion
    }
}
