namespace Koolstoof_App_1.Models
{
    // Singleton row (always Id = 1) holding the handful of site-wide settings
    // an admin should be able to change without a code deploy.
    public class RestaurantSettings
    {
        public int Id { get; set; }

        public TimeSpan WeekdayOpen { get; set; } = new TimeSpan(7, 0, 0);
        public TimeSpan WeekdayClose { get; set; } = new TimeSpan(22, 0, 0);
        public TimeSpan SundayOpen { get; set; } = new TimeSpan(8, 0, 0);
        public TimeSpan SundayClose { get; set; } = new TimeSpan(21, 0, 0);

        // Shown as a dismissible banner on the Home page when Active, e.g. for
        // loadshedding notices or holiday hours.
        public string? AnnouncementText { get; set; }
        public bool AnnouncementActive { get; set; } = false;

        public string WhatsAppNumber { get; set; } = "27604944665";
    }
}
