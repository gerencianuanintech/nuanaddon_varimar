using System;
using System.Globalization;

namespace nuanaddon_varimar.Shared.Parsing {
    public static class SapValueParser {
        public static bool TryParseDate(string value, out DateTime date) {
            date = DateTime.MinValue;
            value = (value ?? string.Empty).Trim();

            if (string.IsNullOrWhiteSpace(value))
                return false;

            if (DateTime.TryParseExact(value, "yyyyMMdd", CultureInfo.InvariantCulture, DateTimeStyles.None, out date) ||
                DateTime.TryParseExact(value, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out date) ||
                DateTime.TryParse(value, CultureInfo.CurrentCulture, DateTimeStyles.None, out date) ||
                DateTime.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.None, out date)) {
                return date.Date != new DateTime(1899, 12, 30);
            }

            return false;
        }

        public static decimal ParseDecimal(string value) {
            value = (value ?? string.Empty).Trim();

            if (string.IsNullOrWhiteSpace(value) || value == "-")
                return 0m;

            decimal result;
            if (decimal.TryParse(value, NumberStyles.Number, CultureInfo.InvariantCulture, out result))
                return result;

            if (decimal.TryParse(value, NumberStyles.Number, CultureInfo.CurrentCulture, out result))
                return result;

            string normalized = value.Replace(",", string.Empty);
            if (decimal.TryParse(normalized, NumberStyles.Number, CultureInfo.InvariantCulture, out result))
                return result;

            return 0m;
        }

        public static int ParseInt(string value) {
            int result;
            return int.TryParse((value ?? string.Empty).Trim(), NumberStyles.Number, CultureInfo.InvariantCulture, out result)
                ? result
                : 0;
        }
    }
}
