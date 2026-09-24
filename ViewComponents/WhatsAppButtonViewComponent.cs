using Koolstoof_App_1.Data;
using Microsoft.AspNetCore.Mvc;

namespace Koolstoof_App_1.ViewComponents
{
    public class WhatsAppButtonViewComponent : ViewComponent
    {
        private readonly ApplicationDbContext _context;

        public WhatsAppButtonViewComponent(ApplicationDbContext context)
        {
            _context = context;
        }

        public IViewComponentResult Invoke()
        {
            var number = _context.RestaurantSettings.First().WhatsAppNumber;
            return View(model: number);
        }
    }
}
