using Koolstoof_App_1.Data;
using Koolstoof_App_1.Helpers;
using Koolstoof_App_1.Models;
using Koolstoof_App_1.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace Koolstoof_App_1.Controllers
{
    [Authorize(Roles = "Admin")]
    public class SpecialsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public SpecialsController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Manage()
        {
            SpecialsService.SyncActiveSpecials(_context);

            var specials = _context.Specials
                .Include(s => s.MenuItem)
                .OrderBy(s => s.IsEnded)
                .ThenByDescending(s => s.CreatedAt)
                .ToList();

            return View(specials);
        }

        [HttpGet]
        public IActionResult Create()
        {
            ViewBag.MenuItems = new SelectList(_context.MenuItems.OrderBy(m => m.Name), "Id", "Name");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(int menuItemId, decimal specialPrice, SpecialEndCondition endCondition,
            bool activeMonday, bool activeTuesday, bool activeWednesday, bool activeThursday,
            bool activeFriday, bool activeSaturday, bool activeSunday,
            DateTime? startAt, DateTime? endAt, bool sitDownOnly, string? imageUrl, IFormFile? imageFile)
        {
            var menuItem = _context.MenuItems.Find(menuItemId);
            if (menuItem == null)
            {
                ModelState.AddModelError("", "Please choose a valid menu item.");
            }
            else if (specialPrice <= 0 || specialPrice >= menuItem.Price)
            {
                ModelState.AddModelError("", $"The special price must be less than the item's normal price (R{menuItem.Price}).");
            }
            else if (_context.Specials.Any(s => s.MenuItemId == menuItemId && !s.IsEnded))
            {
                ModelState.AddModelError("", "This item already has an active special. Stop or delete it first before creating a new one.");
            }

            if (endCondition == SpecialEndCondition.SpecificDays &&
                !(activeMonday || activeTuesday || activeWednesday || activeThursday || activeFriday || activeSaturday || activeSunday))
            {
                ModelState.AddModelError("", "Pick at least one day of the week.");
            }

            if (endCondition == SpecialEndCondition.DateRange && (!startAt.HasValue || !endAt.HasValue || endAt <= startAt))
            {
                ModelState.AddModelError("", "Enter a valid start and end date/time, with the end after the start.");
            }

            var (uploadedUrl, uploadError) = await ImageUploadHelper.SaveAsync(imageFile, _context);
            if (uploadError != null)
            {
                ModelState.AddModelError("", uploadError);
            }

            if (!ModelState.IsValid)
            {
                ViewBag.MenuItems = new SelectList(_context.MenuItems.OrderBy(m => m.Name), "Id", "Name", menuItemId);
                return View();
            }

            var special = new Special
            {
                MenuItemId = menuItemId,
                SpecialPrice = specialPrice,
                EndCondition = endCondition,
                ActiveMonday = activeMonday,
                ActiveTuesday = activeTuesday,
                ActiveWednesday = activeWednesday,
                ActiveThursday = activeThursday,
                ActiveFriday = activeFriday,
                ActiveSaturday = activeSaturday,
                ActiveSunday = activeSunday,
                StartAt = endCondition == SpecialEndCondition.DateRange ? startAt : null,
                EndAt = endCondition == SpecialEndCondition.DateRange ? endAt : null,
                SitDownOnly = sitDownOnly,
                ImageUrl = uploadedUrl ?? imageUrl
            };

            _context.Specials.Add(special);
            _context.SaveChanges();

            SpecialsService.SyncActiveSpecials(_context);

            return RedirectToAction("Manage");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Stop(int id)
        {
            var special = _context.Specials.Include(s => s.MenuItem).FirstOrDefault(s => s.Id == id);
            if (special == null)
            {
                return NotFound();
            }

            special.IsEnded = true;
            special.MenuItem.IsSpecial = false;
            special.MenuItem.SpecialPrice = null;
            special.MenuItem.IsSitDownSpecial = false;
            special.MenuItem.SpecialImageUrl = null;
            _context.SaveChanges();

            return RedirectToAction("Manage");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Delete(int id)
        {
            var special = _context.Specials.Include(s => s.MenuItem).FirstOrDefault(s => s.Id == id);
            if (special == null)
            {
                return NotFound();
            }

            special.MenuItem.IsSpecial = false;
            special.MenuItem.SpecialPrice = null;
            special.MenuItem.IsSitDownSpecial = false;
            special.MenuItem.SpecialImageUrl = null;
            _context.Specials.Remove(special);
            _context.SaveChanges();

            return RedirectToAction("Manage");
        }
    }
}
