using Microsoft.AspNetCore.Mvc.Rendering;
using PaintBooth.Database;
using PaintBooth.Models;
using PaintBooth.Repositories.Interface;

namespace PaintBooth.Repositories
{
    public class EnquiryRepository(AppDbContext _db) : IEnquiryRepository
    {

        // ── Get all enquiries for Index list ──────────────────────
        public List<EnquiryListViewModel> GetAll()
        {
            // Join EnquiryMaster → ComponentTable via ComponentID
            return (from e in _db.EnquiryMasters
                    join c in _db.ComponentTables
                        on e.ComponentID equals c.ComponentID
                        into cj
                    join Cust in _db.CustomerMasters on e.CustomerID equals Cust.CustomerID
                    from c in cj.DefaultIfEmpty()
                    where e.IsActive == true
                    orderby e.EnquiryDate descending
                    select new EnquiryListViewModel
                    {
                        EnquiryID = e.EnquiryID,
                        SalesNO = e.SalesNO,
                        CompanyName = Cust.CompanyName,
                        Component = c != null ? c.Component : "",
                        DraftType = e.DraftType == "1" ? "Side Draught" :
                        e.DraftType == "2" ? "Down Draught" :
                        e.DraftType == "3" ? "Semi Down Draught" : "",
                        Category = c.Category ?? "",
                        EnquiryDate = e.EnquiryDate,
                        IsActive = e.IsActive
                    })
                    .ToList();
        }
        public EnquiryListViewModel GetCustomer(int customerId)
        {
            // Join EnquiryMaster → ComponentTable via ComponentID
            return (from e in _db.EnquiryMasters
                    join c in _db.CustomerMasters
                        on e.CustomerID equals c.CustomerID
                        into cj
                    join cp in _db.CustomerContactPersons on e.CustomerID equals cp.CustomerID
                    from c in cj.DefaultIfEmpty()
                    where e.IsActive == true && e.CustomerID == customerId
                    orderby e.EnquiryDate descending
                    select new EnquiryListViewModel
                    {
                        EnquiryID = e.EnquiryID,
                        CompanyName = c.CompanyName,
                        Address = c != null ? c.CustomerAddress : "",
                        ContactPerson = cp.ContactPerson ?? "",
                        Designation = cp.Designation ?? "",
                        MobileNo = cp.Mobile,
                        Email = cp.EmailId
                    }).FirstOrDefault();

        }
        // ── Get single EnquiryMaster ──────────────────────────────
        public EnquiryMaster? GetById(int enquiryId)
        {
            return _db.EnquiryMasters
                      .Where(x => x.EnquiryID == enquiryId
                               && x.IsActive == true)
                      .FirstOrDefault();
        }

        // ── Get ComponentTable for an enquiry ─────────────────────
        public ComponentTable? GetComponentByEnquiryId(int enquiryId)
        {
            var enquiry = _db.EnquiryMasters
                             .Where(x => x.EnquiryID == enquiryId)
                             .FirstOrDefault();

            if (enquiry == null) return null;

            return _db.ComponentTables
                      .Where(x => x.ComponentID == enquiry.ComponentID
                               && x.IsActive == true)
                      .FirstOrDefault();
        }

        // ── Add — insert ComponentTable first, then EnquiryMaster ─
        //public int Add(EnquiryMaster enquiry, ComponentTable component,MotorDetails motor)
        //{
        //    if (enquiry.SubTypeOfDraftType == "1")
        //    {
        //        component.ComponentEntry = "";
        //        component.DoorType = "";
        //        component.DoorSubType = "";
        //        component.SideDoorLOcation = "";
        //        component.TypeOfHingedDoor = "";
        //        component.TypeOfHinges = "";
        //    }
        //    // Step 1: save ComponentTable
        //    component.IsActive   = true;
        //    component.CreatedDate = DateTime.UtcNow;
        //    _db.ComponentTables.Add(component);
        //    _db.SaveChanges();

        //    // Step 2: link ComponentID to EnquiryMaster
        //    enquiry.ComponentID    = component.ComponentID;
        //    enquiry.CreatedDate    = DateTime.UtcNow;
        //    enquiry.IsActive      = true;
        //    _db.EnquiryMasters.Add(enquiry);
        //    _db.SaveChanges();

