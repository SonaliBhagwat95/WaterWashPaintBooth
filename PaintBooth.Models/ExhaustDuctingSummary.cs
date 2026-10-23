using PaintBooth.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PaintBooth.Models
{
    public class ExhaustDuctingSummary
    {
        public int VerticalDuctCount { get; set; }
        public int HorizontalDuctCount { get; set; }
        public int TotalDuctCount { get; set; }
        public int TotalBendCount { get; set; }
        public decimal Thickness { get; set; }
        public decimal TotalLengthM { get; set; }
        public bool HasData => TotalDuctCount > 0 || TotalBendCount > 0;

        public static ExhaustDuctingSummary Build(List<ExhaustDuctings> rows)
        {
            var s = new ExhaustDuctingSummary();
            if (rows == null) return s;

            bool horizontal = false;      // fallback for old rows with no orientation saved
            decimal totalLenMm = 0;

            foreach (var r in rows.OrderBy(x => x.DuctId))
            {
                var dir = (r.BendDirection ?? "").Trim().ToLower();

                if (string.Equals(r.BendType, "Bend", StringComparison.OrdinalIgnoreCase))
                {
                    s.TotalBendCount++;
                    switch (dir)
                    {
                        case "left":
                        case "right": horizontal = true; break;
                        case "up":
                        case "down": horizontal = false; break;
                    }
                }
                else
                {
                    s.TotalDuctCount++;

                    // Duct rows store Vertical / Horizontal in BendDirection
                    bool isHorizontal = dir == "horizontal" ? true
                                      : dir == "vertical" ? false
                                      : horizontal;          // old records → infer from previous bend
                    horizontal = isHorizontal;               // keep the run direction in sync

                    if (isHorizontal) s.HorizontalDuctCount++; else s.VerticalDuctCount++;
                    if (s.Thickness == 0) s.Thickness = r.DuctThickness;
                    totalLenMm += r.DuctLength ?? 0;
                }
            }

            s.TotalLengthM = Math.Round(totalLenMm / 1000m, 2);
            return s;
        }
    }
}
