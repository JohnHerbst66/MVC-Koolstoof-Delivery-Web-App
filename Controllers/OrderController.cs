using Koolstoof_App_1.Data;
using Koolstoof_App_1.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Koolstoof_App_1.Controllers
{
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
                .OrderByDescending(o => o.PlacedAt)
                .ToList();

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

            if (order.Status < OrderStatus.Delivered)
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
    }
}
