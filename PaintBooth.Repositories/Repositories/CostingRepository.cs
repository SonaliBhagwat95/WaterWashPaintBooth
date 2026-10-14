using Microsoft.EntityFrameworkCore;
using PaintBooth.Database;
using PaintBooth.Models;
using PaintBooth.Repositories.Interface;

namespace PaintBooth.Repositories.Repositories
{
    public class CostingRepository(AppDbContext _db) :ICostingRepository
    {
       

        public List<CostingListViewModel> GetAll()
        {
            // Show enquiries that have at least one panel detail saved
            // and join to customer for company name
            return (from e in _db.EnquiryMasters
                    join c in _db.CustomerMasters
                        on e.CustomerID equals c.CustomerID
                    join p in _db.PanelDetails
                        on e.EnquiryID equals p.EnquiryId
                    where e.IsActive == true
                    group new { e, c } by new
                    {
                        e.EnquiryID,
                        e.SalesNO,
                        c.CompanyName,
                        e.TotalCostOfPanels,
                        e.TotalCostOfFrames,
                        e.TotalCostOfRM,
                        e.CreatedDate
                    } into g
                    orderby g.Key.CreatedDate descending
                    select new CostingListViewModel
                    {
                        EnquiryID = g.Key.EnquiryID,
                        SalesNO = g.Key.SalesNO,
                        CompanyName = g.Key.CompanyName,
                        TotalCostOfPanels = g.Key.TotalCostOfPanels,
                        TotalCostOfFrames = g.Key.TotalCostOfFrames,
                        TotalCostOfRM = g.Key.TotalCostOfRM,
                        CreatedDate = g.Key.CreatedDate
                    })
                    .ToList();
        }
        public CostingModel GetByEnquiryCode(string enquiryCode)
        {
            var model = new CostingModel();

            List<SettingItemViewModel> rateList = GetBySalesNo(enquiryCode);
            // 1. Panel details
            model.DetailsList = GetAllByEnquiryID(enquiryCode);
            model.ExhaustDuctingList = GetAllExhaustDuctingDetails(enquiryCode);
            // 2. Metal baffles
            var baffleList = GetAllMetalBafflesDetails(enquiryCode);
            model.metalBaffles = baffleList;

            if (baffleList != null && baffleList.Any())
            {
                foreach (var b in baffleList)
                {
                    b.PanelPosition = "MetalBaffle";
                    b.SheetThickness = model.DetailsList.FirstOrDefault()?.SheetThickness ?? 0;
                }
                model.DetailsList.AddRange(baffleList);
            }

            // 3. Assign DisplayPanelPosition
            foreach (var group in model.DetailsList.GroupBy(d => GetPanelPrefix(d.PanelPosition)))
            {
                int counter = 1;
                foreach (var item in group)
                {
                    item.DisplayPanelPosition = $"{group.Key}_{counter:D2}";
                    counter++;
                }
            }


            #region Rate
            decimal paintboothRate = rateList
                .Where(r => r.SettingType == "Paintbooth")
                .Select(r => r.Rate)
                .FirstOrDefault();

            decimal extractionRate = rateList
                .Where(r => r.SettingType == "Extraction")
                .Select(r => r.Rate)
                .FirstOrDefault();

            // Structural M. rate — only when a structural item code exists
            decimal structuralMRate = rateList
                .Where(r => !string.IsNullOrEmpty(r.ItemCodeForStructuralM))
                .Select(r => r.RateForStructuralM)
                .FirstOrDefault();


            #endregion
            // 5. Frame (filtered) details
            model.FilteredDetailsList = model.DetailsList
                .Where(d => d.PanelPosition == "TopStructureFrame"
                         || d.PanelPosition == "BaseStructureFrame")
                .GroupBy(d => new { d.EnquiryId, d.PanelPosition })
                .Select(g => g.First())
                .ToList();

           

            // 6. Weight calculations
            model.TotalWeight = Math.Round(model.DetailsList.Sum(d => d.PanelWeight),2);
            model.TotalWeightOfFrame = model.FilteredDetailsList.Sum(d => d.PanelWeight);
            model.RMWeightOfPanels = Math.Round(model.TotalWeight - model.TotalWeightOfFrame, 2);
            // ── 7. NEW: Get Paintbooth (PA) and Extraction (EC) rates from PanelSettings ──
            
            // ── 8. NEW: Split panel-only items (excluding frame + baffle) by PA vs EC prefix ──
            var panelOnlyItems = model.DetailsList
                .Where(d => d.PanelPosition != "TopStructureFrame"
                         && d.PanelPosition != "BaseStructureFrame"
                        /* && d.PanelPosition != "MetalBaffle"*/)
                .ToList();

            double paWeight = panelOnlyItems
                .Where(d => !string.IsNullOrEmpty(d.PanelPosition) && d.PanelPosition.StartsWith("PA"))
                .Sum(d => d.PanelWeight);

            double ecWeight = panelOnlyItems
                .Where(d => !string.IsNullOrEmpty(d.PanelPosition) && d.PanelPosition.StartsWith("EC"))
                .Sum(d => d.PanelWeight);

    //        double baffleWeight = model.DetailsList
    //.Where(d => d.PanelPosition == "MetalBaffle")
    //.Sum(d => d.PanelWeight);
    //        decimal priceOfBaffles = (decimal)baffleWeight * paintboothRate;  // pick the correct rate


            // ── 9. NEW: Price PA panels using Paintbooth rate, EC panels using Extraction rate ──
            decimal priceOfPAPanels = (decimal)paWeight * paintboothRate;
            decimal priceOfECPanels = (decimal)ecWeight * extractionRate;
            decimal TotalWeightofExhaustDuct = Math.Round(model.ExhaustDuctingList.Sum(d => d.DuctNetWeight), 2);
            decimal PriceOfExhaust = TotalWeightofExhaustDuct * paintboothRate;

            model.TotalPriceofPanels = (double)Math.Round(priceOfPAPanels + priceOfECPanels+ PriceOfExhaust, 2);
            decimal frameRate = structuralMRate > 0 ? structuralMRate : paintboothRate;
            model.TotalPriceofFrame = Math.Round(model.TotalWeightOfFrame * (double)frameRate, 2);
            //model.TotalPriceofFrame = Math.Round(model.TotalWeightOfFrame * (double)structuralMRate, 2);

            model.TotalCostOfRawMaterials = Math.Round(model.TotalPriceofPanels + model.TotalPriceofFrame, 2);

            return model;
        }

