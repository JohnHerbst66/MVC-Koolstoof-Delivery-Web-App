using System.Text.Json;
using Koolstoof_App_1.Models;
using Microsoft.AspNetCore.DataProtection;

namespace Koolstoof_App_1.Services
{
    // Holds the shopping cart in an encrypted, signed cookie instead of server memory.
    // Hosts like Vercel run the app as short-lived containers, so anything kept in
    // process memory (the old session cart) disappears between requests. Data
    // Protection makes the cookie tamper-proof: a customer can't edit prices in it.
    public class CartStore
    {
        private const string CookieName = "koolstoof_cart";
        private const int MaxCookieLength = 3800; // browsers cap a cookie around 4096 bytes

        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IDataProtector _protector;

        public CartStore(IHttpContextAccessor httpContextAccessor, IDataProtectionProvider dataProtectionProvider)
        {
            _httpContextAccessor = httpContextAccessor;
            _protector = dataProtectionProvider.CreateProtector("Koolstoof.Cart.v1");
        }

        public List<CartItem> Get()
        {
            var context = _httpContextAccessor.HttpContext;
            if (context == null || !context.Request.Cookies.TryGetValue(CookieName, out var protectedValue))
            {
                return new List<CartItem>();
            }

            try
            {
                var json = _protector.Unprotect(protectedValue);
                return JsonSerializer.Deserialize<List<CartItem>>(json) ?? new List<CartItem>();
            }
            catch
            {
                // Tampered with, from an old key, or corrupt — treat as an empty cart.
                return new List<CartItem>();
            }
        }

        // Returns false when the cart is too big to fit in a cookie; nothing is saved.
        public bool Save(List<CartItem> cart)
        {
            var context = _httpContextAccessor.HttpContext!;
            var protectedValue = _protector.Protect(JsonSerializer.Serialize(cart));
            if (protectedValue.Length > MaxCookieLength)
            {
                return false;
            }

            context.Response.Cookies.Append(CookieName, protectedValue, new CookieOptions
            {
                HttpOnly = true,
                Secure = context.Request.IsHttps,
                SameSite = SameSiteMode.Lax,
                IsEssential = true,
                Expires = DateTimeOffset.UtcNow.AddDays(1)
            });
            return true;
        }

        public void Clear()
        {
            _httpContextAccessor.HttpContext!.Response.Cookies.Delete(CookieName);
        }
    }
}
