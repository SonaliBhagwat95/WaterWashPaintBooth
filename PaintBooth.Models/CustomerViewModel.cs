using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace PaintBooth.Models
{ 
    // ── Single contact person row ─────────────────────────────────
    public class ContactPersonViewModel
    {
        public int    ContactId     { get; set; }

        [StringLength(100)]
        public string ContactPerson { get; set; } = string.Empty;

        [StringLength(100)]
        public string Designation   { get; set; } = string.Empty;

        [StringLength(20)]
        public string Mobile        { get; set; } = string.Empty;

        [StringLength(150)]
        [EmailAddress(ErrorMessage = "Invalid email.")]
        public string EmailId       { get; set; } = string.Empty;

        [StringLength(20)]
        public string PAN           { get; set; } = string.Empty;
    }

    // ── Create / Edit form ────────────────────────────────────────
    public class CustomerFormViewModel
    {
        public int CustomerID { get; set; }

        [Required(ErrorMessage = "Company name is required.")]
        [Display(Name = "Company Name")]
        [StringLength(150)]
        public string CompanyName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Address is required.")]
        [Display(Name = "Company Address")]
        [StringLength(300)]
        public string CustomerAddress { get; set; } = string.Empty;

        [Required(ErrorMessage = "Designation is required.")]
        [StringLength(100)]
        public string Designation { get; set; } = string.Empty;

        [Required(ErrorMessage = "Please select a state.")]
        public int StateId { get; set; }

        public int CityId { get; set; }

        [Required(ErrorMessage = "PIN code is required.")]
        [Range(100000, 999999, ErrorMessage = "Enter a valid 6-digit PIN.")]
        public int Pin { get; set; }
        public string Email { get; set; }

        // Multiple contacts
        public List<ContactPersonViewModel> ContactPersons { get; set; }
            = new List<ContactPersonViewModel> { new ContactPersonViewModel() };

        // Dropdowns
        public IEnumerable<SelectListItem> StateList { get; set; }
            = Enumerable.Empty<SelectListItem>();

        public IEnumerable<SelectListItem> CityList { get; set; }
            = Enumerable.Empty<SelectListItem>();
    }

    // ── Index list row ────────────────────────────────────────────
    // NOTE: ContactPerson and Mobile are now in CustomerContactPerson
    // table (multiple contacts). Only ContactCount shown in list.
    public class CustomerListViewModel
    {
        public int    CustomerID      { get; set; }
        public string CompanyName     { get; set; } = string.Empty;
        public string Email { get; set; }
        public string CustomerAddress { get; set; } = string.Empty;
        public string StateName       { get; set; } = string.Empty;
        public string CityName        { get; set; } = string.Empty;
        public int    Pin             { get; set; }
        public int    ContactCount    { get; set; }
        public bool   IsActive        { get; set; }
    }
}
