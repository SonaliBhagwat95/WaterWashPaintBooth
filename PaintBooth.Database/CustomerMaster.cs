
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PaintBooth.Database
{
    public class CustomerMaster
    {
        [Key]
        public int CustomerID { get; set; }

        [Required(ErrorMessage = "Company name is required.")]
        [StringLength(150)]
        public string CompanyName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Address is required.")]
        [StringLength(300)]
        public string CustomerAddress { get; set; } = string.Empty;

        [Required(ErrorMessage = "Designation is required.")]
        [StringLength(100)]
        public string Designation { get; set; } = string.Empty;

        // ── Location ──────────────────────────────────────────────
        [ForeignKey("StateId")]
        public int StateId { get; set; }

        [ForeignKey("CityId")]
        public int CityId { get; set; }

        public int Pin { get; set; }

        // ── Contact Person ────────────────────────────────────────
        [StringLength(100)]
        public string? ContactPerson { get; set; }

        [StringLength(20)]
        public string? Mobile { get; set; }

        [StringLength(150)]
        [EmailAddress]
        public string? EmailId { get; set; }

        [StringLength(20)]
        public string? PAN { get; set; }

        // ── Status — IsActive replaces IsDeleted ──────────────────
        // IsActive = true  → customer is active   (show in lists)
        // IsActive = false → customer is deleted  (soft delete)
        public bool IsActive { get; set; } = true;

        // ── Audit ─────────────────────────────────────────────────
        public int?      CreatedBy    { get; set; }
        public DateTime? CreatedDate  { get; set; } = DateTime.UtcNow;
        public int?      ModifiedBy   { get; set; }
        public DateTime? ModifiedDate { get; set; }

        // ── Navigation ────────────────────────────────────────────
        public StateMaster?  State { get; set; }
        public CityMaster?   City  { get; set; }
    }
}
