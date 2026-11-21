using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PaintBooth.Models
{
    public class DuctAccessoryWeightDto
    {
        public string Name { get; set; } = "";          // exact RM row description
        public int Qty { get; set; }
        public decimal UnitWeight { get; set; }          // kg per piece / set
        public decimal TotalWeight { get; set; }         // kg
    }
}
