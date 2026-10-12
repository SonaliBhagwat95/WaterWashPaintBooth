using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PaintBooth.Database
{
    public class CustomerContactPerson
    {
        [Key]
        public int ContactId { get; set; }

        [ForeignKey("CustomerID")]
        public int CustomerID { get; set; }

        [StringLength(100)]
        public string ContactPerson { get; set; } = string.Empty;

        [StringLength(100)]
        public string Designation { get; set; } = string.Empty;

        [StringLength(20)]
        public string Mobile { get; set; } = string.Empty;

        [StringLength(150)]
        public string EmailId { get; set; } = string.Empty;

        [StringLength(20)]
        public string PAN { get; set; } = string.Empty;

        public bool IsActive { get; set; } = true;

        // Navigation
        public CustomerMaster? Customer { get; set; }
    }
}
