using System.Globalization;
using Koolstoof_App_1.Data;
using Koolstoof_App_1.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace Koolstoof_App_1.Controllers
{
    // Admin screens for the choices a customer makes on an item (sauce, side, and so on).
    [Authorize(Roles = "Admin")]
    public class OptionsController : Controller
    {
        private const int MaxNameLength = 60;
        private const decimal MaxExtraPrice = 1000m;

        private readonly ApplicationDbContext _context;

        public OptionsController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult Manage(int id)
        {
            var item = LoadItem(id);
            if (item == null)
            {
                return NotFound();
            }

            // Other items that already have choices can be copied, so a "Sauce" list is typed once.
            var sources = _context.MenuItems
                .Where(m => m.Id != id && m.OptionGroups.Any())
                .OrderBy(m => m.Name)
                .Select(m => new SelectListItem(m.Name, m.Id.ToString()))
                .ToList();
            ViewBag.CopySources = sources;

            return View(item);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult AddGroup(int menuItemId, string? name, bool isRequired = false)
        {
            var item = LoadItem(menuItemId);
            if (item == null)
            {
                return NotFound();
            }

            name = Clean(name);
            if (name == null)
            {
                return Fail(menuItemId, $"Give the group a name, for example \"Sauce\" (up to {MaxNameLength} characters).");
            }
            if (item.OptionGroups.Count >= OptionGroup.MaxGroupsPerItem)
            {
                return Fail(menuItemId, $"An item can have at most {OptionGroup.MaxGroupsPerItem} groups of choices.");
            }
            if (item.OptionGroups.Any(g => string.Equals(g.Name, name, StringComparison.OrdinalIgnoreCase)))
            {
                return Fail(menuItemId, $"This item already has a group called \"{name}\".");
            }

            _context.OptionGroups.Add(new OptionGroup
            {
                MenuItemId = menuItemId,
                Name = name,
                IsRequired = isRequired,
                DisplayOrder = item.OptionGroups.Any() ? item.OptionGroups.Max(g => g.DisplayOrder) + 1 : 1
            });
            _context.SaveChanges();

            return Done(menuItemId, $"Added \"{name}\". Now add the choices customers can pick from.");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult UpdateGroup(int id, string? name, bool isRequired = false)
        {
            var group = _context.OptionGroups.Include(g => g.MenuItem).ThenInclude(m => m.OptionGroups).FirstOrDefault(g => g.Id == id);
            if (group == null)
            {
                return NotFound();
            }

            name = Clean(name);
            if (name == null)
            {
                return Fail(group.MenuItemId, $"Give the group a name (up to {MaxNameLength} characters).");
            }
            if (group.MenuItem.OptionGroups.Any(g => g.Id != id && string.Equals(g.Name, name, StringComparison.OrdinalIgnoreCase)))
            {
                return Fail(group.MenuItemId, $"This item already has a group called \"{name}\".");
            }

            group.Name = name;
            group.IsRequired = isRequired;
            _context.SaveChanges();

            return Done(group.MenuItemId, $"Saved \"{name}\".");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteGroup(int id)
        {
            var group = _context.OptionGroups.Find(id);
            if (group == null)
            {
                return NotFound();
            }

            var menuItemId = group.MenuItemId;
            _context.OptionGroups.Remove(group); // its choices are removed with it
            _context.SaveChanges();

            return Done(menuItemId, $"Deleted \"{group.Name}\" and its choices. Orders already placed keep what the customer picked.");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult AddChoice(int groupId, string? name, string? extraPrice)
        {
            var group = _context.OptionGroups.Include(g => g.Choices).FirstOrDefault(g => g.Id == groupId);
            if (group == null)
            {
                return NotFound();
            }

            name = Clean(name);
            if (name == null)
            {
                return Fail(group.MenuItemId, $"Give the choice a name, for example \"Mushroom sauce\" (up to {MaxNameLength} characters).");
            }
            if (!TryParsePrice(extraPrice, out var price))
            {
                return Fail(group.MenuItemId, $"The extra price must be a number between 0 and {MaxExtraPrice:0}. Leave it empty if there is no extra charge.");
            }
            if (group.Choices.Count >= OptionChoice.MaxChoicesPerGroup)
            {
                return Fail(group.MenuItemId, $"A group can have at most {OptionChoice.MaxChoicesPerGroup} choices.");
            }
            if (group.Choices.Any(c => string.Equals(c.Name, name, StringComparison.OrdinalIgnoreCase)))
            {
                return Fail(group.MenuItemId, $"\"{name}\" is already in {group.Name}.");
            }

            _context.OptionChoices.Add(new OptionChoice
            {
                OptionGroupId = groupId,
                Name = name,
                ExtraPrice = price,
                DisplayOrder = group.Choices.Any() ? group.Choices.Max(c => c.DisplayOrder) + 1 : 1
            });
            _context.SaveChanges();

            return Done(group.MenuItemId, $"Added \"{name}\" to {group.Name}.");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult UpdateChoice(int id, string? name, string? extraPrice)
        {
            var choice = _context.OptionChoices.Include(c => c.OptionGroup).ThenInclude(g => g.Choices).FirstOrDefault(c => c.Id == id);
            if (choice == null)
            {
                return NotFound();
            }

            var menuItemId = choice.OptionGroup.MenuItemId;

            name = Clean(name);
            if (name == null)
            {
                return Fail(menuItemId, $"Give the choice a name (up to {MaxNameLength} characters).");
            }
            if (!TryParsePrice(extraPrice, out var price))
            {
                return Fail(menuItemId, $"The extra price must be a number between 0 and {MaxExtraPrice:0}.");
            }
            if (choice.OptionGroup.Choices.Any(c => c.Id != id && string.Equals(c.Name, name, StringComparison.OrdinalIgnoreCase)))
            {
                return Fail(menuItemId, $"\"{name}\" is already in {choice.OptionGroup.Name}.");
            }

            choice.Name = name;
            choice.ExtraPrice = price;
            _context.SaveChanges();

            return Done(menuItemId, $"Saved \"{name}\".");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteChoice(int id)
        {
            var choice = _context.OptionChoices.Include(c => c.OptionGroup).FirstOrDefault(c => c.Id == id);
            if (choice == null)
            {
                return NotFound();
            }

            var menuItemId = choice.OptionGroup.MenuItemId;
            _context.OptionChoices.Remove(choice);
            _context.SaveChanges();

            return Done(menuItemId, $"Removed \"{choice.Name}\". A cart that already holds it will ask the customer to choose again.");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult CopyFrom(int menuItemId, int sourceItemId)
        {
            var target = LoadItem(menuItemId);
            var source = LoadItem(sourceItemId);
            if (target == null || source == null || target.Id == source.Id)
            {
                return NotFound();
            }

            var copied = 0;
            var order = target.OptionGroups.Any() ? target.OptionGroups.Max(g => g.DisplayOrder) : 0;
            foreach (var group in source.OptionGroups.OrderBy(g => g.DisplayOrder))
            {
                if (target.OptionGroups.Count + copied >= OptionGroup.MaxGroupsPerItem ||
                    target.OptionGroups.Any(g => string.Equals(g.Name, group.Name, StringComparison.OrdinalIgnoreCase)))
                {
                    continue;
                }

                _context.OptionGroups.Add(new OptionGroup
                {
                    MenuItemId = target.Id,
                    Name = group.Name,
                    IsRequired = group.IsRequired,
                    DisplayOrder = ++order,
                    Choices = group.Choices.OrderBy(c => c.DisplayOrder)
                        .Select(c => new OptionChoice { Name = c.Name, ExtraPrice = c.ExtraPrice, DisplayOrder = c.DisplayOrder })
                        .ToList()
                });
                copied++;
            }
            _context.SaveChanges();

            return copied == 0
                ? Fail(menuItemId, $"Nothing was copied: {target.Name} already has groups with the same names as {source.Name}.")
                : Done(menuItemId, $"Copied {copied} group{(copied == 1 ? "" : "s")} from {source.Name}.");
        }

        private MenuItem? LoadItem(int id) => _context.MenuItems
            .Include(m => m.OptionGroups).ThenInclude(g => g.Choices)
            .FirstOrDefault(m => m.Id == id);

        private IActionResult Done(int menuItemId, string message)
        {
            TempData["OptionsMessage"] = message;
            return RedirectToAction("Manage", new { id = menuItemId });
        }

        private IActionResult Fail(int menuItemId, string message)
        {
            TempData["OptionsError"] = message;
            return RedirectToAction("Manage", new { id = menuItemId });
        }

        private static string? Clean(string? value)
        {
            var cleaned = string.Join(' ', (value ?? "").Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries));
            return cleaned.Length == 0 || cleaned.Length > MaxNameLength ? null : cleaned;
        }

        // Accepts "10", "10.50" and "10,50" (a comma is common when typing on a South African phone).
        private static bool TryParsePrice(string? value, out decimal price)
        {
            price = 0;
            if (string.IsNullOrWhiteSpace(value))
            {
                return true;
            }

            var normalised = value.Trim().TrimStart('R', 'r', '+').Replace(',', '.').Replace(" ", "");
            return decimal.TryParse(normalised, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out price)
                && price >= 0 && price <= MaxExtraPrice;
        }
    }
}
