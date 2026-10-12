using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PaintBooth.Models.ERP
{
    public class FrappeItemResponse
    {
        public FrappeItemData? data { get; set; }
    }

    public class FrappeItemData
    {
        public string? item_code { get; set; }
        public string? item_name { get; set; }
        public decimal standard_rate { get; set; }
        public decimal last_purchase_rate { get; set; }
        public decimal valuation_rate { get; set; }
    }
}
