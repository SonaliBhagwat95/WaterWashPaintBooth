using PaintBooth.Database;
using PaintBooth.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PaintBooth.Models
{
    public class BOCalculationModel
    {
        public string EnquiryCode { get; set; }
        public decimal TotalPriceOfFrame { get; set; }
        public decimal TotalPriceOfPanels { get; set; }
        public decimal RowMaterials { get; set; }
        public decimal BoughtOut { get; set; }
        public bool IsBOApproved { get; set; }
        public string? ItemRatesJson { get; set; }
        public double AddBOWeight { get; set; }
        public double AddBOCost { get; set; }
        public List<MaterialModel> Materials { get; set; }
        public List <MaterialModel> FrameDetails { get; set; }
        public List <PaintBoothModel> PaintBooth { get; set; }
        public List <SettingItemViewModel> TubeLightDetails { get; set; }
        public List<ExhaustDuctingModel> ExhaustDuctDetails { get; set; }
        public List<BOItemRate> SavedRates { get; set; } = new();
        public List<BOAdditionalItems> AdditionalBOList { get; set; } = new List<BOAdditionalItems>();

    }
}
