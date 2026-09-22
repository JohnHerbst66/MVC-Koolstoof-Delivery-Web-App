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
            bool hasItems =_context.MenuItems.Any(m => m.CategoryId == id);
            if (hasItems)
            {
                return RedirectToAction("Delete", new { id = id });
            }


            var category = _context.MenuCategories.Find(id);
            if (category != null)
            {
                _context.MenuCategories.Remove(category);
                _context.SaveChanges();
            }
           
            return RedirectToAction("Manage","Menu");
        }
    }
}