        public List<SettingItemViewModel> GetBySalesNo(string salesNo)
        {
            return _db.PanelSettings
                      .Where(x => x.SalesNo == salesNo
                               && x.IsActive == true)
                      .Select(x => new SettingItemViewModel
                      {
                          SId = x.SId,
                          SettingType = x.SettingType,
                          PanelWidth = (double)x.PanelWidth,
                          PanelHeight = x.PanelHeight ?? 0,
                          SheetThickness = x.SheetThickness ?? 0,
                          Section = x.Section ?? "",
                          BendStructural = x.BendStructural ?? "",
                          Materials = x.Materials ?? "",
                          StandardBend1 = x.StandardBend1 ?? 0,
                          StandardBend2 = x.StandardBend2 ?? 0,
                          PitchDistance = x.PitchDistance ?? 0,
                          SlotDimensions = x.SlotDimensions ?? "",
                          LightTypes = x.LightTypes ?? "",
                          LightSubTypes = x.LightSubTypes ?? "",
                          LuxLevel = (decimal)x.LuxLevel,
                          Lumens = (decimal)x.Lumens,
                          ItemCode = x.ItemCode ?? "",   // ← NEW
                          Rate = x.Rate ?? 0 ,    // ← NEW
                          ItemCodeForStructuralM = x.ItemCodeForStructuralM ?? "",   // ← NEW
                          RateForStructuralM = x.RateForStructuralM ?? 0m            // ← NEW
                      })
                      .ToList();
        }

        public List<ExhaustDuctings> GetExhaustDuctings(string salesNo, int enquiryId)
        {
            return _db.ExhaustDuctings
                .Where(x => x.SalesNo == salesNo
                         && x.EnquiryID == enquiryId
                         && x.IsActive)
                .OrderBy(x => x.DuctId)
                .ToList();
        }
        private int GetEnquiryID(string enquiryCode)
        {
            return _db.EnquiryMasters
                             .Where(x => x.SalesNO == enquiryCode)
                             .Select(x => x.EnquiryID)
                             .FirstOrDefault();
        }
       
        private string GetPanelPrefix(string panelPosition)
        {
            if (string.IsNullOrEmpty(panelPosition)) return "";

            if (panelPosition.StartsWith("PARight"))          return "PA Right";
            if (panelPosition.StartsWith("PALeft"))           return "PA Left";
            if (panelPosition.StartsWith("PATop"))            return "PA Top";
            if (panelPosition.StartsWith("PAFront"))          return "PA Front";
            if (panelPosition.StartsWith("ECRight"))          return "EC Right";
            if (panelPosition.StartsWith("ECLeft"))           return "EC Left";
            if (panelPosition.StartsWith("ECRear"))           return "EC Rear";
            if (panelPosition == "TopStructureFrame")         return "Top Structure Frame";
            if (panelPosition == "BaseStructureFrame")        return "Base Structure Frame";
            if (panelPosition == "MetalBaffle")               return "Metal Baffle";

            return panelPosition;
        }

        // ─────────────────────────────────────────────────────────
        // PANEL & BAFFLE DATA
        // ─────────────────────────────────────────────────────────
        public List<PaintBoothModel> GetAllByEnquiryID(string enquiryCode)
        {
            return (from e in _db.EnquiryMasters
                    join p in _db.PanelDetails
                        on e.EnquiryID equals p.EnquiryId
                    where e.SalesNO == enquiryCode
                    select new PaintBoothModel
                    {
                        EnquiryId              = p.EnquiryId,
                        PanelPosition          = p.PanelPosition,
                        StandardPanelWidthForD = p.StandardPanelDepth,
                        EqualPanelWidthByH     = p.StandardPanelHeight,
                        FrameWidth             = p.FrameWidth,
                        FrameHeight            = p.FrameHeight,
                        SheetThickness         = p.SheetThickness,
                        NoofPanels             = p.NoOfPanels,
                        PanelWeight            = p.PanelWeight,
                        Section                = _db.PanelSettings
                                                    .Where(s => s.SalesNo == enquiryCode)
                                                    .Select(s => s.Section)
                                                    .FirstOrDefault()
                    }).ToList();
        }

        public List<PaintBoothModel> GetAllMetalBafflesDetails(string enquiryCode)
        {
            return _db.MetalBaffleDetails
                .Where(x => x.SalesNo == enquiryCode)
                .Select(item => new PaintBoothModel
                {
                    StandardPanelWidthForD = (double)item.BaffleWidth,
                    EqualPanelWidthByH     = (double)item.BaffleHeight,
                    SheetThickness         = 0,
                    NoofPanels             = item.Quantity,
                    PanelWeight            = (double)item.BaffleWeight
                }).ToList();
        }

        // ─────────────────────────────────────────────────────────
        // SAVE & FETCH COSTS
        // ─────────────────────────────────────────────────────────
        public int SaveCostOFFrame(string enquiryCode, CostingModel model)
        {
            var enquiry = _db.EnquiryMasters
                                    .FirstOrDefault(e => e.SalesNO == enquiryCode);

            if (enquiry != null)
            {
                enquiry.TotalCostOfPanels = (decimal)model.TotalPriceofPanels;
                enquiry.TotalCostOfFrames = (decimal)model.TotalPriceofFrame;
                enquiry.TotalCostOfRM     = (decimal)model.TotalCostOfRawMaterials;
                _db.SaveChanges();
            }
            return 1;
        }

        public (decimal TotalPriceOfFrame, decimal TotalPriceOfPanels) GetCostOFFrame(
            string enquiryCode)
        {
            var record = _db.PanelDetails
                                   .FirstOrDefault(e => e.SalesNo == enquiryCode);

            return record != null
                ? (record.CostOfFrame, record.CostOfPanels)
                : (0, 0);
        }

