using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PaintBooth.Database
{
    public class ProposalBasisOfDesign
    {
        [Key]
        public int BODID { get; set; }
        public int EnquiryID { get; set; }
        public string RowKey { get; set; }
        public string? Description { get; set; }
        public string? Value { get; set; }
        public bool IsIncluded { get; set; } = true;
        public bool IsCustom { get; set; }
        public int SortOrder { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime CreatedDate { get; set; } = DateTime.Now;
    }
}
