

using Microsoft.EntityFrameworkCore;
using PaintBooth.Database;
using PaintBooth.Models;
using PaintBooth.Repositories.Interface;

namespace PaintBooth.Repositories
{
    public class PanelSettingRepository(AppDbContext _db) : IPanelSettingRepository
    {


        // ── Get all — grouped by SalesNo for index list ───────────
        //public List<PanelSettingListViewModel> GetAll()
        //{
        //    return _db.PanelSettings
        //              .Where(x => x.IsActive == true)
        //              .GroupBy(x => x.SalesNo)
        //              .Select(g => new PanelSettingListViewModel
        //              {
        //                  SalesNo = g.Key,

        //                  PaintBoothThk = g
        //                .Where(x => x.SettingType == "Paintbooth")
        //                .Select(x => x.SheetThickness)
        //                .FirstOrDefault()
        //                .ToString(),

        //                  ECThk = g
        //                      .Where(x => x.SettingType == "Extraction")
        //                      .Select(x => x.SheetThickness)
        //                      .FirstOrDefault()
        //                      .ToString(),

        //                  PaintBoothMaterial = g
        //                      .Where(x => x.SettingType == "Paintbooth")
        //                      .Select(x => x.Materials)
        //                      .FirstOrDefault() ?? "",

        //                  ECMaterial = g
        //                      .Where(x => x.SettingType == "Extraction")
        //                      .Select(x => x.Materials)
        //                      .FirstOrDefault() ?? ""
        //              })
        //              .OrderByDescending(x => x.SalesNo)
        //              .ToList();
        //}

        // ── Get all settings for one enquiry ──────────────────────

        public List<PanelSettingListViewModel> GetAll()
        {
            return _db.PanelSettings
                      .Where(x => x.IsActive == true)
                      .GroupBy(x => x.SalesNo)
                      .Select(g => new
                      {
                          SortKey = g.Max(x => x.CreatedDate),
                          Vm = new PanelSettingListViewModel
                          {
                              SalesNo = g.Key,
                              PaintBoothThk = g.Where(x => x.SettingType == "Paintbooth")
                                               .Select(x => x.SheetThickness)
                                               .FirstOrDefault().ToString(),
                              ECThk = g.Where(x => x.SettingType == "Extraction")
                                       .Select(x => x.SheetThickness)
                                       .FirstOrDefault().ToString(),
                              PaintBoothMaterial = g.Where(x => x.SettingType == "Paintbooth")
                                                    .Select(x => x.Materials)
                                                    .FirstOrDefault() ?? "",
                              ECMaterial = g.Where(x => x.SettingType == "Extraction")
                                            .Select(x => x.Materials)
                                            .FirstOrDefault() ?? ""
                          }
                      })
                      .OrderByDescending(x => x.SortKey)
                      .Select(x => x.Vm)
                      .ToList();
        }


        public List<SettingItemViewModel> GetBySalesNo(string salesNo)
        {
            return _db.PanelSettings
                      .Where(x => x.SalesNo   == salesNo
                               && x.IsActive == true)
                      .Select(x => new SettingItemViewModel
                      {
                          SId            = x.SId,
                          SettingType    = x.SettingType,
                          PanelWidth     = (double)x.PanelWidth,
                          PanelHeight    = x.PanelHeight    ?? 0,
                          SheetThickness = x.SheetThickness ?? 0,
                          Section        = x.Section        ?? "",
                          BendStructural = x.BendStructural ?? "",
                          Materials      = x.Materials      ?? "",
                          StandardBend1  = x.StandardBend1  ?? 0,
                          StandardBend2  = x.StandardBend2  ?? 0,
                          PitchDistance  = x.PitchDistance  ?? 0,
                          SlotDimensions = x.SlotDimensions ?? "",
                          LightTypes     = x.LightTypes     ?? "",
                          LightSubTypes  = x.LightSubTypes  ?? "",
                          LuxLevel       = (decimal)x.LuxLevel,
                          Lumens         = (decimal)x.Lumens,
                          ItemCode = x.ItemCode ?? "",   // ← NEW
                          Rate = x.Rate ?? 0 ,    // ← NEW
                          ItemCodeForStructuralM = x.ItemCodeForStructuralM ?? "",   // ← NEW,
                          RateForStructuralM= x.RateForStructuralM ??0,    // ← NEW
                          WaterCurtain=x.WaterCurtain
                      })
                      .ToList();
        }