        //    motor.MotorTypes=motor.MotorTypes;
        //    motor.EnquiryID= enquiry.EnquiryID;
        //    motor.IsActive = true;
        //    motor.MotorCatalogID = 1;
        //    _db.MotorDetails.Add(motor);
        //    _db.SaveChanges();

        //    return enquiry.EnquiryID;
        //}
        public int Add(EnquiryMaster enquiry, ComponentTable component, MotorDetails motor)
        {
            // ── Null defaults for string fields ──────────────────────
            component.Category = component.Category ?? "";
            component.Component = component.Component ?? "";
            component.Shape = component.Shape ?? "";
            component.Orientation = component.Orientation ?? "";
            component.MaterialofConstruction = component.MaterialofConstruction ?? "";
            component.OverheadConveyorSubTypes = component.OverheadConveyorSubTypes ?? "";
            component.ConveyorNumber = component.ConveyorNumber ?? "";
            component.Image_Path = component.Image_Path ?? "";
            component.Conveyor = component.Conveyor ?? "";
            component.LoadingUnloading = component.LoadingUnloading ?? "";
            component.ComponentHandling = component.ComponentHandling ?? "";
            component.TypeOfPaint = component.TypeOfPaint ?? "";
            component.TypeOfPowder = component.TypeOfPowder ?? "";
            component.ConsumptionPerDay = component.ConsumptionPerDay ?? "";
            component.ExtractionChamberHeight = component.ExtractionChamberHeight ?? "";
            component.ComponentEntry = component.ComponentEntry ?? "";
            component.DoorType = component.DoorType ?? "";
            component.DoorSubType = component.DoorSubType ?? "";
            component.SideDoorLOcation = component.SideDoorLOcation ?? "";
            component.TypeOfHingedDoor = component.TypeOfHingedDoor ?? "";
            component.TypeOfHinges = component.TypeOfHinges ?? "";
            // ── Shape/Orientation dimension mapping ──────────────────────
            var shape = component.Shape ?? "";
            var orientation = component.Orientation ?? "";

            if (shape == "Rectangular" && orientation == "Vertical")
            {
                var temp = component.Length;
                component.Length = component.HeightSize;
                component.HeightSize = temp;
            }
            else if (shape == "Circular" && orientation == "Horizontal")
            {
                component.Length = component.Diameter ?? 0;
                component.WidthSize = component.Diameter ?? 0;
                component.HeightSize = component.CircularHeightSize ?? 0;
            }
            else if (shape == "Circular" && orientation == "Vertical")
            {
                component.Length = component.Diameter ?? 0;
                component.WidthSize = component.CircularHeightSize ?? 0;
                component.HeightSize = component.Diameter ?? 0;
            }



            // ── Null defaults for numeric fields ─────────────────────
            component.Length = component.Length ?? 0;
            component.WidthSize = component.WidthSize ?? 0;
            component.HeightSize = component.HeightSize ?? 0;
            component.Weight = component.Weight ?? 0;
            component.CarringCapacity = component.CarringCapacity ?? 0;
            component.SurfaceArea = component.SurfaceArea ?? 0;
            component.WallThickness = component.WallThickness ?? 0;
            component.ProductionRequirement = component.ProductionRequirement ?? 0;
            component.Workingdays = component.Workingdays ?? 0;
            component.NumberofShifts = component.NumberofShifts ?? 0;
            component.EffectiveWorking = component.EffectiveWorking ?? 0;
            component.Pitch = component.Pitch ?? 0;
            component.Speed = component.Speed ?? 0;
            component.NoOfColors = component.NoOfColors ?? 0;
            component.NoOfCoats = component.NoOfCoats ?? 0;
            component.Viscosity = component.Viscosity ?? 0;
            component.Paint = component.Paint ?? 0;
            component.Powder = component.Powder ?? 0;
            component.DFT = component.DFT ?? 0;
            component.SpecificHeat = component.SpecificHeat ?? 0;
            component.DoorWidth = component.DoorWidth ?? 0;
            component.DoorHeight = component.DoorHeight ?? 0;
            component.WatchGlassBottomDistance = component.WatchGlassBottomDistance ?? 0;
            component.WatchglassHeight = component.WatchglassHeight ?? 0;
            component.WatchglassWidth = component.WatchglassWidth ?? 0;
            component.ModifiedBy = 0;
            component.ModifiedDate = DateTime.Now;
            // ── Bool defaults ─────────────────────────────────────────
            component.chkWatchDoor = component.chkWatchDoor ?? false;

            // ── Door fields override for SubType 1 ───────────────────
            if (enquiry.SubTypeOfDraftType == "1")
            {
                component.ComponentEntry = "";
                component.DoorType = "";
                component.DoorSubType = "";
                component.SideDoorLOcation = "";
                component.TypeOfHingedDoor = "";
                component.TypeOfHinges = "";
            }

            // ── Step 1: Save ComponentTable ───────────────────────────
            component.IsActive = true;
            component.CreatedDate = DateTime.Now;
            _db.ComponentTables.Add(component);
            _db.SaveChanges();

            // ── Step 2: Link ComponentID to EnquiryMaster ─────────────
            enquiry.ComponentID = component.ComponentID;
            enquiry.CreatedDate = DateTime.Now;
            enquiry.SideLocationDoorFAS = enquiry.SideLocationDoorFAS ?? "";
            enquiry.FilterTypes = enquiry.FilterTypes ?? "";
            enquiry.IsActive = true;
            _db.EnquiryMasters.Add(enquiry);
            _db.SaveChanges();

            // ── Step 3: Save MotorDetails ─────────────────────────────
            motor.MotorTypes = motor.MotorTypes ?? "";
            motor.EnquiryID = enquiry.EnquiryID;
            motor.IsActive = true;
            motor.MotorCatalogID = 1;
            _db.MotorDetails.Add(motor);
            _db.SaveChanges();

            return enquiry.EnquiryID;
        }
        public MotorDetails GetMotorDetailsByEnquiryId(int enquiryId)
        {
            return _db.MotorDetails
                .Where(x => x.EnquiryID == enquiryId && x.IsActive == true).FirstOrDefault();
        }



