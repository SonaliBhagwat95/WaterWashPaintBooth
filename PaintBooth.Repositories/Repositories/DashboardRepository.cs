


using PaintBooth.Database;
using PaintBooth.Models;
using PaintBooth.Repositories.Interface;

namespace PaintBooth.Repositories
{
    public class DashboardRepository : IDashboardRepository
    {
        private readonly AppDbContext _db;

        public DashboardRepository(AppDbContext db)
        {
            _db = db;
        }

        public DashboardViewModel GetDashboardData()
        {
            var model = new DashboardViewModel();

            // ── Stat Cards ────────────────────────────────────────
            model.TotalEnquiries         = GetTotalEnquiries();
            model.TotalPaintboothDesigns = GetTotalPaintbooth();   // TODO: when PaintboothDesign table is ready
            model.TotalRawMaterialCosts  = GetRawMaterialsCostings();   // TODO: when RawMaterialCosting table is ready
            model.TotalBOCalculations    = GetBOCostings();   // TODO: when BOCalculation table is ready
            model.TotalPriceBIDs         = 0;   // TODO: when PriceBID table is ready
            model.TotalProjects          = 0;   // TODO: when Project table is ready

            // ── Pipeline — grouped by DraftType ───────────────────
            model.PipelineEnquiry   = GetCountByDraftType("Enquiry");
            model.PipelineDesign    = GetCountByDraftType("Design");
            model.PipelineCosting   = GetCountByDraftType("Costing");
            model.PipelineBID       = GetCountByDraftType("BID");
            model.PipelineApproval  = GetCountByDraftType("Approval");
            model.PipelineProject   = GetCountByDraftType("Project");
            model.PipelineDelivered = GetCountByDraftType("Delivered");

            // ── Recent Enquiries ──────────────────────────────────
            model.RecentEnquiries = GetRecentEnquiries();

            // ── Pending Approvals — empty till other tables ───────
            model.PendingApprovals = new List<PendingApprovalModel>();

            // ── Activity Feed — from enquiries only for now ───────
            //model.Activities = GetActivities();

            return model;
        }

        // ─────────────────────────────────────────────────────────
        // Total enquiries count
        // ─────────────────────────────────────────────────────────
        private int GetTotalEnquiries()
        {
            return _db.EnquiryMasters
                      .Where(x => x.IsActive == true)
                      .Count();
        }
        private int GetTotalPaintbooth()
        {
            return _db.PaintBoothDetails
                .Where(x => x.IsActive == true)
                .Count();
        }
        private int GetRawMaterialsCostings()
        {
            return _db.EnquiryMasters
                      .Where(x => x.TotalCostOfRM>0 && x.TotalCostOfPanels>0 && x.TotalCostOfFrames>0)
                      .Count();
        }
        private int GetBOCostings()
        {
            return _db.PriceDetailsTable
                      .Where(x => x.TotalPriceOfBoughtOut > 0)
                      .Count();
        }
        // ─────────────────────────────────────────────────────────
        // Pipeline — count by DraftType column
        // ─────────────────────────────────────────────────────────
        private int GetCountByDraftType(string draftType)
        {
            return _db.EnquiryMasters
                      .Where(x => x.DraftType  == draftType
                               && x.IsActive == true)
                      .Count();
        }

        // ─────────────────────────────────────────────────────────
        // Recent 5 enquiries — newest first
        // ─────────────────────────────────────────────────────────
        private List<RecentEnquiryModel> GetRecentEnquiries()
        {
            return (from e in _db.EnquiryMasters
                    join c in _db.ComponentTables
                        on e.ComponentID equals c.ComponentID
                        into cj
                    from c in cj.DefaultIfEmpty()
                    join cust in _db.CustomerMasters
                        on e.CustomerID equals cust.CustomerID
                    where e.IsActive == true
                    orderby e.CreatedDate descending
                    select new RecentEnquiryModel
                    {
                        EnquiryID = e.EnquiryID,
                        SalesNO = e.SalesNO ?? "",
                        CustomerName = cust != null ? cust.CompanyName ?? "" : "",
                        DraftType = e.DraftType=="1"?"Side Draft":"1",
                        // ── SubType mapping ───────────────────────────────────
                        SubType = e.SubTypeOfDraftType == "1" ? "Open" :
                                  e.SubTypeOfDraftType == "2" ? "Filters On Top or Door" :
                                  e.SubTypeOfDraftType == "3" ? "Enclosed with FAS" :
                                  e.SubTypeOfDraftType ?? "",
                        EnquiryDate = e.EnquiryDate,
                        TotalCostOfRM = e.TotalCostOfRM+e.TotalCostOfPanels+e.TotalCostOfFrames,
                        IsActive = e.IsActive
                    })
                    .Take(5)
                    .ToList();
        }

        // ─────────────────────────────────────────────────────────
        // Activity Feed — recent enquiries as activities
        // ─────────────────────────────────────────────────────────
        //private List<ActivityModel> GetActivities()
        //{
        //    return _db.EnquiryMasters
        //              .Where(x => x.IsActive == true)
        //              .OrderByDescending(x => x.CreatedDate)
        //              .Take(8)
        //              .AsEnumerable()                          // switch to LINQ-to-Objects safely
        //              .Select(x => new ActivityModel
        //              {
        //                  Message = $"Enquiry {x.SalesNO ?? "N/A"}",
        //                  Category = "Enquiry",
        //                  CreatedAt = x.CreatedDate ?? DateTime.UtcNow,
        //                  TimeAgo = GetTimeAgo(x.CreatedDate ?? DateTime.UtcNow)
        //              })
        //              .ToList();
        //}
        // ─────────────────────────────────────────────────────────
        // Helper — "5m ago", "2h ago", "3d ago"
        // ─────────────────────────────────────────────────────────
        private string GetTimeAgo(DateTime dateTime)
        {
            var diff = DateTime.UtcNow - dateTime;

            if      (diff.TotalMinutes < 1)  return "just now";
            else if (diff.TotalMinutes < 60) return $"{(int)diff.TotalMinutes}m ago";
            else if (diff.TotalHours   < 24) return $"{(int)diff.TotalHours}h ago";
            else if (diff.TotalDays    < 7)  return $"{(int)diff.TotalDays}d ago";
            else                             return dateTime.ToString("dd MMM yyyy");
        }
    }
}
