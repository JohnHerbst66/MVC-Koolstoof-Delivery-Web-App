using Koolstoof_App_1.Data;
using Koolstoof_App_1.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

namespace Koolstoof_App_1.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;

        public HomeController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            // TODO: once Orders/Checkout exist, swap this for a real "most sold" query.
            var featuredItems = _context.MenuItems
                .Include(m => m.Category)
                .Where(m => m.IsSpecial && m.IsInStock)
                .Take(3)
                .ToList();

            return View(featuredItems);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
