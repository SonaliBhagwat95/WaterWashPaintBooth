namespace PaintBooth.Models
{
    // ── Index list row ────────────────────────────────────────────
    public class PanelSettingListViewModel
    {
        public string SalesNo          { get; set; } = string.Empty;
        public string PaintBoothThk    { get; set; } = string.Empty;
        public string ECThk            { get; set; } = string.Empty;
        public string PaintBoothMaterial { get; set; } = string.Empty;
        public string ECMaterial       { get; set; } = string.Empty;
    }

    // ── Create form ───────────────────────────────────────────────
    public class PanelSettingFormViewModel
    {
        public string SalesNo { get; set; } = string.Empty;

        // Collection of settings (Paintbooth + Extraction Chamber)
        public List<SettingItemViewModel> Settings { get; set; } = new();
        public string PaintboothType { get; set; }
    }

    // ── Single setting row (Paintbooth or Extraction Chamber) ─────
    public class SettingItemViewModel
    {
        public int? SId { get; set; }
        public string SalesNo { get; set; }
        public string SettingType { get; set; } = string.Empty;
        public double PanelWidth { get; set; }
        public double PanelHeight { get; set; }
        public double SheetThickness { get; set; }
        public string Section { get; set; }
        public string BendStructural { get; set; }
        public string Materials { get; set; } = string.Empty;
        public double StandardBend1 { get; set; }
        public double StandardBend2 { get; set; }
        public double PitchDistance { get; set; }
        public string SlotDimensions { get; set; }
        public string LightTypes { get; set; } = string.Empty;
        public string LightSubTypes { get; set; } = string.Empty;
        public decimal LuxLevel { get; set; }
        public decimal Lumens { get; set; }
        public int H { get; set; }
        public int W { get; set; }
        public int T { get; set; }
        public int Quantity { get; set; }
        public string WaterCurtain { get; set; }


        //Code added by Ajay for BO Calculations
        public string ItemCode { get; set; }
        public decimal Rate { get; set; }
        public string ItemCodeForStructuralM { get; set; }
        public decimal RateForStructuralM { get; set; }
    }
}
