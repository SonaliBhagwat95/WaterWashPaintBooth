namespace PaintBooth.Models
{
    public class DashboardViewModel
    {
        // ── Stat Cards ────────────────────────────────────────────
        public int TotalEnquiries         { get; set; }
        public int TotalPaintboothDesigns { get; set; }  // 0 for now
        public int TotalRawMaterialCosts  { get; set; }  // 0 for now
        public int TotalBOCalculations    { get; set; }  // 0 for now
        public int TotalPriceBIDs         { get; set; }  // 0 for now
        public int TotalProjects          { get; set; }  // 0 for now

        // ── Pipeline — based on EnquiryMaster.DraftType ──────────
        public int PipelineEnquiry   { get; set; }
        public int PipelineDesign    { get; set; }
        public int PipelineCosting   { get; set; }
        public int PipelineBID       { get; set; }
        public int PipelineApproval  { get; set; }
        public int PipelineProject   { get; set; }
        public int PipelineDelivered { get; set; }

        // ── Recent Enquiries table ────────────────────────────────
        public List<RecentEnquiryModel> RecentEnquiries { get; set; } = new();

        // ── Pending Approvals — empty till other tables exist ─────
        public List<PendingApprovalModel> PendingApprovals { get; set; } = new();

        // ── Activity Feed — from enquiries for now ────────────────
        public List<ActivityModel> Activities { get; set; } = new();
    }

    public class RecentEnquiryModel
    {
        public int      EnquiryID    { get; set; }
        public string   SalesNO      { get; set; } = string.Empty;
        public string   CustomerName { get; set; } = string.Empty;
        public string   DraftType    { get; set; } = string.Empty;
        public string   SubType      { get; set; } = string.Empty;
        public DateTime EnquiryDate { get; set; }
        public decimal  TotalCostOfRM{ get; set; }
        public bool IsActive { get; set; }
    }

    public class PendingApprovalModel
    {
        public int    ReferenceId   { get; set; }
        public string CustomerName  { get; set; } = string.Empty;
        public string ApprovalType  { get; set; } = string.Empty;
        public string Status        { get; set; } = string.Empty;
    }

    public class ActivityModel
    {
        public string   Message   { get; set; } = string.Empty;
        public string   TimeAgo   { get; set; } = string.Empty;
        public string   Category  { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }
}
