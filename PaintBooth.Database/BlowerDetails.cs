using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PaintBooth.Database
{
    public class BlowerDetails
    {
        [Key]
        public int BlowerID { get; set; }
        [ForeignKey("EnquiryID")]
        public int? EnquiryID { get; set; }
        public string? BlowerItemCode { get; set; }
        public decimal? CapacityCublicMperHr { get; set; }
        public decimal? RoundingUpCapacity { get; set; }
        public string? MountingTypes { get; set; }
        public string DriveTypes { get; set; }
        public decimal WaterColumn { get; set; }
        public decimal BlowerHpCalculation { get; set; }
        public DateTime? CreatedDate { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public bool IsActive { get; set; }
    }
}
