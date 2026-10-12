
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PaintBooth.Models;
using PaintBooth.Repositories.Interface;

namespace PaintBooth.Controllers
{
    [Authorize]
    public class PanelSettingController(IPanelSettingRepository _repo, IHttpContextAccessor _httpContextAccessor, IExceptionRepository _Ex) : Controller
    {
        private int userId => HttpContext.Session.GetInt32("UserId") ?? 0;
        // ── GET /PanelSetting/Index ───────────────────────────────
        public IActionResult Index()
        {
            var list = _repo.GetAll();
            return View(list);
        }

        // ── GET /PanelSetting/Create ──────────────────────────────
        public IActionResult Create()
        {

            try
            {
                var model = new PanelSettingFormViewModel();
                return View(model);
            }
            catch (Exception ex)
            {
                _Ex.SaveException("PanelSettingController", "Create", ex.Message, userId);
                return RedirectToAction(nameof(Index));
            }
        }


        // ── AJAX: GET paintbooth category for an enquiry ──────────
        [HttpGet]
        public JsonResult GetEnquiryCategory(string salesNo)
        {
            //try
            //{
            //    var category = _repo.GetCategoryBySalesNo(salesNo ?? "");
            //    return Json(new { success = true, category });
            //}
            //catch (Exception ex)
            //{
            //    _Ex.SaveException("PanelSettingController", "GetEnquiryCategory", ex.Message, userId);
            //    return Json(new { success = false, category = "" });
            //}
            try
            {
                var category = _repo.GetCategoryBySalesNo(salesNo ?? "");
                var paintboothType = _repo.GetPaintboothType(salesNo ?? "");  // ← add this
                return Json(new { success = true, category, paintboothType });
            }
            catch (Exception ex)
            {
                _Ex.SaveException("PanelSettingController", "GetEnquiryCategory", ex.Message, userId);
                return Json(new { success = false, category = "", paintboothType = "" });
            }
        }
        // ── GET /PanelSetting/Edit?salesNo=ENQ-001 ────────────────
        public IActionResult Edit(string salesNo)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(salesNo))
                    return RedirectToAction(nameof(Index));

                var settings = _repo.GetBySalesNo(salesNo);
                string paintboothType = _repo.GetPaintboothType(salesNo);
                var model = new PanelSettingFormViewModel
                {
                    SalesNo = salesNo,
                    Settings = settings,
                    PaintboothType = paintboothType
                };

                return View("Create", model);
            }
            catch (Exception ex)
            {
                _Ex.SaveException("PanelSettingController", "Edit", ex.Message, userId);
                return RedirectToAction(nameof(Index));
            }
        }

        // ── POST /PanelSetting/SaveSetting ────────────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult SaveSetting(PanelSettingFormViewModel model)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(model.SalesNo))
                {
                    TempData["Error"] = "Please enter an Enquiry Code.";
                    return View("Create", model);
                }

                if (model.Settings == null || !model.Settings.Any())
                {
                    TempData["Error"] = "Please add at least one setting.";
                    return View("Create", model);
                }

                var userId = HttpContext.Session.GetInt32("UserId") ?? 0;

                _repo.Save(model.SalesNo, model.Settings, userId);
                _repo.SaveTubeLightDetails(model.SalesNo, model.Settings, userId);
                TempData["Success"] = $"Settings for {model.SalesNo} saved successfully.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _Ex.SaveException("PanelSettingController", "SaveSetting", ex.Message, userId);
                return RedirectToAction(nameof(Index));
            }
        }

        // ── POST /PanelSetting/Delete ─────────────────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Delete(string salesNo)
        {
            var userId = HttpContext.Session.GetInt32("UserId") ?? 0;
            _repo.Delete(salesNo, userId);

            TempData["Success"] = $"Settings for {salesNo} deleted successfully.";
            return RedirectToAction(nameof(Index));
        }

        // ── AJAX: GET enquiry codes for autocomplete ──────────────
        [HttpPost]
        public JsonResult GetEnquiryCodes(string enquiryCode)
        {
            var results = _repo.SearchEnquiryCodes(enquiryCode ?? "");
            return Json(new { success = true, results });
        }

        // ── AJAX: GET existing settings for an enquiry ────────────
        [HttpPost]
        public JsonResult GetSettingsByEnquiry(string enquiryCode)
        {
            try
            {
                var exists = _repo.ExistsBySalesNo(enquiryCode);
                var settings = _repo.GetBySalesNo(enquiryCode);

                return Json(new
                {
                    exists,
                    settings = settings.Select(x => new
                    {
                        sId = x.SId,
                        settingType = x.SettingType,
                        panelWidth = x.PanelWidth,
                        panelHeight = x.PanelHeight,
                        sheetThickness = x.SheetThickness,
                        section = x.Section,
                        bendStructural = x.BendStructural,
                        materials = x.Materials,
                        standardBend1 = x.StandardBend1,
                        standardBend2 = x.StandardBend2,
                        pitchDistance = x.PitchDistance,
                        slotDimensions = x.SlotDimensions,
                        lightType = x.LightTypes,
                        lightSubType = x.LightSubTypes,
                        luxLevel = x.LuxLevel,
                        lumens = x.Lumens,
                        itemCode = x.ItemCode,   // ← NEW
                        rate = x.Rate            // ← NEW
                    })
                });
            }
            catch (Exception ex)
            {
                _Ex.SaveException("PanelSettingController", "GetSettingsByEnquiry", ex.Message, userId);
                return Json(new { success = false, message = "Something went wrong. Please try again." });
            }
        }
    }
}
