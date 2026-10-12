
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PaintBooth.Database;
using PaintBooth.Models;
using PaintBooth.Repositories.Interface;

namespace PaintBooth.Repositories
{
    public class PaintBoothRepository(AppDbContext _db) : IPaintBoothRepository
    {


        // ── Get all ───────────────────────────────────────────────
        public List<PaintBoothListViewModel> GetAll()
        {
            return (from pb in _db.PaintBoothDetails
                    join e in _db.EnquiryMasters
                        on pb.EnquiryId equals e.EnquiryID

                    // LEFT JOIN BlowerDetails
                    join b in _db.BlowerDetails
                        on pb.EnquiryId equals b.EnquiryID
                        into blowerGroup
                    from b in blowerGroup.DefaultIfEmpty()

                        // LEFT JOIN MotorDetails
                    join m in _db.MotorDetails
                        on pb.EnquiryId equals m.EnquiryID
                        into motorGroup
                    from m in motorGroup.DefaultIfEmpty()

                    where pb.IsActive == true
                    orderby pb.CreatedDate descending

                    select new PaintBoothListViewModel
                    {
                        PaintBoothID = pb.PaintBoothID,
                        SalesNO = e.SalesNO,
                        W = (decimal)pb.W,
                        H = (decimal)pb.H,
                        D = (decimal)pb.D,
                        BlowerItemCode = b != null ? b.BlowerItemCode : "",
                        MotorItemCode = m != null ? m.MotorItemCode : ""
                    })
                    .ToList();
        }


        // ── Get by SalesNo ────────────────────────────────────────
        public PaintBoothDetails? GetBySalesNo(string salesNo)
        {
            int enquiryId = GetEnquiryID(salesNo);
            return _db.PaintBoothDetails
                      .Where(x => x.EnquiryId == enquiryId
                               && x.IsActive == true)
                      .FirstOrDefault();
        }
        public int GetEnquiryID(string enquiryno)
        {
            return _db.EnquiryMasters
            .Where(x => x.SalesNO == enquiryno && x.IsActive==true)
            .Select(x => x.EnquiryID).FirstOrDefault();
        }

        public int GetComponentID(string enquiryno)
        {
            return _db.EnquiryMasters
            .Where(x => x.SalesNO == enquiryno && x.IsActive == true)
            .Select(x => x.ComponentID).FirstOrDefault();
        }
       
        // ── Enquiry code autocomplete ─────────────────────────────
        public List<string> SearchEnquiryCodes(string term)
        {
            //return _db.PanelSettings
            //          .Where(x => x.SalesNo.Contains(term)
            //                   && x.IsActive == true)
            //          .OrderBy(x => x.SalesNo)
            //          .Select(x => x.SalesNo)
            //          .Take(10)
            //          .ToList();
            return _db.PanelSettings
              .Where(x => x.SalesNo.Contains(term) && x.IsActive)
              .Select(x => x.SalesNo)
              .Distinct()          // removes duplicate SalesNo
              .OrderBy(x => x)
              .ToList();
        }

        public ComponentTable GetWatchGlassDetails(int ComponentId)
        {
            return _db.ComponentTables.Where(X => X.ComponentID == ComponentId && X.IsActive == true).FirstOrDefault();
        }
        public BlowerDetails GetBlowerDetails(int enquiryId)
        {
            return _db.BlowerDetails.Where(x => x.EnquiryID == enquiryId && x.IsActive == true).FirstOrDefault();
        }
        // ── Load enquiry + setting data when Search clicked ───────
        public EnquirySearchResultViewModel? GetEnquiryData(string salesNo)
        {
            var enquiry = _db.EnquiryMasters
                             .Where(x => x.SalesNO == salesNo
                                      && x.IsActive == true)
                             .FirstOrDefault();

            if (enquiry == null) return null;

            // Get panel dimensions from PanelSettings
            var setting = _db.PanelSettings
                             .Where(x => x.SalesNo == salesNo
                                      && x.SettingType == "Paintbooth"
                                      && x.IsActive == true)
                             .FirstOrDefault();

            var component  =   _db.ComponentTables
                               .Where(x=>x.ComponentID==enquiry.ComponentID && x.IsActive==true)
                               .FirstOrDefault();

            var motor = _db.MotorDetails
                               .Where(x => x.EnquiryID == enquiry.EnquiryID && x.IsActive == true)
                               .FirstOrDefault();

            return new EnquirySearchResultViewModel
            {
                Length = component.Length ?? 0,
                Width = component.WidthSize ?? 0,
                Height = component.HeightSize ?? 0,
                ExtractionChamberHeight = component.ExtractionChamberHeight,
                MotorTypes = motor?.MotorTypes ?? "NFLP",
                PanelWidth = (decimal)(setting?.PanelWidth ?? 0),
                PanelHeight = (decimal)(setting?.PanelHeight ?? 0),
                SubTypeOfDraftType = enquiry.SubTypeOfDraftType,
                Category= component.Category
            };
        }