        public bool UpdateTotalPriceOfFrame(string enquiryCode,
                                             decimal totalPriceOfFrame,
                                             decimal totalPriceOfPanels)
        {
            var record = _db.PanelDetails
                                   .FirstOrDefault(e => e.SalesNo == enquiryCode);

            if (record == null) return false;

            record.CostOfPanels  = totalPriceOfPanels;
            record.CostOfFrame  += totalPriceOfFrame;
            _db.SaveChanges();
            return true;
        }

        // ─────────────────────────────────────────────────────────
        // BO CALCULATIONS DATA
        // ─────────────────────────────────────────────────────────
        public List<MaterialModel> GetAllFrameDetails(string enquiryCode)
        {
            return _db.FilterFrameDetails
                .Where(x => x.IsActive == true && x.SalesNO == enquiryCode)
                .Select(item => new MaterialModel
                {
                    FID          = item.FID,
                    Width        = (int)item.FrameWidth,
                    Height       = (int)item.FrameHeight,
                    Quantity     = item.Quantity,
                    filterWeight = (double)item.FrameWeight
                }).ToList();
        }

        public List<SettingItemViewModel> GetAlllightDetails(string enquiryCode)
        {
            return _db.TubeLightDetails
                .Where(x => x.IsActive == true && x.SalesNo == enquiryCode)
                .Select(item => new SettingItemViewModel
                {
                    LightTypes    = item.LightType,
                    LightSubTypes = item.LightSubType,
                    LuxLevel      = (decimal)item.LuxLevel,
                    Lumens        = (decimal)item.Lumens,
                    Quantity      = item.Quantity
                }).ToList();
        }

        public List<PaintBoothModel> GetAllBlowerDetails(string enquiryCode)
        {
            return (from pb in _db.PaintBoothDetails
                    join e  in _db.EnquiryMasters  on pb.EnquiryId equals e.EnquiryID
                    join m  in _db.MotorDetails     on e.EnquiryID  equals m.EnquiryID
                    join mf in _db.MotorFlange   on m.MotorCatalogID equals mf.MotorCatalogID
                    where e.SalesNO == enquiryCode
                       && pb.IsActive == true
                       && m.IsActive  == true
                    select new PaintBoothModel
                    {
                        CapacityofBlowerinH    = pb.CapacityofBlowerinHr,
                        CapacityofBlowerRoundOf = (double)pb.RoundingCapacity,
                        RatedOutputHP          = (decimal)mf.RatedOutputHP,
                        NoOfBlowers=pb.NoOfBlowers,
                        CapacityofEachBlower=pb.CapacityPerBlower
                    }).ToList();
        }

        public List<ExhaustDuctingModel> GetAllExhaustDuctingDetails(string enquiryCode)
        {
            return (from pb in _db.PaintBoothDetails
                    join e  in _db.EnquiryMasters on pb.EnquiryId equals e.EnquiryID
                    join d  in _db.ExhaustDuctings on e.EnquiryID equals d.EnquiryID
                    where e.SalesNO    == enquiryCode
                       && pb.IsActive == true
                       && d.IsActive  == true
                    select new ExhaustDuctingModel
                    {
                        BendType      = d.BendType,
                        DuctHeight    = d.DuctHeight,
                        DuctWidth     = d.DuctWidth,
                        DuctThickness = d.DuctThickness,
                        DuctLength    = d.DuctLength,
                        DuctNetWeight    = (decimal)d.DuctNetWeight,
                        DuctGrossWeight  = (decimal)d.DuctGrossWeight
                    }).ToList();
        }

        // ─────────────────────────────────────────────────────────
        // PRICE BID
        // ─────────────────────────────────────────────────────────
        
        public int SavePriceDetailsValues(BOCalculationModel model, string enquiryCode, int UserId)
        {
            RMPriceModel RMModel = GetRMCosts(enquiryCode);

            // Step 1 — check if record already exists
            var existing = _db.PriceDetailsTable
                              .FirstOrDefault(x => x.SalesNo == enquiryCode && x.IsActive == true);

            if (existing != null)
            {
                // Only start a new revision if the previous one was finalised
                if (existing.ConfirmPriceBID == true)
                {
                    existing.RevisionNo = existing.RevisionNo + 1;   // use (existing.RevisionNo ?? 0) + 1 if nullable
                    existing.ConfirmPriceBID = false;                // new revision is a draft again
                }

                existing.TotalPriceOfRawMaterials = RMModel.TotalCostOfRM;
                existing.TotalPriceOfBoughtOut = model.BoughtOut;
                existing.IsBOApproved = model.IsBOApproved;
                existing.ModifiedBy = UserId;
                existing.ModifiedDate = DateTime.Now;
            }
            else
            {
                // ✅ INSERT
                var obj = new PriceDetailsTable
                {
                    IsActive = true,
                    SalesNo = enquiryCode,
                    EnquiryId = RMModel.EnquiryId,
                    TotalPriceOfRawMaterials = RMModel.TotalCostOfRM,
                    TotalPriceOfBoughtOut = model.BoughtOut,
                    IsBOApproved = model.IsBOApproved,
                    RevisionNo = 0,       // ← R0 on first save
                    CreatedBy = UserId,
                    CreatedDate = DateTime.Now,
                    ModifiedBy = UserId,
                    ModifiedDate = DateTime.Now,
                    ConfirmPriceBID = false
                };

                _db.PriceDetailsTable.Add(obj);
            }

            _db.SaveChanges();
            return 1;
        }
        public RMPriceModel GetRMCosts(string enquiryCode)
        {
            return _db.EnquiryMasters
                .Where(e => e.SalesNO == enquiryCode&& e.IsActive==true)
                .Select(e => new RMPriceModel
                {
                    EnquiryId = e.EnquiryID,
                    TotalCostOfPanels = e.TotalCostOfPanels,
                    TotalCostOfFrames = e.TotalCostOfFrames,
                    TotalCostOfRM=e.TotalCostOfRM
                }).FirstOrDefault();
        }


