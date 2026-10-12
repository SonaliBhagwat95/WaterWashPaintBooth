using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PaintBooth.Model
{
    public class BendSectionModel
    {
        public int BendId { get; set; }
      
        public decimal W { get; set; }
        public decimal H { get; set; }
        public decimal T { get; set; }
        public decimal L { get; set; }
        public decimal Length { get; set; }
        public bool chkSlots { get; set; }
        public string SlotLocation { get; set;}
        public string SectionName { get; set; }
        public double PitchDistance { get; set; }
        public string SlotDimentions { get; set; }
        public int? CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public int? ModifiedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public bool IsDeleted { get; set; }
        public decimal L1 { get; set; }
        public int Quantity { get; set; }
        public string Materials { get; set; }
        public bool AddSlotchk { get; set; }
    }

    public class SlotDetail
    {
        public string SlotLocation { get; set; }
        public decimal PitchDistance { get; set; }
        public string SlotDimensions { get; set; }
    }
}
