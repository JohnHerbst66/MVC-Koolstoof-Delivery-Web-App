using Koolstoof_App_1.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Koolstoof_App_1.Controllers
{
    [Authorize(Roles = "Admin")]
    public class SettingsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public SettingsController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            var settings = _context.RestaurantSettings.First();
            ViewBag.DeliveryAreas = _context.DeliveryAreas.OrderBy(d => d.Name).ToList();
            return View(settings);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Index(TimeSpan weekdayOpen, TimeSpan weekdayClose, TimeSpan sundayOpen, TimeSpan sundayClose,
            string? announcementText, bool announcementActive, string whatsAppNumber)
        {
            if (weekdayClose <= weekdayOpen)
            {
                ModelState.AddModelError("", "Weekday closing time must be after opening time.");
            }
            if (sundayClose <= sundayOpen)
            {
                ModelState.AddModelError("", "Sunday closing time must be after opening time.");
            }
            if (string.IsNullOrWhiteSpace(whatsAppNumber))
            {
                ModelState.AddModelError("", "Enter a WhatsApp number.");
            }

            if (!ModelState.IsValid)
            {
                ViewBag.DeliveryAreas = _context.DeliveryAreas.OrderBy(d => d.Name).ToList();
                var current = _context.RestaurantSettings.First();
                current.WeekdayOpen = weekdayOpen;
                current.WeekdayClose = weekdayClose;
                current.SundayOpen = sundayOpen;
                current.SundayClose = sundayClose;
                current.AnnouncementText = announcementText;
                current.AnnouncementActive = announcementActive;
                current.WhatsAppNumber = whatsAppNumber;
                return View(current);
            }

            var settings = _context.RestaurantSettings.First();
            settings.WeekdayOpen = weekdayOpen;
            settings.WeekdayClose = weekdayClose;
            settings.SundayOpen = sundayOpen;
            settings.SundayClose = sundayClose;
            settings.AnnouncementText = announcementText;
            settings.AnnouncementActive = announcementActive;
            settings.WhatsAppNumber = whatsAppNumber.Trim();
            _context.SaveChanges();

            TempData["SettingsSaved"] = "Settings saved.";
            return RedirectToAction("Index");
        }
    }
}
