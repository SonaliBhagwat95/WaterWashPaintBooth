using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PaintBooth.Database
{

    [Table("ductaccessorydetails")]
    public class DuctAccessoryDetails
    {
        [Key]
        public int AccessoryID { get; set; }
        public int EnquiryID { get; set; }
        public string? SalesNo { get; set; }
        public string AccessoryType { get; set; } = "";
        public string? Description { get; set; }
        public int Quantity { get; set; }
        [Column(TypeName = "decimal(12,3)")]
        public decimal Weight { get; set; }
        public string? Material { get; set; }
        public bool CostingStatus { get; set; } = true;
        public bool IsActive { get; set; } = true;
        public DateTime? CreatedDate { get; set; }
        public DateTime? ModifiedDate { get; set; }
    }
}
