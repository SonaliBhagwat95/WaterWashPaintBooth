namespace PaintBooth.Models
{
    // ── Used by Costing/Index grid ────────────────────────────────
    public class CostingListViewModel
    {
        public int      EnquiryID             { get; set; }
        public string   SalesNO               { get; set; } = string.Empty;
        public string   CompanyName           { get; set; } = string.Empty;
        public decimal  TotalCostOfPanels     { get; set; }
        public decimal  TotalCostOfFrames     { get; set; }
        public decimal  TotalCostOfRM         { get; set; }
        public DateTime? CreatedDate          { get; set; }
    }
}
