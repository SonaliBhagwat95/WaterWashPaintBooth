
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PaintBooth.Database;
using PaintBooth.Models;
using PaintBooth.Models.ERP;
using PaintBooth.Repositories.Interface;
using PaintBooth.Service;

namespace PaintBooth.Controllers
{
    [Authorize]
    public class EnquiryController(IEnquiryRepository _repo, IWebHostEnvironment _env, IExceptionRepository _Ex, FrappeOpportunityService _frappeService) : Controller
    {


        // ── GET /Enquiry/Index ────────────────────────────────────
        public IActionResult Index()
        {
            var list = _repo.GetAll();
            return View(list);
        }
       
        // ── GET /Enquiry/Create ───────────────────────────────────
        public async Task<IActionResult> Create(string opportunityName = null)
        {
            var vm = new EnquiryFormViewModel
            {
                Enquiry = new EnquiryMaster { EnquiryDate = DateTime.Today },
                Component = new ComponentTable { ExtractionChamberHeight = "2100" },
                SalesEngineers = _repo.GetSalesEngineersDropdown()
            };

            // ── Auto-fill from ERP Opportunity if opportunityName is passed ──
            if (!string.IsNullOrWhiteSpace(opportunityName))
            {
                var userId = HttpContext.Session.GetInt32("UserId") ?? 0;
                try
                {
                    var opp = await _frappeService.GetOpportunityByNameAsync(opportunityName);

                    if (opp != null)
                    {
                        // ── Map only fields that already exist in the form ──
                        vm.Enquiry.SalesNO = opp.name;

                        vm.customer = new EnquiryListViewModel
                        {
                            CompanyName = opp.customer_name,
                            ContactPerson = opp.contact_person,
                            Email = opp.contact_email,
                            MobileNo = opp.contact_mobile,
                            Address = opp.customer_address,
                            CustomerLocation = opp.customer_location   // ← NEW: carried through the form

                        };
                        
                    }
                    else
                    {
                        TempData["Error"] = $"Could not fetch details for Opportunity '{opportunityName}' from ERP.";
                    }
                }
                catch (Exception ex)
                {
                    _Ex.SaveException("EnquiryController", "Create (ERP autofill)", ex.Message, userId);
                    TempData["Error"] = "Failed to fetch enquiry details from ERP.";
                }
            }

            return View(vm);
        }

        // ── POST /Enquiry/Create ──────────────────────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(EnquiryFormViewModel vm, IFormFile? imageFile)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            try
            {
                // Duplicate SalesNO check
                if (!string.IsNullOrWhiteSpace(vm.Enquiry.SalesNO)
                    && _repo.SalesNoExists(vm.Enquiry.SalesNO))
                {
                    ModelState.AddModelError("Enquiry.SalesNO",
                        "This Enquiry No already exists. Please use a different one.");
                }


                // ── NEW: fallback if no customer was picked via autocomplete ──
                if (vm.Enquiry.CustomerID <= 0)
                {
                    vm.Enquiry.CustomerID = _repo.GetOrCreateCustomer(vm.customer);
                }
                // Handle file upload
                if (imageFile != null && imageFile.Length > 0)
                    vm.Component.Image_Path = await SaveFile(imageFile);


                vm.Enquiry.CreatedBy = userId;
                vm.Component.CreatedBy = userId;
              
                if (vm.Enquiry.SubTypeOfDraftType == "1")
                    vm.Enquiry.SideLocationDoorFAS = "";
                if (vm.Enquiry.FilterTypes == null)
                    vm.Enquiry.FilterTypes = "";


                _repo.Add(vm.Enquiry, vm.Component, vm.MotorDetails);

                TempData["Success"] = $"Enquiry {vm.Enquiry.SalesNO} saved successfully.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _Ex.SaveException("EnquiryController", "Create", ex.Message, (int)userId);
                return RedirectToAction(nameof(Index));
            }
        }

        // ── GET /Enquiry/Edit/5 ───────────────────────────────────
        public IActionResult Edit(int id)
        {
            var userId = HttpContext.Session.GetInt32("UserId") ?? 0;
            try
            {
                var enquiry = _repo.GetById(id);
                if (enquiry == null) return NotFound();

                var component = _repo.GetComponentByEnquiryId(id)
                                ?? new ComponentTable();
                // ── Reverse-map dimensions for form display ───────────────────
                var shape = component.Shape ?? "";
                var orientation = component.Orientation ?? "";

                if (shape == "Circular" && orientation == "Horizontal")
                {
                    component.Diameter = component.Length;      // Length = Diameter
                    component.CircularHeightSize = component.HeightSize;
                }
                else if (shape == "Circular" && orientation == "Vertical")
                {
                    component.Diameter = component.Length;      // Length = Diameter
                    component.CircularHeightSize = component.WidthSize;   // WidthSize = CircularHeight in vertical
                }
                else if (shape == "Rectangular" && orientation == "Vertical")
                {
                    // Swap back — was stored as Length=HeightSize, HeightSize=Length
                    var temp = component.Length;
                    component.Length = component.HeightSize;
                    component.HeightSize = temp;
                }

                EnquiryListViewModel CustomerDetails = _repo.GetCustomer(enquiry.CustomerID);

                var motorDetails = _repo.GetMotorDetailsByEnquiryId(id) ?? new MotorDetails();
                var vm = new EnquiryFormViewModel
                {
                    Enquiry = enquiry,
                    Component = component,
                    MotorDetails = motorDetails,
                    customer = CustomerDetails,
                    SalesEngineers = _repo.GetSalesEngineersDropdown()
                };

                return View("Create", vm);
            }
            catch (Exception ex)
            {
                _Ex.SaveException("EnquiryController", "Edit", ex.Message, userId);
                return RedirectToAction(nameof(Index));
            }
        }