        // ── Update — update both tables ───────────────────────────
        public void Update(EnquiryMaster enquiry, ComponentTable component, MotorDetails motor)
        {
            // ── Update EnquiryMaster ──────────────────────────────
            var existingEnquiry = _db.EnquiryMasters
                                     .Where(x => x.EnquiryID == enquiry.EnquiryID && x.IsActive == true)
                                     .FirstOrDefault();

            if (existingEnquiry != null)
            {
                existingEnquiry.CustomerID = enquiry.CustomerID;
                existingEnquiry.EnquiryDate = enquiry.EnquiryDate;
                existingEnquiry.SalesNO = enquiry.SalesNO;
                existingEnquiry.DraftType = enquiry.DraftType;
                existingEnquiry.SubTypeOfDraftType = enquiry.SubTypeOfDraftType;
                existingEnquiry.FilterTypes = enquiry.FilterTypes;
                existingEnquiry.PlenumHeight = enquiry.PlenumHeight;
                existingEnquiry.SideLocationDoorFAS = enquiry.SideLocationDoorFAS == null ? "" : enquiry.SideLocationDoorFAS;
                existingEnquiry.MaintainanceDoorWidth = enquiry.MaintainanceDoorWidth;
                existingEnquiry.MaintainanceDoorHeight = enquiry.MaintainanceDoorHeight;
                existingEnquiry.IsMaintenanceDoorAdded = enquiry.IsMaintenanceDoorAdded;
                existingEnquiry.SalesEngineerId = enquiry.SalesEngineerId;
                existingEnquiry.ModifiedBy = enquiry.ModifiedBy;
                existingEnquiry.ModifiedDate = DateTime.Now;
            }

            // ── Update ComponentTable ─────────────────────────────
            var existingComponent = _db.ComponentTables
                                       .Where(x => x.ComponentID == component.ComponentID)
                                       .FirstOrDefault();

            if (existingComponent != null)
            {
                existingComponent.Category = component.Category;
                existingComponent.Component = component.Component;
                existingComponent.Shape = component.Shape;
                existingComponent.Orientation = component.Orientation;
                existingComponent.Length = component.Length;
                existingComponent.WidthSize = component.WidthSize;
                existingComponent.HeightSize = component.HeightSize;
                existingComponent.Weight = component.Weight;
                existingComponent.CarringCapacity = component.CarringCapacity;
                existingComponent.MaterialofConstruction = component.MaterialofConstruction;
                existingComponent.OverheadConveyorSubTypes = component.OverheadConveyorSubTypes;
                existingComponent.ConveyorNumber = component.ConveyorNumber;
                existingComponent.SurfaceArea = component.SurfaceArea;
                existingComponent.WallThickness = component.WallThickness;
                existingComponent.ProductionRequirement = component.ProductionRequirement;
                existingComponent.Workingdays = component.Workingdays;
                existingComponent.NumberofShifts = component.NumberofShifts;
                existingComponent.Image_Path = component.Image_Path ?? "";
                existingComponent.EffectiveWorking = component.EffectiveWorking ?? 0;
                existingComponent.Conveyor = component.Conveyor ?? "";
                existingComponent.Pitch = component.Pitch ?? 0;
                existingComponent.Speed = component.Speed ?? 0;
                existingComponent.LoadingUnloading = component.LoadingUnloading;
                existingComponent.ComponentHandling = component.ComponentHandling;
                existingComponent.NoOfColors = component.NoOfColors;
                existingComponent.NoOfCoats = component.NoOfCoats;
                existingComponent.Viscosity = component.Viscosity;
                existingComponent.Paint = component.Paint;
                existingComponent.Powder = component.Powder ?? 0;
                existingComponent.TypeOfPaint = component.TypeOfPaint ?? "";
                existingComponent.TypeOfPowder = component.TypeOfPowder ?? "";
                existingComponent.DFT = component.DFT;
                existingComponent.SpecificHeat = component.SpecificHeat;
                existingComponent.ConsumptionPerDay = component.ConsumptionPerDay;
                existingComponent.ExtractionChamberHeight = component.ExtractionChamberHeight;
                existingComponent.ComponentEntry = component.ComponentEntry ?? "";
                existingComponent.DoorType = component.DoorType ?? "";
                existingComponent.DoorSubType = component.DoorSubType ?? "";
                //existingComponent.SideDoorLOcation          = component.SideDoorLOcation ?? "";
                existingComponent.TypeOfHingedDoor = component.TypeOfHingedDoor ?? "";
                existingComponent.DoorWidth = component.DoorWidth ?? 0;
                existingComponent.DoorHeight = component.DoorHeight ?? 0;
                existingComponent.TypeOfHinges = component.TypeOfHinges ?? "";
                existingComponent.WashingChamberTank = component.WashingChamberTank;
                existingComponent.MOC = component.MOC;
                existingComponent.ModifiedBy = component.ModifiedBy;
                existingComponent.ModifiedDate = DateTime.Now;
            }

            //update Motor details
            var existingMotorTypes = _db.MotorDetails
                                     .Where(X => X.EnquiryID == motor.EnquiryID)
                                     .FirstOrDefault();

            if (existingMotorTypes != null)
            {
                existingMotorTypes.EnquiryID = motor.EnquiryID;
                existingMotorTypes.MotorTypes = motor.MotorTypes;
            }
            _db.SaveChanges();
        }

