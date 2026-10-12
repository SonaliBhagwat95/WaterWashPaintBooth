using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PaintBooth.Models
{
    public class PriceBIDListViewModel
    {
        public string SalesNO { get; set; } = string.Empty;
        public string CompanyName { get; set; } = string.Empty;
        public decimal POVALUE { get; set; }
        public decimal BestPrice { get; set; }
        public bool ConfirmPriceBID { get; set; }

    }
}
