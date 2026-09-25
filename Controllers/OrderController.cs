using Koolstoof_App_1.Data;
using Koolstoof_App_1.Helpers;
using Koolstoof_App_1.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Koolstoof_App_1.Controllers
{
    [Authorize(Roles = "Admin")]
    public class OrderController : Controller
    {
        private readonly ApplicationDbContext _context;

        public OrderController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Manage()
        {
            var orders = _context.Orders
                .Include(o => o.OrderItems)
                .Include(o => o.DeliveryArea)
                .Where(o => o.ArchivedAt == null)
                .OrderByDescending(o => o.PlacedAt)
                .ToList();

            ViewBag.HiddenCount = _context.Orders.Count(o => o.ArchivedAt != null);
            return View(orders);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Advance(int id)
        {
            var order = _context.Orders.Find(id);
            if (order == null)
            {
                return NotFound();
            }

            if (order.ArchivedAt == null && order.Status < OrderStatus.Delivered)
            {
                order.Status = (OrderStatus)((int)order.Status + 1);

                if (order.Status == OrderStatus.Delivered && order.PaymentMethod == PaymentMethod.CashOnDelivery)
                {
                    order.IsPaid = true;
                }

                _context.SaveChanges();
            }

            return RedirectToAction("Manage");
        }

        // ----- Hiding and deleting -----
        // Nothing here happens from a single click: every action is reached through a
        // confirmation page, and deleting also needs the order number typed in.

        public IActionResult Hidden()
        {
            var orders = _context.Orders
                .Include(o => o.OrderItems)
                .Include(o => o.DeliveryArea)
                .Where(o => o.ArchivedAt != null)
                .OrderByDescending(o => o.PlacedAt)
                .ToList();

            return View(orders);
        }

        public IActionResult Remove(int id)
        {
            var order = LoadOrder(id);
            if (order == null)
            {
                return NotFound();
            }

            order.CanBeDeleted(out var reason);
            ViewBag.DeleteBlockedReason = reason;
            return View(order);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Hide(int id)
        {
            var order = _context.Orders.Find(id);
            if (order == null)
            {
                return NotFound();
            }

            if (order.ArchivedAt == null)
            {
                order.ArchivedAt = SouthAfricaTime.Now;
                _context.SaveChanges();
            }

            TempData["OrderMessage"] = $"Order #{order.Id} is hidden. You can bring it back from Hidden orders.";
            return RedirectToAction("Manage");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Restore(int id)
        {
            var order = _context.Orders.Find(id);
            if (order == null)
            {
                return NotFound();
            }

            order.ArchivedAt = null;
            _context.SaveChanges();

            TempData["OrderMessage"] = $"Order #{order.Id} is back on the board.";
            return RedirectToAction("Hidden");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Delete(int id, string? confirmNumber)
        {
            var order = LoadOrder(id);
            if (order == null)
            {
                return NotFound();
            }

            // Enforced here as well as on the page, so no request can delete a paid order.
            if (!order.CanBeDeleted(out var reason))
            {
                TempData["OrderError"] = reason;
                return RedirectToAction("Remove", new { id });
            }

            if ((confirmNumber ?? "").Trim().TrimStart('#') != order.Id.ToString())
            {
                TempData["OrderError"] = $"To delete, type the order number ({order.Id}) exactly. Nothing was deleted.";
                return RedirectToAction("Remove", new { id });
            }

            _context.OrderItems.RemoveRange(order.OrderItems);
            _context.Orders.Remove(order);
            _context.SaveChanges();

            TempData["OrderMessage"] = $"Order #{id} was deleted permanently.";
            return RedirectToAction("Manage");
        }

        public IActionResult ClearDelivered()
        {
            var delivered = _context.Orders
                .Where(o => o.ArchivedAt == null && o.Status == OrderStatus.Delivered)
                .OrderBy(o => o.PlacedAt)
                .ToList();

            return View(delivered);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ClearDeliveredConfirmed()
        {
            var now = SouthAfricaTime.Now;
            var delivered = _context.Orders
                .Where(o => o.ArchivedAt == null && o.Status == OrderStatus.Delivered)
                .ToList();

            foreach (var order in delivered)
            {
                order.ArchivedAt = now;
            }
            _context.SaveChanges();

            TempData["OrderMessage"] = delivered.Count == 1
                ? "1 delivered order was hidden."
                : $"{delivered.Count} delivered orders were hidden.";
            return RedirectToAction("Manage");
        }

        private Order? LoadOrder(int id) => _context.Orders
            .Include(o => o.OrderItems)
            .Include(o => o.DeliveryArea)
            .FirstOrDefault(o => o.Id == id);
    }
}