        // ── Soft delete ───────────────────────────────────────────
        public void Delete(int enquiryId, int deletedBy)
        {
            var enquiry = _db.EnquiryMasters
                             .Where(x => x.EnquiryID == enquiryId)
                             .FirstOrDefault();

            if (enquiry == null) return;

            enquiry.IsActive = false;
            enquiry.ModifiedBy = deletedBy;
            enquiry.ModifiedDate = DateTime.Now;

            _db.SaveChanges();
        }

        // ── Check duplicate SalesNO ───────────────────────────────
        public bool SalesNoExists(string salesNo, int? excludeId = null)
        {
            return _db.EnquiryMasters
                      .Where(x => x.SalesNO == salesNo
                               && x.IsActive == true
                               && (excludeId == null
                                   || x.EnquiryID != excludeId))
                      .Any();
        }

        // ── Company name autocomplete ─────────────────────────────
        public List<string> SearchCompanyNames(string term)
        {
            return _db.CustomerMasters
                      .Where(x => x.CompanyName.Contains(term)
                               && x.IsActive == true)
                      .OrderBy(x => x.CompanyName)
                      .Select(x => x.CompanyName)
                      .Take(10)
                      .ToList();
        }

        // ── Get company + contact info when name is selected ──────
        public CompanyInfoViewModel? GetCompanyInfo(string companyName)
        {
            var customer = _db.CustomerMasters
                              .Where(x => x.CompanyName == companyName
                                       && x.IsActive == true)
                              .FirstOrDefault();

            if (customer == null) return null;

            var contact = _db.CustomerContactPersons
                             .Where(x => x.CustomerID == customer.CustomerID
                                      && x.IsActive == true)
                             .FirstOrDefault();

            return new CompanyInfoViewModel
            {
                CustomerID = customer.CustomerID,
                CompanyName = customer.CompanyName,
                CustomerAddress = customer.CustomerAddress,
                ContactPerson = contact?.ContactPerson ?? "",
                Designation = contact?.Designation ?? customer.Designation,
                MobileNo = contact?.Mobile ?? "",
                EmailId = contact?.EmailId ?? ""
            };
        }

