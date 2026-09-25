namespace Koolstoof_App_1.Helpers
{
    // The restaurant runs on South African time, but hosts (Vercel, Azure) run in UTC,
    // so DateTime.Now would put ordering hours and specials two hours off. South Africa
    // has no daylight saving, so a fixed +2 offset is exact — and avoids depending on
    // time-zone data being installed inside a slim container.
    public static class SouthAfricaTime
    {
        private static readonly TimeSpan Offset = TimeSpan.FromHours(2);

        // Kind is Unspecified on purpose: these values are stored in "timestamp without
        // time zone" columns and compared with datetime-local form inputs.
        public static DateTime Now => DateTime.SpecifyKind(DateTime.UtcNow.Add(Offset), DateTimeKind.Unspecified);

        public static DateTime Today => Now.Date;
    }
}
