using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PaintBooth.Database.Helper
{
    public static class SafeExtensions
    {
        // Safe string — returns "" if null or DBNull
        public static string SafeString(this object value)
            => value == null || value is DBNull ? "" : value.ToString() ?? "";

        // Safe decimal
        public static decimal SafeDecimal(this object value)
            => value == null || value is DBNull ? 0m : Convert.ToDecimal(value);

        // Safe DateTime
        public static DateTime SafeDateTime(this object value, DateTime? fallback = null)
            => value == null || value is DBNull
                ? (fallback ?? DateTime.UtcNow)
                : Convert.ToDateTime(value);

        // Safe int
        public static int SafeInt(this object value)
            => value == null || value is DBNull ? 0 : Convert.ToInt32(value);
    }
}
