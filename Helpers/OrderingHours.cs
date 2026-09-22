namespace Koolstoof_App_1.Helpers
{
    public static class OrderingHours
    {
        // TODO: move to an admin-editable RestaurantSettings table once that exists,
        // instead of this hardcoded schedule.
        public static bool IsOpenNow()
        {
            var now = DateTime.Now;
            var (open, close) = now.DayOfWeek == DayOfWeek.Sunday
                ? (new TimeSpan(8, 0, 0), new TimeSpan(21, 0, 0))
                : (new TimeSpan(7, 0, 0), new TimeSpan(22, 0, 0));

            return now.TimeOfDay >= open && now.TimeOfDay <= close;
        }

        public const string HoursDescription = "Mon–Sat 7:00–22:00, Sun 8:00–21:00";
    }
}
