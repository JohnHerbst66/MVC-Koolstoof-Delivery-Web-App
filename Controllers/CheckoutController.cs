using Koolstoof_App_1.Data;
using Koolstoof_App_1.Extensions;
using Koolstoof_App_1.Helpers;
using Koolstoof_App_1.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace Koolstoof_App_1.Controllers
{
    public class CheckoutController : Controller
    {
        private const string CartSessionKey = "Cart";

        private readonly ApplicationDbContext _context;

        public CheckoutController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult Index()
        {
            var cart = HttpContext.Session.GetObjectFromJson<List<CartItem>>(CartSessionKey) ?? new List<CartItem>();

            ViewBag.Cart = cart;
            ViewBag.Subtotal = cart.Sum(c => c.LineTotal);
            ViewBag.DeliveryAreas = new SelectList(_context.DeliveryAreas, "Id", "Name");
            ViewBag.IsOpen = OrderingHours.IsOpenNow();
            ViewBag.HoursDescription = OrderingHours.HoursDescription;

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult PlaceOrder(string customerName, string customerPhone, string deliveryAddress, int deliveryAreaId)
        {
            var cart = HttpContext.Session.GetObjectFromJson<List<CartItem>>(CartSessionKey) ?? new List<CartItem>();

            if (!cart.Any())
            {
                ModelState.AddModelError("", "Your cart is empty.");
            }

            if (!OrderingHours.IsOpenNow())
            {
                ModelState.AddModelError("", $"We're closed right now. Ordering hours: {OrderingHours.HoursDescription}.");
            }

            var deliveryArea = _context.DeliveryAreas.Find(deliveryAreaId);
            if (deliveryArea == null)
            {
                ModelState.AddModelError("", "Please select a valid delivery area.");
            }

            if (string.IsNullOrWhiteSpace(customerName) || string.IsNullOrWhiteSpace(customerPhone) || string.IsNullOrWhiteSpace(deliveryAddress))
            {
                ModelState.AddModelError("", "Please fill in your name, phone number, and delivery address.");
            }

            // Re-validate every line against the live menu, never trust the stored cart alone.
            var invalidItems = new List<string>();
            if (cart.Any())
            {
                foreach (var line in cart)
                {
                    var menuItem = _context.MenuItems.Find(line.MenuItemId);
                    if (menuItem == null || !menuItem.IsInStock || !menuItem.IsDeliverable)
                    {
                        invalidItems.Add(line.Name);
                    }
                }
            }
            if (invalidItems.Any())
            {
                ModelState.AddModelError("", $"These items are no longer available for delivery and were not removed automatically, please update your cart: {string.Join(", ", invalidItems)}.");
            }

            if (!ModelState.IsValid)
            {
                ViewBag.Cart = cart;
                ViewBag.Subtotal = cart.Sum(c => c.LineTotal);
                ViewBag.DeliveryAreas = new SelectList(_context.DeliveryAreas, "Id", "Name", deliveryAreaId);
                ViewBag.IsOpen = OrderingHours.IsOpenNow();
                ViewBag.HoursDescription = OrderingHours.HoursDescription;
                return View("Index");
            }

            var subtotal = cart.Sum(c => c.LineTotal);

            var order = new Order
            {
                CustomerName = customerName,
                CustomerPhone = customerPhone,
                DeliveryAddress = deliveryAddress,
                DeliveryAreaId = deliveryArea!.Id,
                DeliveryFee = deliveryArea.DeliveryFee,
                Subtotal = subtotal,
                Total = subtotal + deliveryArea.DeliveryFee,
                PaymentMethod = PaymentMethod.CashOnDelivery,
                IsPaid = false,
                Status = OrderStatus.Incoming
            };

            foreach (var line in cart)
            {
                order.OrderItems.Add(new OrderItem
                {
                    MenuItemId = line.MenuItemId,
                    MenuItemName = line.Name,
                    UnitPrice = line.Price,
                    Quantity = line.Quantity
                });
            }

            _context.Orders.Add(order);
            _context.SaveChanges();

            HttpContext.Session.Remove(CartSessionKey);

            return RedirectToAction("Confirmation", new { id = order.Id });
        }

        [HttpGet]
        public IActionResult Confirmation(int id)
        {
            var order = _context.Orders
                .Include(o => o.OrderItems)
                .Include(o => o.DeliveryArea)
                .FirstOrDefault(o => o.Id == id);

            if (order == null)
            {
                return NotFound();
            }

            return View(order);
        }
    }
}
