using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PaintBooth.Database
{
    public class PaintBoothDetails
    {
        [Key]
        public int PaintBoothID { get; set; }
        [ForeignKey("EnquiryId")]
        public int EnquiryId { get; set; }
        [Column(TypeName = "double(18,2)")]
        public double D1 { get; set; }
        [Column(TypeName = "double(18,2)")]
        public double D2 { get; set; }

        //[Column(TypeName = "double(18,2)")]
        public double D3 { get; set; }
        [Column(TypeName = "double(18,2)")]
        public double W1 { get; set; }
        [Column(TypeName = "double(18,2)")]
        public double W2 { get; set; }
        [Column(TypeName = "double(18,2)")]
        public double W3 { get; set; }
        //[Column(TypeName = "double(18,2)")]
        public double D { get; set; }
        [Column(TypeName = "double(18,2)")]

        public double H1 { get; set; }
        [Column(TypeName = "double(18,2)")]
        public double H2 { get; set; }
        //[Column(TypeName = "double(18,2)")]
        public double W { get; set; }
        //[Column(TypeName = "double(18,2)")]
        public double H { get; set; }
        [Column(TypeName = "double(18,2)")]

        public double CrossSectionalAreaOfBlower { get; set; }
        public double VelocityofBlower { get; set; }
        public bool designStatus { get; set; }
        public double CapacityofBlowerincubicm { get; set; }
        public double CapacityofBlowerinHr { get; set; }
        public double BetweenCenterDistance { get; set; }
        public decimal RoundingCapacity { get; set; }
        public int NoOfBlowers { get; set; }
        public int CapacityPerBlower { get; set; }
        public int? CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public int? ModifiedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public bool IsActive { get; set; }
    }
}
