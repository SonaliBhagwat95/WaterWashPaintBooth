using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace PaintBooth.Models
{
    public class WaterColumnRowDto
    {
        [JsonPropertyName("itemNumber")]
        public int ItemNumber { get; set; }
        [JsonPropertyName("quantity")]
        public decimal Quantity { get; set; }
        [JsonPropertyName("isSelected")]
        public bool IsSelected { get; set; }
    }
}
