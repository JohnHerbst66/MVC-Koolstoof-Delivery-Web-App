using Koolstoof_App_1.Data;
using Koolstoof_App_1.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Koolstoof_App_1.Controllers
{
    [Authorize(Roles = "Admin")]
    public class DeliveryAreaController : Controller
    {
        private readonly ApplicationDbContext _context;

        public DeliveryAreaController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(DeliveryArea deliveryArea)
        {
            if (!ModelState.IsValid)
            {
                return View(deliveryArea);
            }

            _context.DeliveryAreas.Add(deliveryArea);
            _context.SaveChanges();
            return RedirectToAction("Index", "Settings");
        }

        [HttpGet]
        public IActionResult Edit(int id)
        {
            var area = _context.DeliveryAreas.Find(id);
            if (area == null)
            {
                return NotFound();
            }
            return View(area);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(int id, DeliveryArea deliveryArea)
        {
            if (id != deliveryArea.Id)
            {
                return BadRequest();
            }
            if (!ModelState.IsValid)
            {
                return View(deliveryArea);
            }

            var area = _context.DeliveryAreas.Find(id);
            if (area == null)
            {
                return NotFound();
            }

            area.Name = deliveryArea.Name;
            area.DeliveryFee = deliveryArea.DeliveryFee;
            _context.SaveChanges();

            return RedirectToAction("Index", "Settings");
        }

        [HttpGet]
        public IActionResult Delete(int id)
        {
            var area = _context.DeliveryAreas.Find(id);
            if (area == null)
            {
                return NotFound();
            }
            ViewBag.OrderCount = _context.Orders.Count(o => o.DeliveryAreaId == id);
            return View(area);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            var area = _context.DeliveryAreas.Find(id);
            if (area == null)
            {
                return NotFound();
            }

            if (_context.Orders.Any(o => o.DeliveryAreaId == id))
            {
                // Orders reference this area for their delivery-fee history — never
                // delete out from under them, same reasoning as MenuItem history.
                return RedirectToAction("Delete", new { id });
            }

            _context.DeliveryAreas.Remove(area);
            _context.SaveChanges();

            return RedirectToAction("Index", "Settings");
        }
    }
}
