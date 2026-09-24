using Koolstoof_App_1.Data;
using Koolstoof_App_1.Helpers;
using Koolstoof_App_1.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Koolstoof_App_1.Controllers
{
    [Authorize(Roles = "Admin")]
    public class MenuCategoryController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _env;
        public MenuCategoryController(ApplicationDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(MenuCategory menuCategory, IFormFile? imageFile)
        {
            var (uploadedUrl, uploadError) = await ImageUploadHelper.SaveAsync(imageFile, _env.WebRootPath);
            if (uploadError != null)
            {
                ModelState.AddModelError("", uploadError);
            }

            if (!ModelState.IsValid)
            {
               return View(menuCategory);
            }

            if (uploadedUrl != null)
            {
                menuCategory.ImageUrl = uploadedUrl;
            }

            _context.MenuCategories.Add(menuCategory);
            _context.SaveChanges();
            return RedirectToAction("Manage","Menu");
        }
        




        [HttpGet]
        public IActionResult Edit(int id)
        {
            var category = _context.MenuCategories.Find(id);
            if (category == null)
            {
                return NotFound();
            }
            if (category.IsUncategorized)
            {
                return RedirectToAction("Manage", "Menu");
            }
            return View(category);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, MenuCategory menuCategory, IFormFile? imageFile)
        {
            if (id != menuCategory.Id)
            {
                return BadRequest();
            }

            var (uploadedUrl, uploadError) = await ImageUploadHelper.SaveAsync(imageFile, _env.WebRootPath);
            if (uploadError != null)
            {
                ModelState.AddModelError("", uploadError);
            }

            if (!ModelState.IsValid)
            {
                return View(menuCategory);
            }

            var category = _context.MenuCategories.Find(id);
            if (category == null)
            {
                return NotFound();
            }
            if (category.IsUncategorized)
            {
                return RedirectToAction("Manage", "Menu");
            }

            category.Name = menuCategory.Name;
            category.DisplayOrder = menuCategory.DisplayOrder;
            category.ImageUrl = uploadedUrl ?? menuCategory.ImageUrl;
            _context.SaveChanges();

            return RedirectToAction("Manage", "Menu");
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