        // ── Setting details for modal popup ───────────────────────
        public List<SettingItemViewModel> GetSettingDetails(string salesNo)
        {
            return _db.PanelSettings
                      .Where(x => x.SalesNo == salesNo
                               && x.IsActive == true)
                      .Select(x => new SettingItemViewModel
                      {
                          SId = x.SId,
                          SettingType = x.SettingType,
                          PanelWidth = (double)x.PanelWidth,
                          PanelHeight = (double)x.PanelHeight ,
                          SheetThickness = (double)x.SheetThickness,
                          Section = x.Section ?? "",
                          BendStructural = x.BendStructural ?? "",
                          Materials = x.Materials ?? "",
                          StandardBend1 = (double)x.StandardBend1 ,
                          StandardBend2 = (double)x.StandardBend2 ,
                          PitchDistance = (double)x.PitchDistance ,
                          SlotDimensions = x.SlotDimensions ?? "",
                          LightTypes = x.LightTypes ?? "",
                          LightSubTypes = x.LightSubTypes ?? "",
                          LuxLevel = (decimal)x.LuxLevel,
                          Lumens = (decimal)x.Lumens
                      })
                      .ToList();
        }
        public List<PaintBooth.Database.PressureDrop> GetAallWaterColumnDetails()
        {
            return _db.PressureDrop.Where(x => x.IsActive == true).ToList();
        }
        public List<MotorFlangeModel> GetMotorDetails(decimal BlowerHpCalculation)
        {
            // Fetching and mapping records to MotorFlangeModel
            var records = _db.MotorFlange
                .Where(record => record.RatedOutputHP > BlowerHpCalculation)
                .Select(record => new MotorFlangeModel // Create instances of MotorFlangeModel
                {
                    MotorCatalogID = record.MotorCatalogID,
                    RatedOutputHP = record.RatedOutputHP,
                    Framesize = record.Framesize,
                    TypeReference = record.TypeReference,
                    RatedSpeed = record.RatedSpeed,
                    RatedCurrent = record.RatedCurrent,
                    EfficiencyFullLoad = record.EfficiencyFullLoad
                })
                .OrderBy(record => record.RatedOutputHP)
                .Take(3)
                .ToList();

            return records;
        }

        public string FetchSubTypeOfDraft(string EnquiryCode)
        {
            return _db.EnquiryMasters.Where(c => c.SalesNO == EnquiryCode && c.IsActive == true).Select(c => c.SubTypeOfDraftType).FirstOrDefault();
        }
        public string FetchPaintboothType(int EnquiryId)
        {
            return _db.EnquiryMasters.Where(c => c.EnquiryID == EnquiryId && c.IsActive == true).Select(c => c.SubTypeOfDraftType).FirstOrDefault();
        }
        public string FetchPlenumHeight(string EnquiryCode)
        {
            return _db.EnquiryMasters.Where(c => c.SalesNO == EnquiryCode && c.IsActive == true).Select(c => c.PlenumHeight).FirstOrDefault();
        }


        public void SaveMotorDeatils(int enquiryID, int MotorCatalogID, string MotorItemCode)
        {
            var savedRecord = _db.MotorDetails
                                        .FirstOrDefault(x => x.EnquiryID == enquiryID && x.IsActive == true);

            //  Update the Quantity value
            if (savedRecord != null)
            {
                savedRecord.MotorCatalogID = MotorCatalogID;
                savedRecord.MotorItemCode = MotorItemCode;
                _db.SaveChanges();
            }


        }

        #region Blower
        public void SaveBlowerDetails(int enquiryID, PaintBoothModel model)
        {
            DeleteOldBlowerDetails(enquiryID);
            BlowerDetails obj = new BlowerDetails()
            {
                EnquiryID = enquiryID,
                BlowerItemCode = model.BlowerItemCode,
                CapacityCublicMperHr = (decimal)model.CapacityofBlowerinH,
                RoundingUpCapacity = (decimal)model.CapacityofBlowerRoundOf,
                MountingTypes = model.Mountingtype,
                DriveTypes = model.Drivetype,
                WaterColumn = (decimal)model.WaterColumn,       
                BlowerHpCalculation = (decimal)model.BlowerHpCalculation, 
                CreatedDate = DateTime.Now,
                ModifiedDate = DateTime.Now,
                IsActive = true
            };
            _db.BlowerDetails.Add(obj);
            _db.SaveChanges();
        }
        public void DeleteOldBlowerDetails(int enquiryId)
        {
            var oldRecords = _db.BlowerDetails
                                .Where(x => x.EnquiryID == enquiryId)
                                .ToList();

            _db.BlowerDetails.RemoveRange(oldRecords);
            _db.SaveChanges();
        }
        #endregion

