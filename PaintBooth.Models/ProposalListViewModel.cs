using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PaintBooth.Models
{
    public class ProposalListViewModel
    {
        public string SalesNO { get; set; } = string.Empty;
        public string CompanyName { get; set; } = string.Empty;
        public string ContactPerson { get; set; } = string.Empty;
        public decimal POVALUE { get; set; }
        public DateTime? CreatedDate { get; set; }
        public int RevisionNo { get; set; } = 0;

    }

}
