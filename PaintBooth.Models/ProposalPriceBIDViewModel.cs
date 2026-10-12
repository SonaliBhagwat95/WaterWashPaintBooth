using PaintBooth.Database;

namespace PaintBooth.Models
{
    public class ProposalPriceBIDViewModel
    {
        public ProposalModel ProposalDetails { get; set; }
        public PriceBIDModel PriceBIDDetails { get; set; }
        public ProposalModel MaterialList { get; set; }
        public PriceBIDModel PaymentTerms { get; set; }
        public BlowerDetails BlowerDetails { get; set; }
        public decimal exhaustPrice { get; set; }
        public EnquirySalesEnDetails salesEngineer { get; set; }
        public List<ExhaustDuctings> ExhaustDuctings { get; set; }
        public ExhaustDuctingSummary ExhaustDuctingSummary { get; set; }

        public string samplingPlatformAndLadder { get; set; }

    }
}
