using Koolstoof_App_1.Extensions;
using Koolstoof_App_1.Models;
using Microsoft.AspNetCore.Mvc;

namespace Koolstoof_App_1.ViewComponents
{
    public class CartPreviewViewComponent : ViewComponent
    {
        private const string CartSessionKey = "Cart";

        public IViewComponentResult Invoke()
        {
            var cart = HttpContext.Session.GetObjectFromJson<List<CartItem>>(CartSessionKey) ?? new List<CartItem>();

            var model = new CartPreviewViewModel
            {
                TotalItems = cart.Sum(c => c.Quantity),
                TotalPrice = cart.Sum(c => c.LineTotal)
            };

            return View(model);
        }
    }
}
