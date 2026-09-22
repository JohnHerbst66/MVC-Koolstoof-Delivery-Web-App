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
            var specials = _context.MenuItems
                .Include(m => m.Category)
                .Where(m => m.IsSpecial && m.IsInStock)
                .ToList();

            var bestSellingIds = _context.OrderItems
                .Where(oi => oi.MenuItemId != null)
                .GroupBy(oi => oi.MenuItemId)
                .Select(g => new { MenuItemId = g.Key, TotalSold = g.Sum(oi => oi.Quantity) })
                .OrderByDescending(g => g.TotalSold)
                .Take(3)
                .ToList();

            var mostSoldItems = bestSellingIds
                .Select(x => _context.MenuItems.FirstOrDefault(m => m.Id == x.MenuItemId && m.IsInStock))
                .Where(m => m != null)
                .Select(m => m!)
                .ToList();

            var viewModel = new HomeViewModel
            {
                Specials = specials,
                MostSoldItems = mostSoldItems
            };

            return View(viewModel);
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
