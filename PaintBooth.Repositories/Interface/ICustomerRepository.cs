
using Microsoft.AspNetCore.Mvc.Rendering;
using PaintBooth.Database;

using PaintBooth.Models;

namespace PaintBooth.Repositories.Interface
{
    public interface ICustomerRepository
    {
        // ── Customer CRUD ─────────────────────────────────────────
        List<CustomerListViewModel> GetAll();
        CustomerMaster?             GetById(int customerId);
        int                         Add(CustomerMaster customer);
        void                        Update(CustomerMaster customer);
        void                        Delete(int customerId, int deletedBy);

        // ── Contact Persons ───────────────────────────────────────
        List<CustomerContactPerson> GetContacts(int customerId);
        void                        SaveContacts(int customerId, List<ContactPersonViewModel> contacts);
        void                        DeleteContact(int contactId);

        // ── Dropdowns ─────────────────────────────────────────────
        List<SelectListItem> GetStateDropdown();
        List<SelectListItem> GetCityDropdown(int stateId);

        // ── Validation ────────────────────────────────────────────
        bool CompanyNameExists(string companyName, int? excludeId = null);
       
       
    }
}