        public string FetchPaintBoothType(string EnquiryCode)
        {
            return _db.EnquiryMasters.Where(c => c.SalesNO == EnquiryCode && c.IsActive == true).Select(e => e.SubTypeOfDraftType).FirstOrDefault();
        }
        public string FetchCatagoryType(int ComponentId)
        {
            return _db.ComponentTables.Where(c => c.ComponentID == ComponentId && c.IsActive == true).Select(c => c.Category).FirstOrDefault();
        }

        public decimal FetchRatedOutputHP(int MotorCatalogID)
        {
            return (decimal)_db.MotorFlange.Where(c => c.MotorCatalogID == MotorCatalogID).Select(c => c.RatedOutputHP).FirstOrDefault();
        }
        public string GetMotorTypes(int MotorCatalogID)
        {
            return _db.MotorDetails.Where(c => c.MotorCatalogID == MotorCatalogID).Select(c => c.MotorTypes).FirstOrDefault();
        }

        public PaintBoothModel GetSettingDetailsByCode(string enquiryCode)
        {
            var paintBoothModel = new PaintBoothModel
            {
                Settings = _db.PanelSettings
                                   .Where(s => s.SalesNo == enquiryCode && s.IsActive==true)
                                   .Select(s => new SettingItemViewModel
                                   {
                                       SettingType = s.SettingType,
                                       SId = s.SId,
                                       SalesNo = s.SalesNo,
                                       PanelWidth = (double)s.PanelWidth,
                                       PanelHeight = (double)s.PanelHeight,
                                       SheetThickness = (double)s.SheetThickness,
                                       SlotDimensions = s.SlotDimensions,
                                       StandardBend1 = (double)s.StandardBend1,
                                       StandardBend2 = (double)s.StandardBend2,
                                       Materials = s.Materials,
                                       H = s.H,
                                       W = s.W,
                                       T = s.T,
                                       PitchDistance = (double)s.PitchDistance,
                                       Section = s.Section,
                                       LightTypes = s.LightTypes,
                                       LuxLevel = (decimal)s.LuxLevel,
                                       Lumens = (decimal)s.Lumens
                                   }).ToList()
            };
            return paintBoothModel;
        }

