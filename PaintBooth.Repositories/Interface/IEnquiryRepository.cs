

using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using PaintBooth.Database;
using PaintBooth.Models;

namespace PaintBooth.Repositories.Interface
{
    public interface IEnquiryRepository
    {
        // ── Index list ────────────────────────────────────────────
        List<EnquiryListViewModel> GetAll();

        // ── Single — loads both tables ────────────────────────────
        EnquiryMaster? GetById(int enquiryId);
        ComponentTable? GetComponentByEnquiryId(int enquiryId);

        // ── Create — inserts ComponentTable FIRST, then EnquiryMaster
        //    Returns new EnquiryID
        int Add(EnquiryMaster enquiry, ComponentTable component, MotorDetails motor);


        // ── Update — updates both rows
        void Update(EnquiryMaster enquiry, ComponentTable component, MotorDetails motor);

        // ── Soft delete EnquiryMaster (Component stays for reference)
        void Delete(int enquiryId, int deletedBy);

        // ── Duplicate SalesNO check ───────────────────────────────
        bool SalesNoExists(string salesNo, int? excludeId = null);

        // ── Company autocomplete ──────────────────────────────────
        List<string> SearchCompanyNames(string term);
        CompanyInfoViewModel? GetCompanyInfo(string companyName);
        MotorDetails GetMotorDetailsByEnquiryId(int enquiryId);
        EnquiryListViewModel GetCustomer(int custID);
        public List<SelectListItem> GetSalesEngineersDropdown();


        // Method Added  by Ajay 
        int GetOrCreateCustomer(EnquiryListViewModel customerInfo);
        Dictionary<string, DateTime> GetEnquiryDatesBySalesNos(List<string> salesNos);


    }
}
