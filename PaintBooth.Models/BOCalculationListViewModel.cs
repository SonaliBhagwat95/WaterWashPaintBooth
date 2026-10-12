namespace PaintBooth.Models
{
    // ── Used by BOCalculations/Index grid ─────────────────────────
    public class BOCalculationListViewModel
    {
        public int      EnquiryID           { get; set; }
        public string   SalesNO             { get; set; } = string.Empty;
        public string   CompanyName         { get; set; } = string.Empty;
        public decimal  TotalPriceOfRM      { get; set; }
        public decimal  TotalPriceOfBO      { get; set; }
        public bool     IsPriceBidApproved  { get; set; }
        public DateTime? CreatedDate        { get; set; }
    }
}
