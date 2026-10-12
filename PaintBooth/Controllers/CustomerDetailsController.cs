

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PaintBooth.Database;
using PaintBooth.Models;
using PaintBooth.Repositories.Interface;

namespace PaintBooth.Controllers
{
    [Authorize]
    public class CustomerDetailsController(ICustomerRepository _repo,IExceptionRepository _Ex) : Controller
    {
        // Global userId property — reads from session on every access
        private int userId => HttpContext.Session.GetInt32("UserId") ?? 0;
        // ── GET /CustomerDetails/Index ────────────────────────────
        public IActionResult Index()
        {
           // var userId = HttpContext.Session.GetInt32("UserId") ?? 0;
            try
            {
                var list = _repo.GetAll();
                return View(list);
            }
            catch (Exception ex)
            {
                _Ex.SaveException("CustomerDetailsController", "Index", ex.Message, userId);
                return View();
            }
        }

        // ── GET /CustomerDetails/Create ───────────────────────────
        public IActionResult Create()
        {
            var userId = HttpContext.Session.GetInt32("UserId") ?? 0;
            try
            {
                var model = new CustomerFormViewModel
                {
                    StateList = _repo.GetStateDropdown()
                };
                return View(model);
            }
            catch (Exception ex)
            {
                _Ex.SaveException("CustomerDetailsController", "Index", ex.Message, userId);
                // Option 2: Redirect to Index
                TempData["Error"] = "Something went wrong.";
                return RedirectToAction("Index");
            }
        }

        // ── POST /CustomerDetails/Create ──────────────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(CustomerFormViewModel model)
        {
            try
            {
                //if (!ModelState.IsValid)
                //{
                    model.StateList = _repo.GetStateDropdown();
                    model.CityList = _repo.GetCityDropdown(model.StateId);
                //    return View(model);
                //}

                if (_repo.CompanyNameExists(model.CompanyName))
                {
                    ModelState.AddModelError("CompanyName", "This company name already exists.");
                    model.StateList = _repo.GetStateDropdown();
                    model.CityList = _repo.GetCityDropdown(model.StateId);
                    return View(model);
                }

                // Save customer — get new CustomerID back
                var customer = MapToEntity(model);
                customer.CreatedBy = HttpContext.Session.GetInt32("UserId");
                int newId = _repo.Add(customer);

                // Save all contact persons linked to new CustomerID
                if (model.ContactPersons != null && model.ContactPersons.Any())
                    _repo.SaveContacts(newId, model.ContactPersons);

                TempData["Success"] = $"{model.CompanyName} added successfully.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {

                _Ex.SaveException("CustomerDetailsController", "Create", ex.Message, userId);
                TempData["Error"] = "Something went wrong while saving. Please try again.";
                model.StateList = _repo.GetStateDropdown();
                model.CityList = _repo.GetCityDropdown(model.StateId);
                return View(model);
            }
        }

        // ── GET /CustomerDetails/Edit/5 ───────────────────────────
        public IActionResult Edit(int id)
        {
            try
            {
                var customer = _repo.GetById(id);
                if (customer == null) return NotFound();

                var model = MapToViewModel(customer);
                model.StateList = _repo.GetStateDropdown();
                model.CityList = _repo.GetCityDropdown(customer.StateId);

                // Load existing contact persons
                var contacts = _repo.GetContacts(id);
                model.ContactPersons = contacts.Select(c => new ContactPersonViewModel
                {
                    ContactId = c.ContactId,
                    ContactPerson = c.ContactPerson,
                    Designation = c.Designation,
                    Mobile = c.Mobile,
                    EmailId = c.EmailId,
                    PAN = c.PAN
                }).ToList();

                // Always have at least 1 empty row
                if (!model.ContactPersons.Any())
                    model.ContactPersons.Add(new ContactPersonViewModel());

                return View(model);
            }
            catch (Exception ex)
            {
                _Ex.SaveException("CustomerDetailsController", "Edit_Get", ex.Message, userId);
                TempData["Error"] = "Something went wrong while loading the form.";
                return RedirectToAction(nameof(Index));
            }
        }

        // ── POST /CustomerDetails/Edit/5 ──────────────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(CustomerFormViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    model.StateList = _repo.GetStateDropdown();
                    model.CityList = _repo.GetCityDropdown(model.StateId);
                    return View(model);
                }

                if (_repo.CompanyNameExists(model.CompanyName, model.CustomerID))
                {
                    ModelState.AddModelError("CompanyName", "This company name already exists.");
                    model.StateList = _repo.GetStateDropdown();
                    model.CityList = _repo.GetCityDropdown(model.StateId);
                    return View(model);
                }

                var customer = MapToEntity(model);
                customer.ModifiedBy = HttpContext.Session.GetInt32("UserId");
                _repo.Update(customer);

                // Replace all contact persons
                if (model.ContactPersons != null)
                    _repo.SaveContacts(model.CustomerID, model.ContactPersons);

                TempData["Success"] = $"{model.CompanyName} updated successfully.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {

                _Ex.SaveException("CustomerDetailsController", "Edit_Post", ex.Message, userId);
                TempData["Error"] = "Something went wrong while loading the form.";
                return RedirectToAction(nameof(Index));
            }
        }

        // ── POST /CustomerDetails/Delete/5 ────────────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Delete(int id)
        {
            try
            {
                var userId = HttpContext.Session.GetInt32("UserId") ?? 0;
                _repo.Delete(id, userId);
                TempData["Success"] = "Customer deleted successfully.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _Ex.SaveException("CustomerDetailsController", "Delete", ex.Message, userId);
                TempData["Error"] = "Something went wrong";
                return RedirectToAction(nameof(Index));
            }
        }

        // ── AJAX: cities by state ─────────────────────────────────
        [HttpGet]
        public JsonResult GetCitiesByState(int stateId)
        {
            var cities = _repo.GetCityDropdown(stateId);
            return Json(cities);
        }

        // ─────────────────────────────────────────────────────────
        // HELPERS
        // ─────────────────────────────────────────────────────────

        private CustomerMaster MapToEntity(CustomerFormViewModel m)
        {
            return new CustomerMaster
            {
                CustomerID      = m.CustomerID,
                CompanyName     = m.CompanyName.Trim(),
                EmailId=m.Email,
                CustomerAddress = m.CustomerAddress.Trim(),
                Designation     = m.Designation.Trim(),
                StateId         = m.StateId,
                CityId          = m.CityId,
                Pin             = m.Pin,
                IsActive        = true
            };
        }

        private CustomerFormViewModel MapToViewModel(CustomerMaster c)
        {
            return new CustomerFormViewModel
            {
                CustomerID      = c.CustomerID,
                CompanyName     = c.CompanyName,
                Email           =c.EmailId,
                CustomerAddress = c.CustomerAddress,
                Designation     = c.Designation,
                StateId         = c.StateId,
                CityId          = c.CityId,
                Pin             = c.Pin
            };
        }
    }
}
