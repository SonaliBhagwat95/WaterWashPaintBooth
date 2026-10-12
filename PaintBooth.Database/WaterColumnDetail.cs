using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PaintBooth.Database
{
    public class WaterColumnDetail
    {
        [Key]
        public int Id { get; set; }
        public int EnquiryID { get; set; }
        public int ItemNumber { get; set; }
        public decimal Quantity { get; set; }
        public bool IsSelected { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime ModifiedDate { get; set; }
    }
}
