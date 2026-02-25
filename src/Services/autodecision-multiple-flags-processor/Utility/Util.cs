using Newtonsoft.Json.Linq;
using System.Text;

namespace AutodecisionMultipleFlagsProcessor.Utility
{
    public class Util
    {
        public static bool isFieldsNotEqual(string f1, string f2) =>
            !string.IsNullOrEmpty(f1) && !string.IsNullOrEmpty(f2) &&
            !f1.Equals(f2, StringComparison.CurrentCultureIgnoreCase);

        public static bool isFieldsNotEqual(DateTime? f1, DateTime? f2) =>
           f1.HasValue && f2.HasValue && !IsSameDay(f1.Value, f2.Value);

        public static bool IsSameDay(DateTime date1, DateTime date2) =>
           date1.Year == date2.Year && date1.DayOfYear == date2.DayOfYear;

        public static T GetValueFromJSON<T>(JToken value)
        {
            if (value == null)
                return default;

            var val = value.ToString();
            if (string.IsNullOrEmpty(val))
                return default;

            return value.ToObject<T>();
        }

        public static string RemoveSpecialCharacters(string str)
        {
            StringBuilder sb = new StringBuilder();
            foreach (char c in str)
            {
                if ((c >= '0' && c <= '9') || (c >= 'A' && c <= 'Z') || (c >= 'a' && c <= 'z') || c == '.' ||
                    c == '_' ||
                    c == ' ')
                {
                    sb.Append(c);
                }
            }

            return sb.ToString();
        }

        public static bool IsDateOfBirthInValid(DateTime date_of_birth)
        {
            int year = DateTime.Now.Date.Year;

            int yearDob = date_of_birth.Year;
            if (yearDob > year)
            {
                return true;
            }

            if (date_of_birth.AddYears(18) > DateTime.Now)
            {
                return true;
            }

            return false;
        }

        public static string? GetLastFour(string source) => source?.Length >= 4 ? source.Substring(source.Length - 4) : source;

        public static string StandardizeString(string? name) => !string.IsNullOrEmpty(name) ? name.Trim().ToLower() : string.Empty;
    }
}