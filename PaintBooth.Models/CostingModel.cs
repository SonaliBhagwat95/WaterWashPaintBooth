using PaintBooth.Database;
using PaintBooth.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PaintBooth.Models
{
    public class CostingModel
    {
        public string  EnquiryCode { get; set; }
        public List<PaintBoothModel> DetailsList { get; set; } = new List<PaintBoothModel>();
        public List<PaintBoothModel> FilteredDetailsList { get; set; }
        public List<ExhaustDuctingModel> ExhaustDuctingList { get; set; }
        public string DisplayPanelPosition { get; set; }
        public List<PaintBoothModel> metalBaffles { get; set; }
        public double RMWeightOfPanels { get; set; }
        public double TotalWeightOfFrame { get; set; }
        public double TotalWeight { get; set; }
        public double TotalPriceofPanels { get; set; }
        public double TotalPriceofFrame { get; set; }
        public double TotalCostOfRawMaterials { get; set; }
        public double AddRMWeight { get; set; }
        public double AddRMCost { get; set; }

        public List<MaterialModel> Materials { get; set; }
        public List<CostingAdditionalRM> AdditionalRMList { get; set; }
     = new List<CostingAdditionalRM>();

        public bool IsInFilteredList(PaintBoothModel panel)
        {
            return FilteredDetailsList.Any(f => f.EnquiryId == panel.EnquiryId &&
                                                f.PanelPosition == panel.PanelPosition &&
                                                //f.EqualPanelWidthForD == panel.EqualPanelWidthForD &&
                                                f.EqualPanelWidthByH == panel.EqualPanelWidthByH &&
                                                f.SheetThickness == panel.SheetThickness &&
                                                f.NoofPanels == panel.NoofPanels &&
                                                f.PanelWeight == panel.PanelWeight &&
                                                f.Section==panel.Section);
        }
    }

    public class RawMaterialsValues
    {
        public double RMWeightOfPanels { get; set; }
        public double TotalPriceofPanels { get; set; }
        public double TotalWeightOfFrame { get; set; }
        public double TotalPriceofFrame { get; set; }
        public double TotalCostOfRawMaterials { get; set; }
        public double TotalWeight { get; set; }
        public double AdditionalRMWeight { get; set; }
        public double AdditionalRMCost { get; set; }
    }

    public class CostingAdditionalRMModel
    {
        public int Id { get; set; }
        public int EnquiryId { get; set; }
        public string Description { get; set; }
        public decimal Qty { get; set; }
        public decimal UnitWeight { get; set; }
        public decimal TotalWeight { get; set; }
        public decimal Rate { get; set; }
        public decimal Amount { get; set; }
    }
}
