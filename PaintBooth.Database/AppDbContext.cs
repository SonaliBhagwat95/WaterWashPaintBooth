

using Microsoft.EntityFrameworkCore;
using PaintBooth.database;

namespace PaintBooth.Database
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        // ── DB Tables ─────────────────────────────────────────────
        public DbSet<User>     Users     { get; set; }
        public DbSet<UserRole> UserRoles { get; set; }
        public DbSet<EnquiryMaster> EnquiryMasters { get; set; }
        public DbSet<CustomerMaster> CustomerMasters { get; set; }
        public DbSet<StateMaster> StateMasters { get; set; }
        public DbSet<CityMaster> CityMasters { get; set; }
        public DbSet<CustomerContactPerson> CustomerContactPersons { get; set; }
        public DbSet<PanelSetting> PanelSettings { get; set; }
        //public DbSet<PaintBoothDesign> PaintBoothDesigns { get; set; }
        
        public DbSet<ComponentTable> ComponentTables { get; set; }
        public DbSet<MotorDetails> MotorDetails { get; set; }
        public DbSet<PressureDrop> PressureDrop { get; set; }

        public DbSet<MotorFlange> MotorFlange { get; set; }
        public DbSet<BlowerDetails> BlowerDetails { get; set; }
        public DbSet<MetalBaffleDetails> MetalBaffleDetails { get; set; }
        public DbSet<FilterFrameDetails> FilterFrameDetails { get; set; }
        public DbSet<PanelDetails> PanelDetails { get; set; }
        public DbSet<TubeLightDetails> TubeLightDetails { get; set; }
        public DbSet<PaintBoothDetails> PaintBoothDetails { get; set; }
        public DbSet<PriceDetailsTable> PriceDetailsTable { get; set; }
        public DbSet<PaymentTerms> PaymentTerms { get; set; }
        public DbSet<CostingAdditionalRM> CostingAdditionalRM { get; set; }
        public DbSet<BOAdditionalItems> BOAdditionalItems { get; set; }
        public DbSet<ENCCalculation> ENCCalculation { get; set; }

        public DbSet<ExhaustDuctings> ExhaustDuctings { get; set; }
        public DbSet<MOC> MOC { get; set; }
        public DbSet<WaterColumnDetail> WaterColumnDetail { get; set; }
        public DbSet<BOItemRate> BOItemRate { get; set; }
        public DbSet<ExceptionHandler> ExceptionHandler { get; set; }
        public DbSet<EnquirySalesEnDetails> EnquirySalesEndDetails { get; set; }


        // ── Primary keys ──────────────────────────────────────

        protected override void OnModelCreating(ModelBuilder mb)
        {
            // ── Table names ───────────────────────────────────────
            mb.Entity<User>().ToTable("Users");
            mb.Entity<UserRole>().ToTable("UserRoles");
            //mb.Entity<Customer>().ToTable("Customers");
            mb.Entity<EnquiryMaster>().ToTable("EnquiryMaster");

            mb.Entity<CustomerMaster>().ToTable("CustomerMaster");
            mb.Entity<StateMaster>().ToTable("StateMaster");
            mb.Entity<CityMaster>().ToTable("CityMaster");

            mb.Entity<CustomerMaster>().HasKey(x => x.CustomerID);
            mb.Entity<StateMaster>().HasKey(x => x.StateId);
            mb.Entity<CityMaster>().HasKey(x => x.CityId);
            mb.Entity<CostingAdditionalRM>().ToTable("CostingAdditionalRM");
            mb.Entity<BOAdditionalItems>().ToTable("BOAdditionalItems");
            mb.Entity<ENCCalculation>().ToTable("ENCCalculation");
            mb.Entity<EnquirySalesEnDetails>().ToTable("EnquirySalesEnDetails");

            mb.Entity<CustomerMaster>()
              .HasOne(x => x.State)
              .WithMany()
              .HasForeignKey(x => x.StateId);

            mb.Entity<CustomerMaster>()
              .HasOne(x => x.City)
              .WithMany()
              .HasForeignKey(x => x.CityId);
            mb.Entity<CustomerContactPerson>().ToTable("CustomerContactPerson");
            mb.Entity<CustomerContactPerson>().HasKey(x => x.ContactId);
            mb.Entity<PanelSetting>().ToTable("PanelSettings");
            mb.Entity<PanelSetting>().HasKey(x => x.SId);
            mb.Entity<ComponentTable>().ToTable("ComponentTable");
            mb.Entity<ComponentTable>().HasKey(x => x.ComponentID);

            mb.Entity<MotorDetails>().ToTable("MotorDetails");
            mb.Entity<MotorDetails>().HasKey(x => x.MotorID);

            // EnquiryMaster navigation to ComponentTable (optional but useful):
            mb.Entity<EnquiryMaster>()
              .HasOne<ComponentTable>()
              .WithMany()
              .HasForeignKey(x => x.ComponentID)
              .IsRequired(false);



            // ── Primary keys ──────────────────────────────────────
            mb.Entity<User>().HasKey(x => x.EmpId);
            mb.Entity<UserRole>().HasKey(x => x.RoleId);
            
        }
    }

}

