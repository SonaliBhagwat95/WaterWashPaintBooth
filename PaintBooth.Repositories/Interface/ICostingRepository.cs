

using PaintBooth.Database;

using PaintBooth.Models;

namespace PaintBooth.Repositories.Interface
{
    public interface ICostingRepository
    {
        // ── Panel & baffle data ───────────────────────────────────
        List<CostingListViewModel> GetAll();
        List<PaintBoothModel> GetAllByEnquiryID(string enquiryCode);
        List<PaintBoothModel> GetAllMetalBafflesDetails(string enquiryCode);
        CostingModel GetByEnquiryCode(string enquiryCode);
        // ── Save & fetch costs ────────────────────────────────────
        int  SaveCostOFFrame(string enquiryCode, CostingModel model);
        (decimal TotalPriceOfFrame, decimal TotalPriceOfPanels) GetCostOFFrame(string enquiryCode);
        bool UpdateTotalPriceOfFrame(string enquiryCode,
                                     decimal totalPriceOfFrame,
                                     decimal totalPriceOfPanels);

        // ── BO Calculations data ──────────────────────────────────
        List<MaterialModel>       GetAllFrameDetails(string enquiryCode);
        List<SettingItemViewModel>        GetAlllightDetails(string enquiryCode);
        List<PaintBoothModel>     GetAllBlowerDetails(string enquiryCode);
        List<ExhaustDuctingModel> GetAllExhaustDuctingDetails(string enquiryCode);
        public decimal GetTotalDuctNetWeight(int enquiryId);

        public decimal GetExhaustDuctPrice(string salesNo, int enquiryId);
        public List<ExhaustDuctings> GetExhaustDuctings(string salesNo, int enquiryId);

        // ── Price BID ─────────────────────────────────────────────
        int  SavePriceDetailsValues(BOCalculationModel model, string enquiryCode,int userId);
        bool UpdatePriceDetails(PriceBIDModel model);
        List<PriceBIDModel> GetPriceBIDData(string enquiryCode);
        List<PriceBIDModel> GetEnquiryCodes(string enquiryCode);
        PriceBIDModel       FetchPriceBidRecoreds(string enquiryCode);
        PriceBIDModel       GetAllPaymentTerms(int enquiryId);
        int                 SavePaymentTermsValues(PriceBIDModel model, string enquiryCode,int userId);

        // ── Proposal ──────────────────────────────────────────────
        ProposalModel GetAllDetails(string enquiryCode);
        ProposalModel GetMaterialsValue(string enquiryCode);
        public BlowerDetails GetBlowerMounting(int enquiryId);
        public bool IsSamplingPlatformProvided(int enquiryId);

        // ── Weight totals ─────────────────────────────────────────
        decimal CalculateTotalWeightByEnquiryCode(string enquiryCode);

        // ── Autocomplete ──────────────────────────────────────────
        List<string> SearchEnquiryCodes(string searchTerm);
        List<MaterialModel> GetAllMaterial();
        List<BOCalculationListViewModel> GetAllBO();

        List<PriceBIDListViewModel> GetAllPriceBID();
        int GetEnquiryIdBySalesNo(string salesNo);
        List<ProposalListViewModel> GetAllProposals();
        //BOCalculation? GetSavedBODetails(string enquiryCode);
        void SaveBOItemRates(string enquiryCode, string itemRatesJson, int userId);
        List<BOItemRate> GetSavedBOItemRates(string enquiryCode);
        PriceDetailsTable? GetSavedBODetails(string enquiryCode);
        //List<MaterialModel> GetAllMaterial();
        List<MaterialModel> GetAllMaterialForBO();
        MaterialModel GetMaterialById(int id);
        void SaveMaterial(MaterialModel model, int userId);
        void DeleteMaterial(int id, int userId);
        public int GetEnquiryIdByCode(string enquiryCode);
        public List<CostingAdditionalRM> GetAdditionalRM(int enquiryId);
        public void DeleteAdditionalRM(int enquiryId);
        public void SaveAdditionalRM(List<CostingAdditionalRM> list);
        public List<BOAdditionalItems> GetAdditionalBO(int enquiryId);
        public void DeleteAdditionalBO(int enquiryId);
        public void SaveAdditionalBO(List<BOAdditionalItems> list);
        public void SaveENCCalculation(string salesNO, int enquiryId, string rowsJson,
                               decimal margin, decimal subTotal, decimal finalValue);

        public ENCCalculation GetENCCalculation(string salesNO);
        public EnquirySalesEnDetails GetSalesDetails(int salesID);

    }
}
