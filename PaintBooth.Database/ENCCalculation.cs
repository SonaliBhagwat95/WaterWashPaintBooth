using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PaintBooth.Database
{
    public class ENCCalculation
    {
        public int Id { get; set; }
        public int EnquiryId { get; set; }       // ← add
        public string SalesNO { get; set; }
        public string RowsJson { get; set; }
        public decimal ContractorMargin { get; set; }
        public decimal SubTotal { get; set; }
        public decimal FinalValue { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime UpdatedAt { get; set; } = DateTime.Now;
    }
}