        public bool UpdatePriceDetails(PriceBIDModel model)
        {
            var record = _db.PriceDetailsTable
                                   .FirstOrDefault(x => x.SalesNo == model.SalesNO);

            if (record == null) return false;
            // ── Increment revision on every price bid update ──────
            int nextRevision = _db.PriceDetailsTable
                                  .Where(x => x.SalesNo == model.SalesNO)
                                  .Select(x => (int?)x.RevisionNo)
                                  .Max() ?? 0;
            nextRevision += 1;

            record.RevisionNo = nextRevision;  // ← ADD THIS

            record.LabourCost         = model.LabourCost;
            record.EAndCCost          = model.EAndCCost;
            record.PandFCost          = model.PandFCost;
            record.FreightCost        = model.FreightCost;
            record.BasicCost          = model.BasicCost;
            record.BestPrice          = model.BestPrice;
            record.Insurance          = model.Insurance;
            record.CommercialFactor   = model.CommercialFactor;
            record.IncentiveFactor    = model.IncentiveFactor;
            record.TPCCost            = model.TPCCost;
            record.DesignChargesCost  = model.DesignChargesCost;
            record.POVALUE            = model.POVALUE;
            record.TVCCost            = model.TVCCost;
            record.ConfirmPriceBID    = model.ConfirmPriceBID;
            // ── ADD THESE — rate fields ───────────────────────────
            record.EnCRate = model.EnCRate;
            record.PnFRate = model.PnFRate;
            record.FreightRate = model.FreightRate;
            record.DesignChargesRate = model.DesignChargesRate;
            record.ContingencyRate = model.ContingencyRate;
            record.InsuranceRate = model.InsuranceRate;
            record.CommercialFactorRate = model.CommercialFactorRate;
            record.IncentiveFactorRate = model.IncentiveFactorRate;
            record.TPCRate = model.TPCRate;
            record.PORate = model.PORate;
            record.NegotiationRate = model.NegotiationRate;

            _db.SaveChanges();
            return true;
        }

        public List<PriceBIDModel> GetPriceBIDData(string enquiryCode)
        {
            return (from e in _db.EnquiryMasters
                    join p in _db.PriceDetailsTable
                        on e.SalesNO equals p.SalesNo
                    where p.IsActive == true && p.SalesNo == enquiryCode
                    select new PriceBIDModel
                    {
                        PriceOfRM = e.TotalCostOfRM + e.TotalCostOfFrames + e.TotalCostOfPanels,
                        PriceOfBO = p.TotalPriceOfBoughtOut
                    }).ToList();
        }

        public List<PriceBIDModel> GetEnquiryCodes(string enquiryCode)
        {
            return _db.PriceDetailsTable
                .Where(e => e.IsBOApproved == true && e.SalesNo == enquiryCode)
                .Select(e => new PriceBIDModel
                {
                    SalesNO   = e.SalesNo,
                    PriceOfRM = e.TotalPriceOfRawMaterials,
                    PriceOfBO = e.TotalPriceOfBoughtOut
                }).ToList();
        }

       
        public PriceBIDModel FetchPriceBidRecoreds(string enquiryCode)
        {
            return _db.PriceDetailsTable
                .Where(x => x.IsActive == true && x.SalesNo == enquiryCode)
                .OrderByDescending(x => x.CreatedDate)
                .Select(item => new PriceBIDModel
                {
                    // ── Cost fields ───────────────────────────────────
                    PriceOfRM = item.TotalPriceOfRawMaterials,
                    PriceOfBO=item.TotalPriceOfBoughtOut,
                    LabourCost = item.LabourCost,
                    EAndCCost = item.EAndCCost,
                    PandFCost = item.PandFCost,
                    FreightCost = item.FreightCost,
                    BasicCost = item.BasicCost,
                    BestPrice = item.BestPrice,
                    Insurance = item.Insurance,
                    CommercialFactor = item.CommercialFactor,
                    IncentiveFactor = item.IncentiveFactor,
                    TPCCost = item.TPCCost,
                    DesignChargesCost = item.DesignChargesCost,
                    POVALUE = item.POVALUE,
                    TVCCost = item.TVCCost,
                    NegotiationCost = item.NegotiationCost,
                    ContingencyCost = item.ContingencyCost,
                    ConfirmPriceBID = item.ConfirmPriceBID,
                    RevisionNo=item.RevisionNo,
                    // ── Rate fields ───────────────────────────────────
                    //EnCRate = item.EnCRate,
                    PnFRate = item.PnFRate,
                    FreightRate = item.FreightRate,
                    DesignChargesRate = item.DesignChargesRate,
                    ContingencyRate = item.ContingencyRate,
                    InsuranceRate = item.InsuranceRate,
                    CommercialFactorRate = item.CommercialFactorRate,
                    IncentiveFactorRate = item.IncentiveFactorRate,
                    TPCRate = item.TPCRate,
                    PORate = item.PORate,
                    NegotiationRate = item.NegotiationRate
                })
                .FirstOrDefault();
        }
        public PriceBIDModel GetAllPaymentTerms(int enquiryId)
        {
            return _db.PaymentTerms
                .Where(x => x.IsActive == 1 && x.EnquiryId == enquiryId)
                .Select(item => new PriceBIDModel
                {
                    AdvancedPercentage                = item.AdvancedPercentage,
                    DAPPercentage                     = item.DAPPercentage,
                    SUPPLYBEFOREDISPATCHPercentage    = item.SUPPLYBEFOREDISPATCHPercentage,
                    SUPPLYAFTERDISPATCHPercentage     = item.SUPPLYAFTERDISPATCHPercentage,
                    INSTALLATIONPercentage             = item.INSTALLATIONPercentage,
                    COMMISSIONINGPercentage            = item.COMMISSIONINGPercentage,
                    DeliverySupplyperiod               = item.DeliverySupplyperiod==null? "12-14 weeks": item.DeliverySupplyperiod,
                    EAndCPeriod                       = item.EAndCPeriod==null? "4-6 weeks": item.EAndCPeriod
                }).FirstOrDefault();
        }

