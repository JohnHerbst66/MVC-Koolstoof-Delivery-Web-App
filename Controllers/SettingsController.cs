using Koolstoof_App_1.Data;
using Koolstoof_App_1.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Koolstoof_App_1.Controllers
{
    [Authorize(Roles = "Admin")]
    public class SettingsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public SettingsController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var settings = _context.RestaurantSettings.First();
            ViewBag.DeliveryAreas = _context.DeliveryAreas.OrderBy(d => d.Name).ToList();
            ViewBag.AdminAccounts = (await _userManager.GetUsersInRoleAsync("Admin"))
                .OrderBy(u => u.UserName)
                .ToList();
            return View(settings);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(TimeSpan weekdayOpen, TimeSpan weekdayClose, TimeSpan sundayOpen, TimeSpan sundayClose,
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
                ViewBag.AdminAccounts = (await _userManager.GetUsersInRoleAsync("Admin"))
                    .OrderBy(u => u.UserName)
                    .ToList();
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

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null || !await _userManager.IsInRoleAsync(user, "Admin"))
            {
                return NotFound();
            }

            var newPassword = PasswordGenerator.GenerateStrong();

            await _userManager.RemovePasswordAsync(user);
            var result = await _userManager.AddPasswordAsync(user, newPassword);
            if (!result.Succeeded)
            {
                TempData["ResetError"] = string.Join(" ", result.Errors.Select(e => e.Description));
                return RedirectToAction("Index");
            }

            // Invalidate any existing sessions on this account (e.g. a lost device)
            // now that its password has changed.
            await _userManager.UpdateSecurityStampAsync(user);

            TempData["ResetAdminId"] = user.UserName;
            TempData["ResetPassword"] = newPassword;
            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateNotifyNumber(string userId, string? notifyWhatsAppNumber)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null || !await _userManager.IsInRoleAsync(user, "Admin"))
            {
                return NotFound();
            }

            user.PhoneNumber = string.IsNullOrWhiteSpace(notifyWhatsAppNumber) ? null : notifyWhatsAppNumber.Trim();
            await _userManager.UpdateAsync(user);

            TempData["SettingsSaved"] = $"WhatsApp number for {user.UserName} saved.";
            return RedirectToAction("Index");
        }
    }
}
