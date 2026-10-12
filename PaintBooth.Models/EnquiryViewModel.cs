

using Microsoft.AspNetCore.Mvc.Rendering;
using PaintBooth.Database;

namespace PaintBooth.Models
{
    public class EnquiryFormViewModel
    {
        public EnquiryMaster Enquiry   { get; set; } = new EnquiryMaster();
        public ComponentTable Component { get; set; } = new ComponentTable();
        public MotorDetails MotorDetails { get; set; }= new MotorDetails();
        public EnquiryListViewModel customer { get; set; }
        public List<SelectListItem> SalesEngineers { get; set; } = new();
    }

    public class EnquiryListViewModel
    {
        public int EnquiryID { get; set; }
        public string SalesNO { get; set; } = string.Empty;
        public string CompanyName { get; set; } = string.Empty;
        public string Component { get; set; } = string.Empty;
        public string DraftType { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public DateTime EnquiryDate { get; set; }
        public string? Stage { get; set; }
        public bool IsActive { get; set; }
        //Company details

        public string Address { get; set; }
        public string ContactPerson { get; set; }
        public string Designation { get; set; }
        public string MobileNo { get; set; }
        public string Email { get; set; }

        
        public string? CustomerLocation { get; set; }   // NEW — holds ERP customer_location

    }

    public class CompanyInfoViewModel
    {
        public int     CustomerID      { get; set; }
        public string  CompanyName     { get; set; } = string.Empty;
        public string? CustomerAddress { get; set; }
        public string  ContactPerson   { get; set; } = string.Empty;
        public string? Designation     { get; set; }
        public string  MobileNo        { get; set; } = string.Empty;
        public string  EmailId         { get; set; } = string.Empty;
    }
}