        public int SavePaymentTermsValues(PriceBIDModel model, string enquiryCode,int userId)
        {
            int enquiryId     = GetEnquiryID(enquiryCode);
            var existingRecord = _db.PaymentTerms
                .FirstOrDefault(x => x.EnquiryId == enquiryId && x.IsActive == 1);

            if (existingRecord != null)
            {
                // UPDATE
                existingRecord.AdvancedPercentage             = model.AdvancedPercentage;
                existingRecord.DAPPercentage                  = model.DAPPercentage;
                existingRecord.SUPPLYBEFOREDISPATCHPercentage = model.SUPPLYBEFOREDISPATCHPercentage;
                existingRecord.SUPPLYAFTERDISPATCHPercentage  = model.SUPPLYAFTERDISPATCHPercentage;
                existingRecord.INSTALLATIONPercentage          = model.INSTALLATIONPercentage;
                existingRecord.COMMISSIONINGPercentage         = model.COMMISSIONINGPercentage;
                existingRecord.DeliverySupplyperiod            = model.DeliverySupplyperiod;
                existingRecord.EAndCPeriod                    = model.EAndCPeriod;
                existingRecord.ModifiedBy = userId;
                existingRecord.ModifiedDate                   = DateTime.Now;
                _db.PaymentTerms.Update(existingRecord);
            }
            else
            {
                // INSERT
                var obj = new PaymentTerms
                {
                    IsActive                         = 1,
                    EnquiryId                         = enquiryId,
                    AdvancedPercentage                = model.AdvancedPercentage,
                    DAPPercentage                     = model.DAPPercentage,
                    SUPPLYBEFOREDISPATCHPercentage    = model.SUPPLYBEFOREDISPATCHPercentage,
                    SUPPLYAFTERDISPATCHPercentage     = model.SUPPLYAFTERDISPATCHPercentage,
                    INSTALLATIONPercentage             = model.INSTALLATIONPercentage,
                    COMMISSIONINGPercentage            = model.COMMISSIONINGPercentage,
                    DeliverySupplyperiod               = model.DeliverySupplyperiod,
                    EAndCPeriod                       = model.EAndCPeriod,
                    CreatedBy                         = userId,
                    CreatedDate                       = DateTime.Now,
                    ModifiedBy                        = 0,
                    ModifiedDate                      = DateTime.Now
                };
                _db.PaymentTerms.Add(obj);
            }

            _db.SaveChanges();
            return 1;
        }

        // ─────────────────────────────────────────────────────────
        // PROPOSAL
        // ─────────────────────────────────────────────────────────
        public ProposalModel GetAllDetails(string enquiryCode)
        {
            return (from e  in _db.EnquiryMasters
                    join c  in _db.CustomerMasters   on e.CustomerID  equals c.CustomerID
                    join cp in _db.CustomerContactPersons on e.CustomerID equals cp.CustomerID
                    join co in _db.ComponentTables   on e.ComponentID  equals co.ComponentID
                    join p  in _db.PaintBoothDetails on e.EnquiryID    equals p.EnquiryId
                    where e.SalesNO == enquiryCode
                    select new ProposalModel
                    {
                        EnquiryID             = e.EnquiryID,
                        CompanyName           = c.CompanyName,
                        CompEmail             = c.EmailId,
                        DraftType             = e.DraftType,
                        SubTypeOfDraftType=e.SubTypeOfDraftType,
                        CompAddress           = c.CustomerAddress,
                        ContactPerson         = cp.ContactPerson,
                        Designation           = cp.Designation,
                        MobileNo              = cp.Mobile,
                        SalesNO               = e.SalesNO,
                        Component=co.Component,
                        MaterialOfConstruction=co.MaterialofConstruction,
                        WidthSize             = (double)co.WidthSize,
                        HeightSize            = (double)co.HeightSize,
                        Length                = (double)co.Length,
                        Workingdays           = (double)co.Workingdays,
                        NumberofShifts        = (double)co.NumberofShifts,
                        EffectiveWorking      = (double)co.EffectiveWorking,
                        Weight                = (double)co.Weight,
                        ProductionRequirement = (int)co.ProductionRequirement,
                        ComponentHandling     = co.ComponentHandling,
                        Viscosity             = (double)co.Viscosity,
                        DFT                   = (double)co.DFT,
                        SpecificHeat          = (double)co.SpecificHeat,
                        LoadingUnloading      = co.LoadingUnloading,
                        ImagePath = co.Image_Path,
                        NoOfCoats = (int)co.NoOfCoats,
                        Paint= (int)co.Paint,
                        NoOfColors = (int)co.NoOfColors,
                        Category              = co.Category,
                        TypeOfPaint           = co.TypeOfPaint,
                        ExtractionChamberHeight = co.ExtractionChamberHeight,
                        ComponantWeight       = (double)co.Weight,
                        D= (float)p.D,
                        W= (float)p.W,
                        H= (float)p.H,
                        NoOfBlowers= p.NoOfBlowers,
                        SalesEngineerId = (int)e.SalesEngineerId,
                        DoorSubType=co.DoorSubType,
                        DoorWidth= (decimal)co.DoorWidth,
                        DoorHeight= (decimal)co.DoorHeight
                    }).FirstOrDefault();
        }

        //public ProposalModel GetMaterialsValue(string enquiryCode)
        //{
        //    int enquiryID = _db.EnquiryMasters
        //                              .Where(e => e.SalesNO == enquiryCode)
        //                              .Select(e => e.EnquiryID)
        //                              .FirstOrDefault();

        //    string paintboothMaterial = _db.PanelSettings
        //        .Where(s => s.SalesNo == enquiryCode && s.SettingType == "Paintbooth")
        //        .Select(s => s.Materials)
        //        .FirstOrDefault();

        //    string extractionMaterial = _db.PanelSettings
        //        .Where(s => s.SalesNo == enquiryCode && s.SettingType == "Extraction")
        //        .Select(s => s.Materials)
        //        .FirstOrDefault();