        // ── POST /Enquiry/Edit/5 ──────────────────────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(EnquiryFormViewModel vm,
                                               IFormFile? imageFile)
        {
            var userId = HttpContext.Session.GetInt32("UserId") ?? 0;
            try
            {
                if (!string.IsNullOrWhiteSpace(vm.Enquiry.SalesNO)
                && _repo.SalesNoExists(vm.Enquiry.SalesNO, vm.Enquiry.EnquiryID))
                {
                    ModelState.AddModelError("Enquiry.SalesNO",
                        "This Enquiry No already exists.");
                }

                if (imageFile != null && imageFile.Length > 0)
                    vm.Component.Image_Path = await SaveFile(imageFile);


                vm.Enquiry.ModifiedBy = userId;
                vm.Enquiry.FilterTypes = vm.Enquiry.FilterTypes == null ? "" : vm.Enquiry.FilterTypes;
                vm.Component.ModifiedBy = userId;

                _repo.Update(vm.Enquiry, vm.Component, vm.MotorDetails);

                TempData["Success"] = $"Enquiry {vm.Enquiry.SalesNO} updated successfully.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _Ex.SaveException("EnquiryController", "Edit POST", ex.Message, userId);
                return RedirectToAction(nameof(Index));
            }
        }

        // ── POST /Enquiry/Delete/5 ────────────────────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Delete(int id)
        {
            var userId = HttpContext.Session.GetInt32("UserId") ?? 0;
            try
            {

                _repo.Delete(id, userId);
                TempData["Success"] = "Enquiry deleted successfully.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _Ex.SaveException("EnquiryController", "Delete", ex.Message, userId);
                return RedirectToAction(nameof(Index));
            }
        }

        // ── AJAX: company autocomplete ────────────────────────────
        [HttpGet]
        public JsonResult SearchCompanies(string term)
        {
            var results = _repo.SearchCompanyNames(term ?? "");
            return Json(results);
        }

        // ── AJAX: get company info when company name selected ─────
        [HttpPost]
        public JsonResult SearchByCompanyName(string companyName)
        {
            var info = _repo.GetCompanyInfo(companyName);
            if (info == null) return Json(new { });

            return Json(new
            {
                customerId = info.CustomerID,
                customerAddress = info.CustomerAddress,
                contactperson = info.ContactPerson,
                designation = info.Designation,
                mobileNo = info.MobileNo,
                emailId = info.EmailId
            });
        }

        // ── AJAX: validate SalesNO ────────────────────────────────
        [HttpGet]
        public JsonResult CheckSalesNo(string salesNo, int? enquiryId)
        {
            var exists = _repo.SalesNoExists(salesNo, enquiryId);
            return Json(new { exists });
        }

        // ─────────────────────────────────────────────────────────
        // FILE UPLOAD HELPER
        // ─────────────────────────────────────────────────────────
        private async Task<string> SaveFile(IFormFile file)
        {
            var uploadsFolder = Path.Combine(_env.WebRootPath, "uploads", "enquiry");
            Directory.CreateDirectory(uploadsFolder);

            var uniqueName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
            var filePath = Path.Combine(uploadsFolder, uniqueName);

            using var stream = new FileStream(filePath, FileMode.Create);
            await file.CopyToAsync(stream);

            return "/uploads/enquiry/" + uniqueName;
        }

        #region FETCH Equiry FROM ERP

        // ── GET /Enquiry/EnquiryFromERP ─────────────────────────────
        public async Task<IActionResult> EnquiryFromERP(DateTime? startDate = null, DateTime? endDate = null)
        {
            var userId = HttpContext.Session.GetInt32("UserId") ?? 0;
            try
            {
                List<FrappeOpportunityData> opportunities;

                if (startDate.HasValue && endDate.HasValue)
                {
                    // ── Date range filter applied ──
                    opportunities = await _frappeService.GetOpportunitiesByDateRangeAsync(startDate.Value, endDate.Value);
                }
                else
                {
                    // ── Existing default behavior — UNCHANGED ──
                    opportunities = await _frappeService.GetLatestOpportunitiesAsync();
                    //opportunities = await _frappeService.GetAllOpportunitiesAsync();
                }

                // ── Attach locally saved EnquiryDate (if this opportunity was already imported) ──
                var salesNos = opportunities.Select(o => o.name).ToList();
                var enquiryDates = _repo.GetEnquiryDatesBySalesNos(salesNos);

                foreach (var opp in opportunities)
                {
                    if (enquiryDates.TryGetValue(opp.name, out var localDate))
                        opp.SubmittedEnquiryDate = localDate;
                }

                // Pass selected dates back to view so the inputs stay filled after filtering
                ViewBag.StartDate = startDate?.ToString("yyyy-MM-dd");
                ViewBag.EndDate = endDate?.ToString("yyyy-MM-dd");

                return View(opportunities);
            }
            catch (Exception ex)
            {
                _Ex.SaveException("EnquiryController", "EnquiryFromERP", ex.Message, userId);
                TempData["Error"] = "Failed to fetch enquiries from ERP.";
                return RedirectToAction(nameof(Index));
            }
        }
        #endregion
    }
}
