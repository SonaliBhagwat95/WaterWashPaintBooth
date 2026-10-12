namespace PaintBooth.Models
{
    // ── Index list ────────────────────────────────────────────────
    public class PaintBoothListViewModel
    {
        public int    PaintBoothID { get; set; }
        public string SalesNO     { get; set; } = string.Empty;
        public decimal W          { get; set; }
        public decimal H          { get; set; }
        public decimal D          { get; set; }
        public string? BlowerItemCode { get; set; }
        public string? MotorItemCode  { get; set; }
    }

    // ── Enquiry search result (loaded when Search clicked) ────────
    public class EnquirySearchResultViewModel
    {
        public double Length                  { get; set; }  // Depth
        public double Width                   { get; set; }
        public double Height                  { get; set; }
        public string? ExtractionChamberHeight { get; set; }
        public string? MotorTypes              { get; set; }
        public decimal PanelWidth              { get; set; }
        public decimal PanelHeight             { get; set; }
        public string? SubTypeOfDraftType { get; set; }//paintbooth type like open /filters on Top
        public string? Category { get; set; }
    }

    // ── Motor detail row ──────────────────────────────────────────
    public class MotorDetailViewModel
    {
        public int     MotorCatalogID    { get; set; }
        public decimal RatedOutputHP     { get; set; }
        public string  FrameSize         { get; set; } = string.Empty;
        public string  TypeReference     { get; set; } = string.Empty;
        public string  RatedSpeed        { get; set; } = string.Empty;
        public string  RatedCurrent      { get; set; } = string.Empty;
        public string  EfficiencyFullLoad{ get; set; } = string.Empty;
    }

    // ── Pressure drop row ─────────────────────────────────────────
    public class PressureDropViewModel
    {
        public int    ItemNumber    { get; set; }
        public string Description   { get; set; } = string.Empty;
        public decimal PressureDrop_mm { get; set; }
    }
}