        //    return (from e  in _db.EnquiryMasters
        //            join pb in _db.PaintBoothDetails on e.EnquiryID  equals pb.EnquiryId
        //            join t  in _db.TubeLightDetails  on e.EnquiryID  equals t.EnquiryID
        //            join m  in _db.MotorDetails       on e.EnquiryID  equals m.EnquiryID
        //            join mf in _db.MotorFlange     on m.MotorCatalogID equals mf.MotorCatalogID
        //            where e.SalesNO == enquiryCode && e.IsActive==true
        //            select new ProposalModel
        //            {
        //                EXhaustCapacity   = pb.CapacityofBlowerinHr,
        //                Lumens            = (decimal)t.Lumens,
        //                LuxLevel          = (decimal)t.LuxLevel,
        //                TubelightQuantity = t.Quantity,
        //                MotorCapacity     = mf != null && mf.RatedOutputHP.HasValue
        //                                    ? (decimal)mf.RatedOutputHP.Value : 0,
        //                PaintBoothMaterial = paintboothMaterial ?? "N/A",
        //                ECMaterials        = extractionMaterial  ?? "N/A",
        //                D                  = (float)pb.D,
        //                W                  = (float)pb.W,
        //                H                  = (float)pb.H,
        //                D3                 = (float)pb.D3
        //            }).FirstOrDefault();
        //}
        public ProposalModel GetMaterialsValue(string enquiryCode)
        {
            int enquiryID = _db.EnquiryMasters
                               .Where(e => e.SalesNO == enquiryCode)
                               .Select(e => e.EnquiryID)
                               .FirstOrDefault();

            //string paintboothMaterial = _db.PanelSettings
            //    .Where(s => s.SalesNo == enquiryCode && s.SettingType == "Paintbooth")
            //    .Select(s => s.Materials)
            //    .FirstOrDefault();
                    var paintboothSetting = _db.PanelSettings
            .Where(s => s.SalesNo == enquiryCode && s.SettingType == "Paintbooth")
            .Select(s => new { s.Materials, s.SheetThickness })
            .FirstOrDefault();

            string paintboothMaterial = paintboothSetting?.Materials;
            var sheetThickness = paintboothSetting?.SheetThickness;

            //string extractionMaterial = _db.PanelSettings
            //    .Where(s => s.SalesNo == enquiryCode && s.SettingType == "Extraction")
            //    .Select(s => s.Materials)
            //    .FirstOrDefault();
            var extractionSetting = _db.PanelSettings
    .Where(s => s.SalesNo == enquiryCode && s.SettingType == "Extraction")
    .Select(s => new { s.Materials, s.SheetThickness })
    .FirstOrDefault();

            string extractionMaterial = extractionSetting?.Materials;
            double extractionThickness = extractionSetting?.SheetThickness ?? 0;


            return (from e in _db.EnquiryMasters
                    join pb in _db.PaintBoothDetails on e.EnquiryID equals pb.EnquiryId
                    // ── LEFT JOIN TubeLightDetails ────────────────────
                    // ── LEFT JOIN TubeLightDetails — active record only ───────────
                    join t in _db.TubeLightDetails.Where(x => x.IsActive == true)
                        on e.EnquiryID equals t.EnquiryID
                        into tGroup
                    from t in tGroup.DefaultIfEmpty()
                        // ── LEFT JOIN MotorDetails ────────────────────────
                    join m in _db.MotorDetails on e.EnquiryID equals m.EnquiryID
                        into mGroup
                    from m in mGroup.DefaultIfEmpty()
                        // ── LEFT JOIN MotorFlange ─────────────────────────
                    join mf in _db.MotorFlange on m.MotorCatalogID equals mf.MotorCatalogID
                        into mfGroup
                    from mf in mfGroup.DefaultIfEmpty()
                    where e.SalesNO == enquiryCode && e.IsActive == true
                    select new ProposalModel
                    {
                        EXhaustCapacity =  pb.CapacityPerBlower,
                        Lumens = t != null ? (decimal)t.Lumens : 0,
                        LuxLevel = t != null ? (decimal)t.LuxLevel : 0,
                        TubelightQuantity = t != null ? t.Quantity : 0,
                        MotorCapacity = mf != null && mf.RatedOutputHP.HasValue
                                             ? (decimal)mf.RatedOutputHP.Value : 0,
                        PaintBoothMaterial = paintboothMaterial ?? "N/A",
                        ECMaterials = extractionMaterial ?? "N/A",
                        D = (float)pb.D,
                        W = (float)pb.W,
                        H = (float)pb.H,
                        D3 = (float)pb.D3,
                        LightSubType=t != null ? t.LightSubType : "N/A",
                        LightType= t != null ? t.LightType : "N/A",
                        PASheetThickness= (double)sheetThickness,
                        ECSheetThickness= (double)extractionThickness,
                        MotorType = m.MotorTypes == "FLP" ? "Flame Proof" : "Non-Flame Proof"
                    })
                    .FirstOrDefault();
        }
        public BlowerDetails GetBlowerMounting(int enquiryId)
        {
            return _db.BlowerDetails.Where(x => x.EnquiryID == enquiryId).FirstOrDefault();            
        }
        public bool IsSamplingPlatformProvided(int enquiryId)
        {
            return _db.CostingAdditionalRM
                .Any(x => x.EnquiryId == enquiryId
                       && x.Description == "Sampling Inspection Platform" && x.Amount > 0);

        }

        public EnquirySalesEnDetails GetSalesDetails(int salesID)
        {
            return _db.EnquirySalesEndDetails.Where(z => z.Id == salesID).FirstOrDefault();
        }
        // ─────────────────────────────────────────────────────────
        // WEIGHT TOTALS
        // ─────────────────────────────────────────────────────────
        public decimal CalculateTotalWeightByEnquiryCode(string enquiryCode)
        {
            var panelWeight = (decimal)_db.PanelDetails
                .Where(p => p.SalesNo == enquiryCode)
                .Sum(p => p.PanelWeight);

            var frameWeight = _db.FilterFrameDetails
                .Where(t => t.SalesNO == enquiryCode)
                .Sum(t => t.FrameWeight);

            var baffleWeight = _db.MetalBaffleDetails
                .Where(t => t.SalesNo == enquiryCode)
                .Sum(t => t.BaffleWeight);

            var ductWeight = _db.ExhaustDuctings
                .Where(t => t.SalesNo == enquiryCode)
                .Sum(t => t.DuctNetWeight);

            return (decimal)(panelWeight + frameWeight + baffleWeight + ductWeight);
        }

        // ─────────────────────────────────────────────────────────
        // AUTOCOMPLETE
        // ─────────────────────────────────────────────────────────
        public List<string> SearchEnquiryCodes(string searchTerm)
        {
            return (from e  in _db.EnquiryMasters
                    join pb in _db.PaintBoothDetails
                        on e.EnquiryID equals pb.EnquiryId
                    where e.SalesNO.Contains(searchTerm)
                    select e.SalesNO)
                    .Distinct()
                    .ToList();
        }

