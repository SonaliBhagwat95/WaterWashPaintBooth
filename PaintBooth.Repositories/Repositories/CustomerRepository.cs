using Microsoft.AspNetCore.Mvc.Rendering;
using PaintBooth.Database;
using PaintBooth.Models;
using PaintBooth.Repositories.Interface;

namespace PaintBooth.Repositories
{
    public class CustomerRepository(AppDbContext _db) : ICustomerRepository
    {
        //private readonly ;

        //public CustomerRepository(AppDbContext db)
        //{
        //    _db = db;
        //}

        // ── Get all active customers ──────────────────────────────
        public List<CustomerListViewModel> GetAll()
        {
            return _db.CustomerMasters
                      .Where(x => x.IsActive == true)
                      .OrderByDescending(x => x.CreatedDate)
                      .Select(x => new CustomerListViewModel
                      {
                          CustomerID      = x.CustomerID,
                          CompanyName     = x.CompanyName,
                          Email= x.EmailId,
                          CustomerAddress = x.CustomerAddress,
                          StateName       = x.State != null ? x.State.StateName : "",
                          CityName        = x.City  != null ? x.City.CityName   : "",
                          Pin             = x.Pin,
                          ContactCount    = _db.CustomerContactPersons
                                              .Count(c => c.CustomerID == x.CustomerID
                                                       && c.IsActive   == true),
                          IsActive        = x.IsActive
                      })
                      .ToList();
        }

        // ── Get single customer ───────────────────────────────────
        public CustomerMaster? GetById(int customerId)
        {
            return _db.CustomerMasters
                      .Where(x => x.CustomerID == customerId
                               && x.IsActive   == true)
                      .FirstOrDefault();
        }

        // ── Add new customer — returns new CustomerID ─────────────
        public int Add(CustomerMaster customer)
        {

            customer.CreatedDate = DateTime.Now;
            customer.IsActive    = true;

            _db.CustomerMasters.Add(customer);
            _db.SaveChanges();

            return customer.CustomerID;
        }

        // ── Update customer ───────────────────────────────────────
        public void Update(CustomerMaster customer)
        {
            var existing = _db.CustomerMasters
                              .Where(x => x.CustomerID == customer.CustomerID)
                              .FirstOrDefault();

            if (existing == null) return;

            existing.CompanyName     = customer.CompanyName;
            existing.EmailId         = customer.EmailId;
            existing.CustomerAddress = customer.CustomerAddress;
            existing.Designation     = customer.Designation;
            existing.StateId         = customer.StateId;
            existing.CityId          = customer.CityId;
            existing.Pin             = customer.Pin;
            existing.ModifiedBy      = customer.ModifiedBy;
            existing.ModifiedDate    = DateTime.Now;

            _db.SaveChanges();
        }

        // ── Soft delete customer ──────────────────────────────────
        public void Delete(int customerId, int deletedBy)
        {
            var customer = _db.CustomerMasters
                              .Where(x => x.CustomerID == customerId)
                              .FirstOrDefault();

            if (customer == null) return;

            customer.IsActive     = false;
            customer.ModifiedBy   = deletedBy;
            customer.ModifiedDate = DateTime.Now;

            _db.SaveChanges();
        }

        // ── Get all contact persons for a customer ────────────────
        public List<CustomerContactPerson> GetContacts(int customerId)
        {
            return _db.CustomerContactPersons
                      .Where(x => x.CustomerID == customerId
                               && x.IsActive   == true)
                      .ToList();
        }

        // ── Save contact persons ──────────────────────────────────
        // Replaces all existing contacts with the new submitted list
        public void SaveContacts(int customerId, List<ContactPersonViewModel> contacts)
        {
            // Step 1 — soft delete all existing contacts for this customer
            var existing = _db.CustomerContactPersons
                              .Where(x => x.CustomerID == customerId)
                              .ToList();

            existing.ForEach(x => x.IsActive = false);

            // Step 2 — add new contacts (skip empty rows)
            foreach (var c in contacts)
            {
                // Skip rows where all fields are empty
                bool isEmpty = string.IsNullOrWhiteSpace(c.ContactPerson)
                            && string.IsNullOrWhiteSpace(c.Mobile)
                            && string.IsNullOrWhiteSpace(c.EmailId);

                if (isEmpty) continue;

                _db.CustomerContactPersons.Add(new CustomerContactPerson
                {
                    CustomerID    = customerId,
                    ContactPerson = c.ContactPerson?.Trim() ?? "",
                    Designation   = c.Designation?.Trim()   ?? "",
                    Mobile        = c.Mobile?.Trim()         ?? "",
                    EmailId       = c.EmailId?.Trim()        ?? "",
                    PAN           = c.PAN?.Trim()            ?? "",
                    IsActive      = true
                });
            }

            _db.SaveChanges();
        }

        // ── Delete single contact person ──────────────────────────
        public void DeleteContact(int contactId)
        {
            var contact = _db.CustomerContactPersons
                             .Where(x => x.ContactId == contactId)
                             .FirstOrDefault();

            if (contact == null) return;

            contact.IsActive = false;
            _db.SaveChanges();
        }

        // ── State dropdown ────────────────────────────────────────
        public List<SelectListItem> GetStateDropdown()
        {
            return _db.StateMasters
                      .Where(x => x.IsActive == true)
                      .OrderBy(x => x.StateName)
                      .Select(x => new SelectListItem
                      {
                          Value = x.StateId.ToString(),
                          Text  = x.StateName
                      })
                      .ToList();
        }

        // ── City dropdown by state ────────────────────────────────
        public List<SelectListItem> GetCityDropdown(int stateId)
        {
            return _db.CityMasters
                      .Where(x => x.StateId  == stateId
                               && x.IsActive == true)
                      .OrderBy(x => x.CityName)
                      .Select(x => new SelectListItem
                      {
                          Value = x.CityId.ToString(),
                          Text  = x.CityName
                      })
                      .ToList();
        }

        // ── Check duplicate company name ──────────────────────────
        public bool CompanyNameExists(string companyName, int? excludeId = null)
        {
            return _db.CustomerMasters
                      .Where(x => x.CompanyName == companyName
                               && x.IsActive    == true
                               && (excludeId    == null
                                   || x.CustomerID != excludeId))
                      .Any();
        }
    }
}
