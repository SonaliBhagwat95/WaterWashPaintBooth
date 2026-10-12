using System;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PaintBooth.Models
{
    public class PriceBIDModel
    {

        public string SalesNO { get; set; }
        public decimal PriceOfRM { get; set; }
        public decimal PriceOfBO { get; set; }
        public bool IsPriceBidApproved { get; set; }
       public int RevisionNo { get; set; }
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
        public bool ConfirmPriceBID { get; set; }
        public decimal PandFCost { get; set; }
        public decimal TVCCost { get; set; }
        public decimal Throughputvalue { get; set; }
        public decimal Overheads { get; set; }
        public decimal ContingencyCost { get; set; }
        public decimal NegotiationCost { get; set; }
        //Payment terms
        public decimal AdvancedPercentage { get; set; }
        public decimal DAPPercentage { get; set; }
        public decimal SUPPLYBEFOREDISPATCHPercentage { get; set; }
        public decimal SUPPLYAFTERDISPATCHPercentage { get; set; }
        public decimal INSTALLATIONPercentage { get; set; }
        public decimal COMMISSIONINGPercentage { get; set; }
        public string DeliverySupplyperiod { get; set; }
        public string EAndCPeriod { get; set; }

        // Rate fields (user-entered percentages)
        public decimal EnCRate { get; set; }
        public decimal PnFRate { get; set; }
        public decimal FreightRate { get; set; }
        public decimal DesignChargesRate { get; set; }
        public decimal ContingencyRate { get; set; }
        public decimal InsuranceRate { get; set; }
        public decimal CommercialFactorRate { get; set; }
        public decimal IncentiveFactorRate { get; set; }
        public decimal TPCRate { get; set; }
        public decimal PORate { get; set; }   // must be >= 100
        public decimal NegotiationRate { get; set; }   // default 10


    }
}
