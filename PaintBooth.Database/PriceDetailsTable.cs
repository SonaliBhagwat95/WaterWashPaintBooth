using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PaintBooth.Database
{
    public class PriceDetailsTable
    {
        [Key]
        public int PID { get; set; }
        [ForeignKey("EnquiryId")]
        public int EnquiryId { get; set; }
        public string SalesNo { get; set; }
        public int RevisionNo { get; set; } = 0;
        public decimal TotalPriceOfRawMaterials { get; set; }
        public decimal TotalPriceOfBoughtOut { get; set; }
        public int? CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime ModifiedDate { get; set; }
        public int ModifiedBy { get; set; }
        public bool IsActive { get; set; }
        public bool IsBOApproved { get; set; }
        public decimal LabourCost { get; set; }
        public decimal EAndCCost { get; set; }
        public decimal FreightCost { get; set; }
        public decimal BasicCost { get; set; }
        public decimal BestPrice { get; set; }
        public decimal Insurance { get; set; }
        public decimal CommercialFactor { get; set; }
        public decimal IncentiveFactor { get; set; }
        public decimal TPCCost { get; set; }
        public decimal DesignChargesCost { get; set; }
        public decimal POVALUE { get; set; }
        public decimal TVCCost { get; set; }
        public decimal PandFCost { get; set; }
        public bool ConfirmPriceBID { get; set; }

        // ── Rate fields — ADD THESE ───────────────────────────
        public decimal EnCRate { get; set; }
        public decimal PnFRate { get; set; }
        public decimal FreightRate { get; set; }
        public decimal DesignChargesRate { get; set; }
        public decimal ContingencyRate { get; set; }
        public decimal ContingencyCost { get; set; }
        public decimal InsuranceRate { get; set; }
        public decimal CommercialFactorRate { get; set; }
        public decimal IncentiveFactorRate { get; set; }
        public decimal TPCRate { get; set; }
        public decimal PORate { get; set; }
        public decimal NegotiationRate { get; set; }
        public decimal NegotiationCost { get; set; }
    }
}
