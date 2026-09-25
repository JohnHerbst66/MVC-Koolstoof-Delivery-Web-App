using Koolstoof_App_1.Data;
using Koolstoof_App_1.Models;
using Koolstoof_App_1.Services;
using Microsoft.AspNetCore.Mvc;

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

            var menuItem = _context.MenuItems.Find(menuItemId);
            if (menuItem == null || !menuItem.IsInStock || !menuItem.IsDeliverable)
            {
                return NotFound();
            }

            var cart = _cartStore.Get();

            var existingLine = cart.FirstOrDefault(c => c.MenuItemId == menuItemId);
            if (existingLine != null)
            {
                existingLine.Quantity = Math.Min(MaxQuantityPerLine, existingLine.Quantity + quantity);
            }
            else
            {
                cart.Add(new CartItem
                {
                    MenuItemId = menuItem.Id,
                    Name = menuItem.Name,
                    Price = menuItem.IsSpecial && !menuItem.IsSitDownSpecial && menuItem.SpecialPrice.HasValue ? menuItem.SpecialPrice.Value : menuItem.Price,
                    Quantity = Math.Min(MaxQuantityPerLine, quantity)
                });
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
        public IActionResult AdjustQuantity(int menuItemId, int delta)
        {
            var cart = _cartStore.Get();

            var line = cart.FirstOrDefault(c => c.MenuItemId == menuItemId);
            if (line != null)
            {
                line.Quantity = Math.Clamp(line.Quantity + delta, 1, MaxQuantityPerLine);
                _cartStore.Save(cart);
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Remove(int menuItemId)
        {
            var cart = _cartStore.Get();
            cart.RemoveAll(c => c.MenuItemId == menuItemId);
            _cartStore.Save(cart);

            return RedirectToAction("Index");
        }
    }
}
