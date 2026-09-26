using Koolstoof_App_1.Data;
using Koolstoof_App_1.Models;
using Koolstoof_App_1.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Koolstoof_App_1.Controllers
{
    public class CartController : Controller
    {
        private const int MaxQuantityPerLine = 99;

        private readonly ApplicationDbContext _context;
        private readonly CartStore _cartStore;

        public CartController(ApplicationDbContext context, CartStore cartStore)
        {
            _context = context;
            _cartStore = cartStore;
        }

        public IActionResult Index()
        {
            return View(_cartStore.Get());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Add(int menuItemId, int quantity = 1, string? returnUrl = null)
        {
            if (quantity < 1)
            {
                quantity = 1;
            }

            SpecialsService.SyncActiveSpecials(_context);

            var menuItem = _context.MenuItems
                .Include(m => m.OptionGroups).ThenInclude(g => g.Choices)
                .FirstOrDefault(m => m.Id == menuItemId);
            if (menuItem == null || !menuItem.IsInStock || !menuItem.IsDeliverable)
            {
                return NotFound();
            }

            // Options arrive as one form field per group, named choice_{groupId}.
            var selectedChoiceIds = Request.Form.Keys
                .Where(k => k.StartsWith("choice_"))
                .SelectMany(k => Request.Form[k].ToArray())
                .Select(v => int.TryParse(v, out var id) ? id : 0)
                .Where(id => id > 0)
                .ToList();

            var selection = OptionSelection.Resolve(menuItem, selectedChoiceIds);
            if (!selection.IsValid)
            {
                TempData["CartError"] = selection.Error;
                return RedirectToAction("Index", "Menu", null, $"item-{menuItem.Id}");
            }

            var newLine = new CartItem
            {
                MenuItemId = menuItem.Id,
                Name = menuItem.Name,
                Price = menuItem.DeliveryPrice + selection.ExtraPrice,
                Quantity = Math.Min(MaxQuantityPerLine, quantity),
                ChoiceIds = selection.ChoiceIds,
                Choices = selection.Text
            };

            var cart = _cartStore.Get();

            var existingLine = cart.FirstOrDefault(c => c.LineKey == newLine.LineKey);
            if (existingLine != null)
            {
                existingLine.Quantity = Math.Min(MaxQuantityPerLine, existingLine.Quantity + quantity);
            }
            else
            {
                cart.Add(newLine);
            }

            if (!_cartStore.Save(cart))
            {
                TempData["CartError"] = "Your cart is full — please check out or remove something before adding more.";
            }

            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }

            return RedirectToAction("Index", "Menu");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult AdjustQuantity(string lineKey, int delta)
        {
            var cart = _cartStore.Get();

            var line = cart.FirstOrDefault(c => c.LineKey == lineKey);
            if (line != null)
            {
                line.Quantity = Math.Clamp(line.Quantity + delta, 1, MaxQuantityPerLine);
                _cartStore.Save(cart);
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Remove(string lineKey)
        {
            var cart = _cartStore.Get();
            cart.RemoveAll(c => c.LineKey == lineKey);
            _cartStore.Save(cart);

            return RedirectToAction("Index");
        }
    }
}
