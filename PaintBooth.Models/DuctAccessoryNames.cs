using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PaintBooth.Models
{
    /// AccessoryType keys saved in ductaccessorydetails + the exact RM costing row descriptions.
    public static class DuctAccessoryNames
    {
        public const string Port = "Sampling_Port";
        public const string Platform = "Sampling_Platform";
        public const string Railing = "Safety_Railing";
        public const string Ladder = "Ladder";
        public const string SupVertical = "Support_Vertical";
        public const string SupHorizontal = "Support_Horizontal";
        public const string SupBracing = "Support_Bracing";

        public static readonly Dictionary<string, string> RmRow = new()
        {
            { Port,          "Sampling Port" },
            { Platform,      "Sampling Inspection Platform" },
            { Railing,       "Safety railing" },
            { Ladder,        "Ladder" },
            { SupVertical,   "Vertical Straight Structure" },
            { SupHorizontal, "Horizontal Structure" },
            { SupBracing,    "Cross bracing" },
        };
    }
}