        // ── Save — delete old + insert new (replace strategy) ─────
        public void Save(string salesNo, List<SettingItemViewModel> settings, int userId)
        {
            // Step 1 — soft delete all existing settings for this enquiry
            var existing = _db.PanelSettings
                              .Where(x => x.SalesNo   == salesNo
                                       && x.IsActive == true)
                              .ToList();

            existing.ForEach(x =>
            {
                x.IsActive   = false;
                x.ModifiedBy  = userId;
                x.ModifiedDate = DateTime.Now;
            });


            // Step 2 — insert new settings
            foreach (var item in settings)
            {
                int? H = null, W = null, T = null;

                // 🔹 Split H*W*T from BendStructural (or Section if that's your field)
                if (!string.IsNullOrEmpty(item.BendStructural))
                {
                    string[] parts = item.BendStructural.Split('*');

                    if (parts.Length == 3)
                    {
                        H = int.TryParse(parts[0], out int hVal) ? hVal : null;
                        W = int.TryParse(parts[1], out int wVal) ? wVal : null;
                        T = int.TryParse(parts[2], out int tVal) ? tVal : null;
                    }
                }
                _db.PanelSettings.Add(new PanelSetting
                {
                    SalesNo        = salesNo,
                    SettingType    = item.SettingType,
                    PanelWidth     = item.PanelWidth,
                    PanelHeight    = item.PanelHeight,
                    SheetThickness = item.SheetThickness,
                    Section        = item.Section,
                    BendStructural = item.BendStructural,
                    Materials      = item.Materials,
                    StandardBend1  = item.StandardBend1,
                    StandardBend2  = item.StandardBend2,
                    PitchDistance  = item.PitchDistance,
                    SlotDimensions = item.SlotDimensions,
                    H = H ?? 0,
                    W = W ?? 0,
                    T = T ?? 0,
                    LightTypes     = item.LightTypes,
                    LightSubTypes  = item.LightSubTypes,
                    LuxLevel       = (double?)item.LuxLevel,
                    Lumens         = (double?)item.Lumens,
                    ItemCode = item.ItemCode,              // ← NEW
                    Rate = item.Rate,                  // ← NEW
                    ItemCodeForStructuralM = item.ItemCodeForStructuralM,              // ← NEW
                    RateForStructuralM = item.RateForStructuralM,                  // ← NEW
                    IsActive      = true,
                    CreatedBy      = userId,
                    CreatedDate    = DateTime.Now,
                    WaterCurtain   =   item.WaterCurtain==null ? "" : item.WaterCurtain
                });
            }

            _db.SaveChanges();
        }
        public void SaveTubeLightDetails(string salesNo, List<SettingItemViewModel> settings, int userId)
        {
            try
            {
                int enquiryId = GetEnquiryID(salesNo);

                // Step 1 — soft delete existing TubeLightDetails for this enquiry
                var existing = _db.TubeLightDetails
                                  .Where(x => x.SalesNo == salesNo && x.IsActive == true)
                                  .ToList();

                existing.ForEach(x =>
                {
                    x.IsActive = false;
                    
                });

                // Step 2 — insert fresh records for Paintbooth type only
                foreach (var setting in settings)
                {
                    if (setting.SettingType == "Paintbooth")
                    {
                        _db.TubeLightDetails.Add(new TubeLightDetails
                        {
                            EnquiryID = enquiryId,
                            SalesNo = salesNo,
                            LightType = setting.LightTypes,
                            LightSubType = setting.LightSubTypes,
                            LuxLevel = setting.LuxLevel,
                            Lumens = setting.Lumens,
                            //Quantity = 0,
                            IsActive = true,
                            
                        });
                    }
                }

                _db.SaveChanges(); // ← single SaveChanges at the end, not inside loop
            }
            catch (Exception ex)
            {
                throw; // ← use bare 'throw' to preserve original stack trace
            }
        }
        private int GetEnquiryID(string enquiryCode)
        {
            return _db.EnquiryMasters
                             .Where(x => x.SalesNO == enquiryCode)
                             .Select(x => x.EnquiryID)
                             .FirstOrDefault();
        }
        // ── Delete all settings for an enquiry ────────────────────
        public void Delete(string salesNo, int userId)
        {
            var settings = _db.PanelSettings
                              .Where(x => x.SalesNo == salesNo)
                              .ToList();

            settings.ForEach(x =>
            {
                x.IsActive    = false;
                x.ModifiedBy   = userId;
                x.ModifiedDate = DateTime.UtcNow;
            });

            _db.SaveChanges();
        }

        // ── Check if settings exist ───────────────────────────────
        public bool ExistsBySalesNo(string salesNo)
        {
            return _db.PanelSettings
                      .Where(x => x.SalesNo   == salesNo
                               && x.IsActive == true)
                      .Any();
        }

        // ── Autocomplete — search enquiry codes ───────────────────
        public List<string> SearchEnquiryCodes(string term)
        {
            return _db.EnquiryMasters
                      .Where(x => x.SalesNO.Contains(term)
                               && x.IsActive == true)
                      .OrderBy(x => x.SalesNO)
                      .Select(x => x.SalesNO)
                      .Take(10)
                      .ToList();
        }

        public string GetCategoryBySalesNo(string salesNo)
        {
           int CompId =  GetComponentID(salesNo);

            return _db.ComponentTables
        .Where(e => e.ComponentID == CompId)
        .Select(e => e.Category)
        .FirstOrDefault() ?? "";
        }

        private int GetComponentID(string salesNo)
        {
            return _db.EnquiryMasters
                             .Where(x => x.SalesNO == salesNo)
                             .Select(x => x.ComponentID)
                             .FirstOrDefault();
        }

        public string GetPaintboothType(string salesNo)
        {
            return _db.EnquiryMasters.Where(x => x.SalesNO == salesNo && x.IsActive == true).Select(x => x.SubTypeOfDraftType).FirstOrDefault();
        }
    }
}
