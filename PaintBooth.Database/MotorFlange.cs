using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PaintBooth.Database
{
    public class MotorFlange
    {
        [Key]
        public int MotorCatalogID { get; set; }
        public decimal? RatedOutPut { get; set; }
        public decimal? RatedOutputHP { get; set; }
        public string Framesize { get; set; }
        public string TypeReference { get; set; }
        public decimal? RatedSpeed { get; set; }
        public decimal? RatedCurrent { get; set; }
        public decimal? RatedTorque { get; set; }
        public decimal? PowerFactorFullLoad { get; set; }
        public decimal? PowerFactor34Load { get; set; }
        public decimal? PowerFactor12load { get; set; }
        public decimal? EfficiencyFullLoad { get; set; }
        public decimal? Efficiency34Load { get; set; }
        public decimal? Efficiency12Load { get; set; }
        public decimal? CurrentRatio { get; set; }
        public decimal? CurrentRatio1 { get; set; }
        public decimal? CurrentRatio2 { get; set; }
        public decimal? RotorGD2 { get; set; }
        public decimal? NetWeight { get; set; }
        public string Pole { get; set; }
        public decimal? P { get; set; }
        public decimal? N { get; set; }
        public decimal? M { get; set; }
        public decimal? I { get; set; }
        public decimal? S { get; set; }
        public decimal? T { get; set; }
        public decimal? LA { get; set; }
        public decimal? AD { get; set; }
        public string L { get; set; }
        public decimal? LV { get; set; }
        public decimal? AC { get; set; }
        public decimal? V { get; set; }
        public decimal? AG { get; set; }
        public string S2BSC { get; set; }
        public decimal? DDA { get; set; }
        public decimal? EEA { get; set; }
        public decimal? FFA { get; set; }
        public decimal? GAGC { get; set; }
        public decimal? ll1 { get; set; }
        public string D5 { get; set; }
    }
}
