using Koolstoof_App_1.Services;
using Koolstoof_App_1.Models;
using Microsoft.AspNetCore.Mvc;

namespace Koolstoof_App_1.ViewComponents
{
    public class CartPreviewViewComponent : ViewComponent
    {
        private readonly CartStore _cartStore;

        public CartPreviewViewComponent(CartStore cartStore)
        {
            _cartStore = cartStore;
        }

        public IViewComponentResult Invoke()
        {
            var cart = _cartStore.Get();

            var model = new CartPreviewViewModel
            {
                TotalItems = cart.Sum(c => c.Quantity),
                TotalPrice = cart.Sum(c => c.LineTotal)
            };

            return View(model);
        }
    }
}
