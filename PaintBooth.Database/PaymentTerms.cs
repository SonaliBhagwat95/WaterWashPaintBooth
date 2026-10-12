using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PaintBooth.Database
{
    public class PaymentTerms
    {
        [Key]
        public int PaymentTermsId { get; set; }
        public int EnquiryId { get; set; }
        public decimal AdvancedPercentage { get; set; }
        public decimal DAPPercentage { get; set; }
        public decimal SUPPLYBEFOREDISPATCHPercentage { get; set; }
        public decimal SUPPLYAFTERDISPATCHPercentage { get; set; }
        public decimal INSTALLATIONPercentage { get; set; }
        public decimal COMMISSIONINGPercentage { get; set; }
        public string DeliverySupplyperiod { get; set; }
        public string EAndCPeriod { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime ModifiedDate { get; set; }
        public int CreatedBy { get; set; }
        public int ModifiedBy { get; set; }
        public byte IsActive { get; set; }

    }
}