        public List<SelectListItem> GetSalesEngineersDropdown()
        {
            return _db.EnquirySalesEndDetails
                .Where(e => e.EngineerName != null)
                .Select(e => new SelectListItem
                {
                    Value = e.Id.ToString(),
                    Text = e.EngineerName
                })
                .ToList();
        }

        #region Coded Added by Ajay 

        // ── Find existing customer by name, or create new one from ERP/manual data ──
        public int GetOrCreateCustomer(EnquiryListViewModel customerInfo)
        {
            if (customerInfo == null || string.IsNullOrWhiteSpace(customerInfo.CompanyName))
                return 0;

            var existing = _db.CustomerMasters
                .Where(x => x.CompanyName == customerInfo.CompanyName && x.IsActive == true)
                .FirstOrDefault();

            if (existing != null)
                return existing.CustomerID;

            // ── Try to match ERP customer_location to a known City ──────
            CityMaster? matchedCity = null;

            if (!string.IsNullOrWhiteSpace(customerInfo.CustomerLocation))
            {
                var locationTrimmed = customerInfo.CustomerLocation.Trim();

                matchedCity = _db.CityMasters
                    .Where(c => c.IsActive == true
                             && c.CityName.ToLower() == locationTrimmed.ToLower())
                    .FirstOrDefault();
            }

            // ── Fallback if no city match found (e.g. "vasai" isn't in your table) ──
            int resolvedCityId;
            int resolvedStateId;

            if (matchedCity != null)
            {
                resolvedCityId = matchedCity.CityId;
                resolvedStateId = matchedCity.StateId;
            }
            else
            {
                // Fallback default — adjust to whichever city/state makes sense as your default
                var fallbackCity = _db.CityMasters
                    .Where(c => c.CityName == "Mumbai" && c.IsActive == true)
                    .FirstOrDefault();

                resolvedCityId = fallbackCity?.CityId ?? 1;   // Mumbai = CityId 1 per your data
                resolvedStateId = fallbackCity?.StateId ?? 1; // Maharashtra = StateId 1
            }

            var newCustomer = new CustomerMaster
            {
                CompanyName = customerInfo.CompanyName,
                CustomerAddress = string.IsNullOrWhiteSpace(customerInfo.Address) ? "Not Provided" : customerInfo.Address,
                Designation = string.IsNullOrWhiteSpace(customerInfo.Designation) ? "Not Provided" : customerInfo.Designation,
                StateId = resolvedStateId,
                CityId = resolvedCityId,
                ContactPerson = customerInfo.ContactPerson ?? "",
                Mobile = customerInfo.MobileNo ?? "",
                EmailId = customerInfo.Email ?? "",
                IsActive = true,
                CreatedDate = DateTime.UtcNow
            };
            _db.CustomerMasters.Add(newCustomer);
            _db.SaveChanges();

            var newContact = new CustomerContactPerson
            {
                CustomerID = newCustomer.CustomerID,
                ContactPerson = customerInfo.ContactPerson ?? "",
                Designation = customerInfo.Designation ?? "",
                Mobile = customerInfo.MobileNo ?? "",
                EmailId = customerInfo.Email ?? "",
                IsActive = true
            };
            _db.CustomerContactPersons.Add(newContact);
            _db.SaveChanges();

            return newCustomer.CustomerID;
        }

        // ── Get local EnquiryDate for a batch of SalesNO values (used for ERP grid) ──
        public Dictionary<string, DateTime> GetEnquiryDatesBySalesNos(List<string> salesNos)
        {
            return _db.EnquiryMasters
                .Where(e => e.IsActive == true && salesNos.Contains(e.SalesNO))
                .ToDictionary(e => e.SalesNO, e => e.EnquiryDate);
        }
        #endregion
    }
}
