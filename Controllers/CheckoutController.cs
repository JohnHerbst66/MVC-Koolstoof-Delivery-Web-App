using Koolstoof_App_1.Data;
using Koolstoof_App_1.Helpers;
using Koolstoof_App_1.Models;
using Koolstoof_App_1.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Options;
using Microsoft.EntityFrameworkCore;

namespace Koolstoof_App_1.Controllers
{
    public class CheckoutController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly PayFastSettings _payFastSettings;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly WhatsAppNotificationService _whatsAppNotificationService;
        private readonly CartStore _cartStore;

        public CheckoutController(ApplicationDbContext context, IOptions<PayFastSettings> payFastSettings,
            UserManager<IdentityUser> userManager, WhatsAppNotificationService whatsAppNotificationService, CartStore cartStore)
        {
            _context = context;
            _payFastSettings = payFastSettings.Value;
            _userManager = userManager;
            _whatsAppNotificationService = whatsAppNotificationService;
            _cartStore = cartStore;
        }

        [HttpGet]
        public IActionResult Index()
        {
            var cart = _cartStore.Get();
            var settings = _context.RestaurantSettings.First();

            ViewBag.Cart = cart;
            ViewBag.Subtotal = cart.Sum(c => c.LineTotal);
            ViewBag.DeliveryAreas = new SelectList(_context.DeliveryAreas, "Id", "Name");
            ViewBag.IsOpen = OrderingHours.IsOpenNow(settings);
            ViewBag.HoursDescription = OrderingHours.HoursDescription(settings);
            ViewBag.WhatsAppNumber = settings.WhatsAppNumber;

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PlaceOrder(string customerName, string customerPhone, string deliveryAddress, int deliveryAreaId, PaymentMethod paymentMethod)
        {
            var cart = _cartStore.Get();
            var settings = _context.RestaurantSettings.First();

            if (!cart.Any())
            {
                ModelState.AddModelError("", "Your cart is empty.");
            }

            if (!OrderingHours.IsOpenNow(settings))
            {
                ModelState.AddModelError("", $"We're closed right now. Ordering hours: {OrderingHours.HoursDescription(settings)}.");
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
                ViewBag.IsOpen = OrderingHours.IsOpenNow(settings);
                ViewBag.HoursDescription = OrderingHours.HoursDescription(settings);
                ViewBag.WhatsAppNumber = settings.WhatsAppNumber;
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
                PaymentMethod = paymentMethod,
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

            _cartStore.Clear();

            var adminNumbers = (await _userManager.GetUsersInRoleAsync("Admin"))
                .Select(u => u.PhoneNumber)
                .Where(n => !string.IsNullOrWhiteSpace(n))
                .Select(n => n!);
            await _whatsAppNotificationService.SendOrderAlertAsync(adminNumbers, order.Id.ToString(), order.CustomerName, order.Total.ToString("F2"), deliveryArea.Name);

            if (paymentMethod == PaymentMethod.PayFast)
            {
                return RedirectToAction("PayFastRedirect", new { id = order.Id });
            }

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

        // Renders a page that auto-submits an HTML form to PayFast's payment page —
        // PayFast expects a browser POST with these fields, not a redirect with a
        // query string, which is why this isn't just a RedirectResult.
        [HttpGet]
        public IActionResult PayFastRedirect(int id)
        {
            var order = _context.Orders.Include(o => o.OrderItems).FirstOrDefault(o => o.Id == id);
            if (order == null || order.PaymentMethod != PaymentMethod.PayFast)
            {
                return NotFound();
            }

            var fields = new List<KeyValuePair<string, string>>
            {
                new("merchant_id", _payFastSettings.MerchantId),
                new("merchant_key", _payFastSettings.MerchantKey),
                new("return_url", Url.Action("PayFastReturn", "Checkout", new { id = order.Id }, Request.Scheme)!),
                new("cancel_url", Url.Action("PayFastCancel", "Checkout", new { id = order.Id }, Request.Scheme)!),
                new("notify_url", Url.Action("PayFastNotify", "Checkout", null, Request.Scheme)!),
                new("name_first", order.CustomerName),
                new("m_payment_id", order.Id.ToString()),
                new("amount", order.Total.ToString("F2")),
                new("item_name", $"Koolstoof order #{order.Id}"),
            };

            var signature = PayFastHelper.GenerateSignature(fields, _payFastSettings.Passphrase);

            ViewBag.ProcessUrl = _payFastSettings.ProcessUrl;
            ViewBag.Fields = fields;
            ViewBag.Signature = signature;

            return View();
        }

        [HttpGet]
        public IActionResult PayFastReturn(int id)
        {
            return RedirectToAction("Confirmation", new { id });
        }

        [HttpGet]
        public IActionResult PayFastCancel(int id)
        {
            ViewBag.OrderId = id;
            return View();
        }

        // PayFast calls this server-to-server (the ITN) once a payment completes —
        // never trust the browser return_url alone to mark an order paid.
        [HttpPost]
        public async Task<IActionResult> PayFastNotify()
        {
            var form = await Request.ReadFormAsync();

            var fields = new List<KeyValuePair<string, string>>();
            string? postedSignature = null;
            foreach (var key in form.Keys)
            {
                if (key == "signature")
                {
                    postedSignature = form[key];
                    continue;
                }
                fields.Add(new KeyValuePair<string, string>(key, form[key]!));
            }

            var expectedSignature = PayFastHelper.GenerateSignature(fields, _payFastSettings.Passphrase, isNotification: true);
            if (postedSignature == null || !string.Equals(postedSignature, expectedSignature, StringComparison.OrdinalIgnoreCase))
            {
                return BadRequest();
            }

            if (!int.TryParse(form["m_payment_id"], out var orderId))
            {
                return BadRequest();
            }

            var order = _context.Orders.Find(orderId);
            if (order == null)
            {
                return NotFound();
            }

            // The amount PayFast says it took must match what we asked for.
            if (form["payment_status"] == "COMPLETE" &&
                decimal.TryParse(form["amount_gross"], System.Globalization.NumberStyles.Number, System.Globalization.CultureInfo.InvariantCulture, out var paid) &&
                paid == order.Total)
            {
                order.IsPaid = true;
                _context.SaveChanges();
            }

            return Ok();
        }
    }
}
