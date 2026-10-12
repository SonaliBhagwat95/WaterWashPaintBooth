using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PaintBooth.Database
{
    public class MotorDetails
    {
        [Key]
        public int MotorID { get; set; }
        [ForeignKey("EnquiryID")]
        public int? EnquiryID { get; set; }
        [ForeignKey("MotorCatalogID")]
        public int? MotorCatalogID { get; set; }
        public bool IsActive { get; set; }
        public string? MotorTypes { get; set; }
        public string? MotorItemCode { get; set; }
    }
}
