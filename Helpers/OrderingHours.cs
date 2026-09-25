using Koolstoof_App_1.Models;

namespace Koolstoof_App_1.Helpers
{
    public static class OrderingHours
    {
        public static bool IsOpenNow(RestaurantSettings settings)
        {
            var now = SouthAfricaTime.Now;
            var (open, close) = now.DayOfWeek == DayOfWeek.Sunday
                ? (settings.SundayOpen, settings.SundayClose)
                : (settings.WeekdayOpen, settings.WeekdayClose);

            return now.TimeOfDay >= open && now.TimeOfDay <= close;
        }

        public static string HoursDescription(RestaurantSettings settings) =>
            $"Mon–Sat {Format(settings.WeekdayOpen)}–{Format(settings.WeekdayClose)}, Sun {Format(settings.SundayOpen)}–{Format(settings.SundayClose)}";

        public static string TodayHoursDescription(RestaurantSettings settings)
        {
            var (open, close) = SouthAfricaTime.Now.DayOfWeek == DayOfWeek.Sunday
                ? (settings.SundayOpen, settings.SundayClose)
                : (settings.WeekdayOpen, settings.WeekdayClose);

            return $"{Format(open)} – {Format(close)}";
        }

        private static string Format(TimeSpan t) => SouthAfricaTime.Today.Add(t).ToString("H:mm");
    }
}
