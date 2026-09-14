using Microsoft.AspNetCore.Mvc;
using Koolstoof_App_1.Data;
using Koolstoof_App_1.Models;

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
            return RedirectToAction("Index","Menu");
        }
        public IActionResult Index()
        {
            return View();
        }
    }
}
