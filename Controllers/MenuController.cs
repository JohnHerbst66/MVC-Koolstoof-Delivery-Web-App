using Koolstoof_App_1.Data;
using Koolstoof_App_1.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;


namespace Koolstoof_App_1.Controllers
{
    public class MenuController : Controller
    {
        private readonly ApplicationDbContext _context;
        public MenuController(ApplicationDbContext context)
        {
            _context = context;
        }
        public IActionResult Index()
        {
            var categories = _context.MenuCategories
                .Include(c => c.MenuItems)
                .OrderBy(c => c.DisplayOrder)
                .ToList();
            return View(categories);
        }

        public IActionResult Manage()
        {
            var categories = _context.MenuCategories
                .Include(c => c.MenuItems)
                .OrderBy(c => c.DisplayOrder)
                .ToList();
            return View(categories);
        }
        [HttpGet]
        public IActionResult Create()
        {
            ViewBag.Categories = new SelectList(_context.MenuCategories, "Id", "Name");
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(MenuItem item)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Categories = new SelectList(_context.MenuCategories, "Id", "Name");
                return View(item);
            }
            _context.MenuItems.Add(item);
            _context.SaveChanges();
            return RedirectToAction("Manage");
        }

        //||Edit actions||\\
        [HttpGet]
        public IActionResult Edit(int id)
        {
            var item = _context.MenuItems.Find(id);
            if (item == null)
            {
                return NotFound();
            }
            ViewBag.Categories = new SelectList(_context.MenuCategories, "Id", "Name");
            return View(item);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(int id, MenuItem item)
        {
            if (id != item.Id)
            {
                return BadRequest();
            }
            if (!ModelState.IsValid)
            {
                ViewBag.Categories = new SelectList(_context.MenuCategories, "Id", "Name");
                return View(item);
            }
            _context.MenuItems.Update(item);
            _context.SaveChanges();
            return RedirectToAction("Manage");
        }


        [HttpGet]
        public IActionResult Delete(int id)
        {
            var item = _context.MenuItems.Include(m => m.Category).FirstOrDefault(m => m.Id == id);
            if (item == null) {
                return NotFound();
            }
            ViewBag.HasOrderHistory = _context.OrderItems.Any(oi => oi.MenuItemId == id);
            return View(item);
        }
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            var item = _context.MenuItems.Find(id);
            if (item == null)
            {
                return NotFound();
            }
            if (_context.OrderItems.Any(oi => oi.MenuItemId == id))
            {
                return RedirectToAction("Delete", new { id });
            }
            _context.MenuItems.Remove(item);
            _context.SaveChanges();
            return RedirectToAction("Manage");
        }


    }




}

