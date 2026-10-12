

using PaintBooth.Models;

namespace PaintBooth.Repositories.Interface
{
    public interface IPanelSettingRepository
    {
        // ── Index list ────────────────────────────────────────────
        List<PanelSettingListViewModel> GetAll();

        // ── Get all settings for one enquiry ──────────────────────
        List<SettingItemViewModel> GetBySalesNo(string salesNo);

        // ── Save (insert or update all settings for an enquiry) ───
        void Save(string salesNo, List<SettingItemViewModel> settings, int userId);

        // ── Delete all settings for an enquiry ────────────────────
        void Delete(string salesNo, int userId);

        // ── Check if settings exist for enquiry ───────────────────
        bool ExistsBySalesNo(string salesNo);

        // ── Enquiry code search (autocomplete) ────────────────────
        List<string> SearchEnquiryCodes(string term);
        void SaveTubeLightDetails(string salesNo, List<SettingItemViewModel> settings, int userId);
        string GetCategoryBySalesNo(string salesNo);
        string GetPaintboothType(string salesNo);
    }
}
