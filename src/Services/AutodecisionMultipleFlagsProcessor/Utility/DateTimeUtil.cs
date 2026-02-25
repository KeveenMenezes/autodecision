using System.Globalization;

namespace AutodecisionMultipleFlagsProcessor.Utility
{
    public static class DateTimeUtil
    {
        private static readonly TimeZoneInfo MiamiTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time");

        public static DateTime Now
        {
            get
            {
                var utcNow = DateTime.UtcNow;
                return TimeZoneInfo.ConvertTimeFromUtc(utcNow, MiamiTimeZone);
            }
        }

        public static string ToString(string? format)
        {
            var miamiNow = Now;
            return format == null ? miamiNow.ToString(CultureInfo.CurrentCulture) : miamiNow.ToString(format);
        }

        public static DateTime Today => Now.Date;

        public static DateTime AddDays(double value) => Now.AddDays(value);

        public static DateTime AddHours(double value) => Now.AddHours(value);

        public static int GetDifferenceInDaysFromToday(this DateTime? dateTime) => dateTime.HasValue ? (Now - dateTime.Value).Days : 0;
        
        public static int GetDifferenceInDaysFromToday(this DateTime dateTime) => (Now - dateTime).Days;

        public static int GetDifferenceInMinutesFromNow(this DateTime dateTime) => (int)(Now - dateTime).TotalMinutes;

        public static bool IsBiggerOrEqual(DateTime firstDate, DateTime secondDate) => firstDate.Date >= secondDate.Date;
    }
}