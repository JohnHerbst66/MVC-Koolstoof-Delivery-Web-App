using Koolstoof_App_1.Data;
using Koolstoof_App_1.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Koolstoof_App_1.Controllers
{
    public class MenuCategoryController : Controller
    {
        private readonly ApplicationDbContext _context;
        public MenuCategoryController(ApplicationDbContext context)
        {
            _context = context;
        }
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }
        [HttpPost]
        public IActionResult Create(MenuCategory menuCategory)
        {
            if (!ModelState.IsValid)
            {
               return View(menuCategory);  
            }
            _context.MenuCategories.Add(menuCategory);
            _context.SaveChanges();
            return RedirectToAction("Manage","Menu");
        }
        




        [HttpGet]
        public IActionResult Delete(int id)
        {
            var category = _context.MenuCategories.Find(id);
            if (category == null)
            {
                return NotFound();
            }
            ViewBag.ItemCount = _context.MenuItems.Count(m => m.CategoryId == id);
            return View(category);
        }
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            var category = _context.MenuCategories.Find(id);
            if (category == null)
            {
                return NotFound();
            }

            // The Uncategorized bucket itself can never be deleted.
            if (category.IsUncategorized)
            {
                return RedirectToAction("Manage", "Menu");
            }

            var uncategorized = _context.MenuCategories.FirstOrDefault(c => c.IsUncategorized);
            if (uncategorized != null)
            {
                var orphanedItems = _context.MenuItems.Where(m => m.CategoryId == id);
                foreach (var item in orphanedItems)
                {
                    item.CategoryId = uncategorized.Id;
                }
            }

            _context.MenuCategories.Remove(category);
            _context.SaveChanges();

            return RedirectToAction("Manage","Menu");
        }
    }
}
