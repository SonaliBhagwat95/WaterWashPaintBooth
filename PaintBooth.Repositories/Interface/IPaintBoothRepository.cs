using PaintBooth.Database;
using PaintBooth.Models;


namespace PaintBooth.Repositories.Interface
{
    public interface IPaintBoothRepository
    {
        // ── List ──────────────────────────────────────────────────
        List<PaintBoothListViewModel> GetAll();

        // ── Single ────────────────────────────────────────────────
        PaintBoothDetails? GetBySalesNo(string salesNo);
        int GetEnquiryID(string EnquiryNo);
        int GetComponentID(string EnquiryNo);
        // Autocomplete
        List<string> SearchEnquiryCodes(string term);

            // Search — load enquiry + setting
           // EnquiryModel GetEnquiryDetailsByCode(string enquiryCode);
            //string GetMotorTypesByEnqID(int enquiryId);
           // PaintBoothModel GetSettingDetailsByCode(string enquiryCode);

            // Water column
            List<Database.PressureDrop> GetAallWaterColumnDetails();

            // Motor
            List<MotorFlangeModel> GetMotorDetails(decimal blowerHp);
        BlowerDetails GetBlowerDetails(int enquiryId);
        public void SaveExhaustDucting(int enquiryId, string salesNo, PaintBoothModel model, int userId);
        public List<ExhaustDuctings> GetExhaustDuctingByEnquiry(int enquiryId);
            // Save
            bool Exists(int enquiryId);

        EnquirySearchResultViewModel GetEnquiryData(string salesNo);

        string FetchSubTypeOfDraft(string EnquiryCode);
        string FetchPaintboothType(int EnquiryId);
        string FetchPlenumHeight(string EnquiryCode);
        void SaveMotorDeatils(int enquiryID, int MotorCatalogID, string MotorItemCode);
        void SaveBlowerDetails(int enquiryID, PaintBoothModel model);
        string FetchPaintBoothType(string EnquiryCode);
        decimal FetchRatedOutputHP(int MotorCatalogID);
        public ComponentTable GetWatchGlassDetails(int ComponentId);
        string GetMotorTypes(int MotorCatalogID);
        PaintBoothModel GetSettingDetailsByCode(string enquiryCode);
        int SavePaintBooth(PaintBoothModel model, int enquiryId);
        public int UpdateBetweenCenterDistance(int enquiryId, decimal? betweenCenterDistance, PaintBoothModel model);
        int FetchComponentId(int enquiryID);
        public void UpdateWatchGlassDetails(int ComponentId, PaintBoothModel model);
        public void UpdateDoorDetails(int ComponentId, PaintBoothModel model);
        
        EnquiryMaster FetchMaintenanceDoorDetails(int enquiryID);
        ComponentTable FetchDoorType(int componentEntryId);
        string FetchExtractionHeight(int ComponentID);
        void DeletePaintBooth(int enquiryId);
        void SaveWaterColumnDetails(int enquiryId, string waterColumnRowsJson);
        List<WaterColumnDetail> GetWaterColumnDetails(int enquiryId);
        MotorDetails? GetMotorBySalesNo(string salesNo);
        public void UpdateTubelightDetails(PaintBoothModel model, decimal LuxLevel, decimal Lumens);
        public string GetFilterType(int enquiryID);
        public string FetchCatagoryType(int ComponentId);
        public string FetchWashingTankType(int ComponentId);
    }
}