        public List<MaterialModel> GetAllMaterial()
        {
            List<MaterialModel> getMaterial = _db.MOC.Where(m => m.IsActive == true)
                .Select(item => new MaterialModel()
                {
                    MOCID = item.MOCId,
                    MOCName = item.MOCName,
                    Density = item.Density,
                    Rate = item.Rate,
                }).ToList();
            return getMaterial;
        }

        public List<BOCalculationListViewModel> GetAllBO()
        {
            return (from p in _db.PriceDetailsTable
                    join e in _db.EnquiryMasters
                        on p.EnquiryId equals e.EnquiryID
                    join c in _db.CustomerMasters
                        on e.CustomerID equals c.CustomerID
                    where p.IsActive == true && p.IsBOApproved==true
                    orderby p.CreatedDate descending
                    select new BOCalculationListViewModel
                    {
                        EnquiryID = e.EnquiryID,
                        SalesNO = e.SalesNO,
                        CompanyName = c.CompanyName,
                        TotalPriceOfRM = e.TotalCostOfRM,
                        TotalPriceOfBO = p.TotalPriceOfBoughtOut,
                        IsPriceBidApproved = p.IsBOApproved,
                        CreatedDate = p.CreatedDate
                    })
                    .ToList();
        }
        public List<PriceBIDListViewModel> GetAllPriceBID()
        {
            return (from p in _db.PriceDetailsTable
                    join e in _db.EnquiryMasters on p.EnquiryId equals e.EnquiryID
                    join c in _db.CustomerMasters on e.CustomerID equals c.CustomerID
                    where p.IsActive == true && p.ConfirmPriceBID == true    
                    orderby p.CreatedDate descending
                    select new PriceBIDListViewModel
                    {
                        SalesNO = e.SalesNO,
                        CompanyName = c.CompanyName,
                        POVALUE = p.POVALUE,
                        BestPrice = p.BestPrice,
                        ConfirmPriceBID = p.ConfirmPriceBID
                    }).ToList();
        }

        public int GetEnquiryIdBySalesNo(string salesNo)
        {
            return _db.EnquiryMasters
                .Where(x => x.SalesNO == salesNo)
                .Select(x => x.EnquiryID)
                .FirstOrDefault();
        }
        
        public List<ProposalListViewModel> GetAllProposals()
        {
            return (from e in _db.EnquiryMasters
                    join c in _db.CustomerMasters on e.CustomerID equals c.CustomerID
                    join p in _db.PriceDetailsTable on e.EnquiryID equals p.EnquiryId
                    where p.IsActive == true
                       && p.IsBOApproved == true
                    orderby p.CreatedDate descending
                    select new ProposalListViewModel
                    {
                        SalesNO = e.SalesNO,
                        CompanyName = c.CompanyName,
                        ContactPerson = _db.CustomerContactPersons
                                           .Where(cp => cp.CustomerID == e.CustomerID)
                                           .Select(cp => cp.ContactPerson)
                                           .FirstOrDefault(),   // ← takes only 1, no multiplication
                        POVALUE = p.POVALUE,
                        CreatedDate = p.CreatedDate,
                        RevisionNo = p.RevisionNo
                    })
                    .ToList();
        }
       
        public void SaveBOItemRates(string enquiryCode, string itemRatesJson, int userId)
        {
            var existing = _db.BOItemRate
                              .Where(x => x.EnquiryCode == enquiryCode && x.IsActive == true)
                              .ToList();

            if (!string.IsNullOrWhiteSpace(itemRatesJson))
            {
                var options = new System.Text.Json.JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };

                var rates = System.Text.Json.JsonSerializer
                                  .Deserialize<List<BOItemRateDto>>(itemRatesJson, options);

                if (rates != null)
                {
                    foreach (var r in rates)
                    {
                        // Try to find existing row
                        var existingRow = existing
                            .FirstOrDefault(x => x.ItemType == r.ItemType && x.RowIndex == r.RowIndex);

                        if (existingRow != null)
                        {
                           
                            // ✅ UPDATE
                            existingRow.ItemCode = r.ItemCode;      // ← NEW
                            existingRow.Rate = r.Rate;
                            existingRow.Quantity = r.Quantity;
                            existingRow.Cost = r.Cost;
                            existingRow.ModifiedBy = userId;
                            existingRow.ModifiedDate = DateTime.Now;
                        }
                        else
                        {
                            // ✅ INSERT NEW
                            _db.BOItemRate.Add(new BOItemRate
                            {
                                EnquiryCode = enquiryCode,
                                ItemType = r.ItemType,
                                RowIndex = r.RowIndex,
                                ItemCode = r.ItemCode,              // ← NEW
                                Rate = r.Rate,
                                Quantity = r.Quantity,
                                Cost = r.Cost,
                                IsActive = true,
                                CreatedBy = userId,
                                ModifiedBy = userId,
                                CreatedDate = DateTime.Now,
                                ModifiedDate = DateTime.Now
                            });
                        }
                    }
                }
            }

            _db.SaveChanges();
        }
        public List<BOItemRate> GetSavedBOItemRates(string enquiryCode)
        {
            return _db.BOItemRate
                      .Where(x => x.EnquiryCode == enquiryCode && x.IsActive == true)
                      .OrderBy(x => x.ItemType)
                      .ThenBy(x => x.RowIndex)
                      .ToList();
        }

        // Gets saved BO summary (BoughtOut amount + approval status)
        public PriceDetailsTable? GetSavedBODetails(string enquiryCode)
        {
            return _db.PriceDetailsTable
                      .Where(x => x.SalesNo == enquiryCode && x.IsActive == true)
                      .OrderByDescending(x => x.CreatedDate)
                      .FirstOrDefault();
        }

        public int GetEnquiryIdByCode(string enquiryCode)
        {
            return _db.EnquiryMasters
                      .Where(e => e.SalesNO == enquiryCode)
                      .Select(e => e.EnquiryID)
                      .FirstOrDefault();
        }

