using Koolstoof_App_1.Data;
using Koolstoof_App_1.Extensions;
using Koolstoof_App_1.Models;
using Koolstoof_App_1.Services;
using Microsoft.AspNetCore.Mvc;

namespace Koolstoof_App_1.Controllers
{
    public class CartController : Controller
    {
        private const string CartSessionKey = "Cart";

        private readonly ApplicationDbContext _context;

        public CartController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            var cart = HttpContext.Session.GetObjectFromJson<List<CartItem>>(CartSessionKey) ?? new List<CartItem>();
            return View(cart);
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

            var cart = HttpContext.Session.GetObjectFromJson<List<CartItem>>(CartSessionKey) ?? new List<CartItem>();

            var existingLine = cart.FirstOrDefault(c => c.MenuItemId == menuItemId);
            if (existingLine != null)
            {
                existingLine.Quantity += quantity;
            }
            else
            {
                cart.Add(new CartItem
                {
                    MenuItemId = menuItem.Id,
                    Name = menuItem.Name,
                    Price = menuItem.IsSpecial && !menuItem.IsSitDownSpecial && menuItem.SpecialPrice.HasValue ? menuItem.SpecialPrice.Value : menuItem.Price,
                    Quantity = quantity
                });
            }

            HttpContext.Session.SetObjectAsJson(CartSessionKey, cart);

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
            var cart = HttpContext.Session.GetObjectFromJson<List<CartItem>>(CartSessionKey) ?? new List<CartItem>();

            var line = cart.FirstOrDefault(c => c.MenuItemId == menuItemId);
            if (line != null)
            {
                line.Quantity = Math.Max(1, line.Quantity + delta);
                HttpContext.Session.SetObjectAsJson(CartSessionKey, cart);
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Remove(int menuItemId)
        {
            var cart = HttpContext.Session.GetObjectFromJson<List<CartItem>>(CartSessionKey) ?? new List<CartItem>();
            cart.RemoveAll(c => c.MenuItemId == menuItemId);
            HttpContext.Session.SetObjectAsJson(CartSessionKey, cart);

            return RedirectToAction("Index");
        }
    }
}