        public bool Exists(int enquiryId)
        {
            return _db.PaintBoothDetails
                .Any(pb => pb.EnquiryId == enquiryId && pb.designStatus == true);
        }
        public int SavePaintBooth(PaintBoothModel model,int enquiryId)
        {
            PaintBoothDetails objtbl = new PaintBoothDetails();
            try
            {
               
                    objtbl = new PaintBoothDetails();
                    objtbl.IsActive = true;
                    objtbl.EnquiryId = enquiryId;
                    objtbl.D1 = model.D1;
                    objtbl.D2 = model.D2;
                    objtbl.D3 = model.D3;
                    objtbl.W1 = model.W1;
                    objtbl.W2 = model.W2;
                    objtbl.W3 = model.W3;
                    objtbl.D = model.D1 + model.D2 + model.Depth;
                    objtbl.W = model.W1 + model.W2 + model.Width;
                    objtbl.H = model.H1 + model.H2 + model.Height;
                    objtbl.H1 = model.H1;
                    objtbl.H2 = model.H2;
                    objtbl.CrossSectionalAreaOfBlower = model.CrossSectionalArea;
                    objtbl.VelocityofBlower = model.VelocityofBlower;
                    objtbl.CapacityofBlowerincubicm = model.CapacityofBlower;
                    objtbl.CapacityofBlowerinHr = model.CapacityofBlowerinH;
                    objtbl.RoundingCapacity = (decimal)model.CapacityofBlowerRoundOf;
                    objtbl.NoOfBlowers = model.NoOfBlowers;
                    objtbl.CapacityPerBlower = model.CapacityofEachBlower;
                    objtbl.BetweenCenterDistance = model.BetweenCenterDistance == null ? 0 : model.BetweenCenterDistance;
                    objtbl.CreatedBy = 1;
                    objtbl.CreatedDate = DateTime.Now;
                    objtbl.ModifiedBy = 0;
                    objtbl.designStatus = true;
                    _db.PaintBoothDetails.Add(objtbl);
                
                _db.SaveChanges();

            }
            catch (Exception ex)
            {
                throw ex;
            }
            return 1;
        }
        public int UpdateBetweenCenterDistance(int enquiryId, decimal? betweenCenterDistance,PaintBoothModel model)
        {
            try
            {
                var record = _db.PaintBoothDetails.FirstOrDefault(x => x.EnquiryId == enquiryId);
                if (record != null)
                {
                    record.BetweenCenterDistance = (double)(betweenCenterDistance == null ? 0 : betweenCenterDistance);
                    record.NoOfBlowers = model.NoOfBlowers;
                    record.CapacityPerBlower = model.CapacityofEachBlower;
                    record.D1 = model.D1;
                    record.D2 = model.D2;
                    record.D3 = model.D3;
                    record.D = model.D;
                    record.W1 = model.W1;
                    record.W2 = model.W2;
                    record.W = model.W;
                    record.H = model.H;
                    record.H1 = model.H1;
                    record.H2 = model.H2;
                    record.ModifiedBy = 1;
                    record.ModifiedDate = DateTime.Now;
                    _db.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return 1;
        }
        public int FetchComponentId(int enquiryID)
        {
            return _db.EnquiryMasters.Where(c => c.EnquiryID == enquiryID).Select(c => c.ComponentID).FirstOrDefault();
        }

       
        public EnquiryMaster FetchMaintenanceDoorDetails(int enquiryID)
        {
            return _db.EnquiryMasters.Where(x => x.EnquiryID == enquiryID).FirstOrDefault();
        }
        public ComponentTable FetchDoorType(int componentEntryId)
        {
            return _db.ComponentTables.Where(x => x.ComponentID == componentEntryId && x.IsActive==true).FirstOrDefault();
        }
        public string FetchExtractionHeight(int ComponentID)
        {
            return _db.ComponentTables.Where(c => c.ComponentID == ComponentID).Select(c => c.ExtractionChamberHeight).FirstOrDefault();
        }

        public void DeletePaintBooth(int enquiryId)
        {
            // Soft delete PaintBoothDetails
            var paintBooth = _db.PaintBoothDetails
                                .Where(x => x.EnquiryId == enquiryId && x.IsActive == true)
                                .ToList();

            paintBooth.ForEach(x =>
            {
                x.IsActive = false;
                x.ModifiedDate = DateTime.UtcNow;
            });

            // Soft delete BlowerDetails
            var blower = _db.BlowerDetails
                            .Where(x => x.EnquiryID == enquiryId)
                            .ToList();

            blower.ForEach(x => x.IsActive = false);

            // Soft delete MotorDetails
            var motor = _db.MotorDetails
                           .Where(x => x.EnquiryID == enquiryId && x.IsActive == true)
                           .ToList();

            motor.ForEach(x => x.IsActive = false);

            _db.SaveChanges();
        }

        public void SaveWaterColumnDetails(int enquiryId, string waterColumnRowsJson)
        {
            // Soft delete existing rows
            var existing = _db.WaterColumnDetail
                              .Where(x => x.EnquiryID == enquiryId && x.IsActive == true)
                              .ToList();

            existing.ForEach(x =>
            {
                x.IsActive = false;
                x.ModifiedDate = DateTime.Now;
            });

            // Parse and insert new rows
            if (!string.IsNullOrWhiteSpace(waterColumnRowsJson))
            {
                var rows = System.Text.Json.JsonSerializer
                                 .Deserialize<List<WaterColumnRowDto>>(waterColumnRowsJson);

                if (rows != null)
                {
                    foreach (var row in rows)
                    {
                        _db.WaterColumnDetail.Add(new WaterColumnDetail
                        {
                            EnquiryID = enquiryId,
                            ItemNumber = row.ItemNumber,
                            Quantity = row.Quantity,
                            IsSelected = row.IsSelected,
                            IsActive = true,
                            CreatedDate = DateTime.Now,
                            ModifiedDate = DateTime.Now
                        });
                    }
                }
            }

            _db.SaveChanges();
        }

        public void UpdateWatchGlassDetails(int ComponentId,PaintBoothModel model)
        {
            // Soft delete existing rows
            var existing = _db.ComponentTables
                              .Where(x => x.ComponentID == ComponentId && x.IsActive == true)
                              .ToList();

            foreach (var item in existing)
            {
                item.chkWatchDoor = model.chkWatchDoor;
                item.WatchGlassBottomDistance = model.WatchGlassBottomDistance;
                item.WatchglassHeight = model.WatchglassHeight;
                item.WatchglassWidth = model.WatchglassWidth;
            }
            _db.SaveChanges();
        }
        public void UpdateDoorDetails(int ComponentId, PaintBoothModel model)
        {
            // Soft delete existing rows
            var existing = _db.ComponentTables
                              .Where(x => x.ComponentID == ComponentId && x.IsActive == true)
                              .ToList();

            foreach (var item in existing)
            {
                item.DoorWidth = model.HingedDoorWidth;
                item.DoorHeight = model.HingedDoorHeight;
            }
            _db.SaveChanges();
        }
        public List<WaterColumnDetail> GetWaterColumnDetails(int enquiryId)
        {
            return _db.WaterColumnDetail
                      .Where(x => x.EnquiryID == enquiryId && x.IsActive == true)
                      .ToList();
        }
        public MotorDetails? GetMotorBySalesNo(string salesNo)
        {
            int enquiryId = GetEnquiryID(salesNo);
            return _db.MotorDetails
                      .Where(x => x.EnquiryID == enquiryId && x.IsActive == true)
                      .FirstOrDefault();
        }

        public void UpdateTubelightDetails(PaintBoothModel model,decimal LuxLevel, decimal Lumens)
        {

            int TubeLightQuantity = 1;
            decimal PaintBoothArea = ((decimal)(model.W * model.D)) / 1000000m;//Area is in SquareMeters
           double TubeLightCalculations = (double)Math.Ceiling((PaintBoothArea * LuxLevel) / (Lumens * TubeLightQuantity * 0.5m));//0.5m is scaleing factor
            model.Lights = (int)TubeLightCalculations;

          
            // Step 2: Retrieve the saved record based on a unique identifier
            var savedRecord = _db.TubeLightDetails.FirstOrDefault(x => x.EnquiryID == model.EnquiryId || x.SalesNo == model.SalesNO);

            // Step 3: Update the Quantity value
            if (savedRecord != null)
            {
                savedRecord.Quantity = (int)TubeLightCalculations;

                // Step 4: Save the changes for the updated Quantity
                _db.SaveChanges();  // Only the Quantity column will be updated
            }
        }

        public string GetFilterType(int enquiryID)
        {
            return _db.EnquiryMasters.Where(z => z.EnquiryID == enquiryID && z.IsActive == true).Select(z => z.FilterTypes).FirstOrDefault();
        }

        public void SaveExhaustDucting(int enquiryId, string salesNo, PaintBoothModel model, int userId)
        {
            // deserialize the JSON the form posted
            List<ExhaustDuctingModel> list = new();
            if (!string.IsNullOrWhiteSpace(model.ExhaustDuctingListJson))
            {
                list = System.Text.Json.JsonSerializer.Deserialize<List<ExhaustDuctingModel>>(
                           model.ExhaustDuctingListJson,
                           new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true })
                       ?? new List<ExhaustDuctingModel>();
            }

            // soft-delete prior rows for this enquiry (avoid accumulation on re-save)
            var old = _db.ExhaustDuctings
                         .Where(x => x.EnquiryID == enquiryId && x.IsActive == true)
                         .ToList();
            old.ForEach(x => { x.IsActive = false; x.ModifiedBy = userId; x.ModifiedDate = DateTime.Now; });

            // build the new batch — shared dimensions come from the header fields
            var rows = list.Select(x => new ExhaustDuctings
            {
                BendType = x.BendType,
                BendDirection = x.BendDirection,
                DuctLength = x.DuctLength,
                DuctWidth = (decimal)model.ExhaustWidth,
                DuctHeight = (decimal)model.ExhaustDuctHeight,
                DuctThickness = (decimal)model.ExhaustThickness,
                DuctNetWeight = x.DuctNetWeight,
                DuctGrossWeight = x.DuctGrossWeight,
                SalesNo = salesNo,
                EnquiryID = enquiryId,
                CreatedBy = userId,
                CreatedDate = DateTime.Now,
                IsActive = true
            }).ToList();

            if (rows.Any())
                _db.ExhaustDuctings.AddRange(rows);   // batch insert, not per-row

            _db.SaveChanges();
        }
        public List<ExhaustDuctings> GetExhaustDuctingByEnquiry(int enquiryId)
        {
            return _db.ExhaustDuctings
                      .Where(x => x.EnquiryID == enquiryId && x.IsActive == true)
                      .OrderBy(x => x.DuctId)
                      .ToList();
        }
    }
}
