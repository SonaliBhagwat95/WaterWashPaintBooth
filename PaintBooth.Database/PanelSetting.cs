using System.ComponentModel.DataAnnotations;

namespace PaintBooth.Database
{
    public class PanelSetting
    {
        [Key]
        public int SId { get; set; }

        // ── Enquiry link ──────────────────────────────────────────
        public string SalesNo { get; set; } = string.Empty;

        // ── Setting Type ──────────────────────────────────────────
        // Paintbooth or Extraction Chamber
        public string SettingType { get; set; } = string.Empty;

        // ── Panel ─────────────────────────────────────────────────
        public double? PanelWidth { get; set; }
        public double? PanelHeight { get; set; }
        public double? SheetThickness { get; set; }

        // ── Section ───────────────────────────────────────────────
        public string? Section { get; set; }
        public string? BendStructural { get; set; }
        public double? StandardBend1 { get; set; }
        public double? StandardBend2 { get; set; }
        public double? PitchDistance { get; set; }
        public string? SlotDimensions { get; set; }
        public int H { get; set; }
        public int W { get; set; }
        public int T { get; set; }
        public string? WaterCurtain { get; set; }

        // ── Material ──────────────────────────────────────────────
        public string? Materials { get; set; }

        // ── Lighting ──────────────────────────────────────────────
        public string? LightTypes { get; set; }
        public string? LightSubTypes { get; set; }
        public double? LuxLevel { get; set; }
        public double? Lumens { get; set; }

        // ── Audit ─────────────────────────────────────────────────
        public int? CreatedBy { get; set; }
        public DateTime? CreatedDate { get; set; }
        public int? ModifiedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public bool IsActive { get; set; } = true;


        // ── NEW: Item Code + Rate (auto-fetched from ERP Item API) by Ajay ──
        public string? ItemCode { get; set; }
        public decimal? Rate { get; set; }
        public string? ItemCodeForStructuralM { get; set; }
        public decimal? RateForStructuralM { get; set; }
    }
}