        public List<BOAdditionalItems> GetAdditionalBO(int enquiryId)
        {
            return _db.BOAdditionalItems
                      .Where(x => x.EnquiryId == enquiryId)
                      .OrderBy(x => x.Id)
                      .ToList();
        }

        public void DeleteAdditionalBO(int enquiryId)
        {
            var rows = _db.BOAdditionalItems
                          .Where(x => x.EnquiryId == enquiryId)
                          .ToList();
            if (rows.Any())
            {
                _db.BOAdditionalItems.RemoveRange(rows);
                _db.SaveChanges();
            }
        }

        public void SaveAdditionalBO(List<BOAdditionalItems> list)
        {
            _db.BOAdditionalItems.AddRange(list);
            _db.SaveChanges();
        }

        public decimal GetExhaustDuctPrice(string salesNo, int enquiryId)
        {
            // 1. Paintbooth rate (active only)
            decimal paintboothRate = _db.PanelSettings
                .Where(x => x.SalesNo == salesNo
                         && x.IsActive == true
                         && x.SettingType == "Paintbooth")
                .Select(x => x.Rate ?? 0)
                .FirstOrDefault();

            // 2. Total duct net weight (active only)
            decimal totalDuctNetWeight = _db.ExhaustDuctings
                .Where(d => d.EnquiryID == enquiryId && d.IsActive == true)
                .Sum(d => d.DuctNetWeight ?? 0);

            // 3. Price
            return Math.Round(paintboothRate * totalDuctNetWeight, 2);
        }

        #region Materials

        // ── Get materials for BO (MOCId > 6) ─────────────────────────
        public List<MaterialModel> GetAllMaterialForBO()
        {
            return _db.MOC
                      .Where(x => x.MOCId > 6 && x.IsActive == true)
                      .Select(x => new MaterialModel
                      {
                          MOCID = x.MOCId,
                          MOCName = x.MOCName,
                          Rate = x.Rate
                      })
                      .ToList();
        }

        // ── Get single material by ID ─────────────────────────────────
        public MaterialModel GetMaterialById(int id)
        {
            return _db.MOC
                      .Where(x => x.MOCId == id && x.IsActive == true)
                      .Select(x => new MaterialModel
                      {
                          MOCID = x.MOCId,
                          MOCName = x.MOCName,
                          Density = x.Density,
                          Rate = x.Rate,
                          CreatedBy = x.CreatedBy != null ? (int)x.CreatedBy : 0,
                          CreatedDate = x.CreatedDate ?? DateTime.MinValue,
                          ModifiedBy = x.ModifiedBy,
                          ModifiedDate = x.ModifiedDate ?? DateTime.MinValue,
                          IsActive = x.IsActive
                      })
                      .FirstOrDefault();
        }

        // ── Save (insert or update) ───────────────────────────────────
        public void SaveMaterial(MaterialModel model, int userId)
        {
            if (model.MOCID > 0)
            {
                // UPDATE existing
                var existing = _db.MOC.Find(model.MOCID);
                if (existing == null) return;

                existing.MOCName = model.MOCName;
                existing.Density = model.Density;
                existing.Rate = model.Rate;
                existing.ModifiedBy = userId;
                existing.ModifiedDate = DateTime.Now;
                existing.IsActive = true;
            }
            else
            {
                // INSERT new
                _db.MOC.Add(new MOC
                {
                    MOCName = model.MOCName,
                    Density = model.Density,
                    Rate = model.Rate,
                    CreatedBy = userId,
                    CreatedDate = DateTime.Now,
                    ModifiedBy = 0,
                    ModifiedDate = DateTime.Now,
                    IsActive = true
                });
            }

            _db.SaveChanges();
        }

        // ── Soft delete ───────────────────────────────────────────────
        public void DeleteMaterial(int id, int userId)
        {
            var obj = _db.MOC.Find(id);
            if (obj == null) return;

            obj.IsActive = false;
            obj.ModifiedBy = userId;
            obj.ModifiedDate = DateTime.Now;

            _db.SaveChanges();
        }

        #endregion


        #region Additional RM
        
        

        // Get saved additional RM rows
        public List<CostingAdditionalRM> GetAdditionalRM(int enquiryId)
        {
            return _db.CostingAdditionalRM
                      .Where(x => x.EnquiryId == enquiryId)
                      .OrderBy(x => x.Id)
                      .ToList();
        }

        // Delete existing rows before re-saving
        public void DeleteAdditionalRM(int enquiryId)
        {
            var rows = _db.CostingAdditionalRM
                          .Where(x => x.EnquiryId == enquiryId)
                          .ToList();

            if (rows.Any())
            {
                _db.CostingAdditionalRM.RemoveRange(rows);
                _db.SaveChanges();
            }
        }

        // Save new rows
        public void SaveAdditionalRM(List<CostingAdditionalRM> list)
        {
            _db.CostingAdditionalRM.AddRange(list);
            _db.SaveChanges();
        }
        #endregion

        public void SaveENCCalculation(string salesNO, int enquiryId, string rowsJson,
                                decimal margin, decimal subTotal, decimal finalValue)
        {
            var existing = _db.ENCCalculation
                              .FirstOrDefault(x => x.SalesNO == salesNO);
            if (existing != null)
            {
                existing.EnquiryId = enquiryId;
                existing.RowsJson = rowsJson;
                existing.ContractorMargin = margin;
                existing.SubTotal = subTotal;
                existing.FinalValue = finalValue;
                existing.UpdatedAt = DateTime.Now;
            }
            else
            {
                _db.ENCCalculation.Add(new ENCCalculation
                {
                    EnquiryId = enquiryId,
                    SalesNO = salesNO,
                    RowsJson = rowsJson,
                    ContractorMargin = margin,
                    SubTotal = subTotal,
                    FinalValue = finalValue,
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now
                });
            }
            _db.SaveChanges();
        }

        public ENCCalculation GetENCCalculation(string salesNO)
        {
            return _db.ENCCalculation
                      .FirstOrDefault(x => x.SalesNO == salesNO);
        }


        public decimal GetTotalDuctNetWeight(int enquiryId)
        {
            return _db.ExhaustDuctings
                      .Where(d => d.EnquiryID == enquiryId && d.IsActive == true)
                      .Sum(d => d.DuctNetWeight ?? 0);
        }

    }
}
