using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Koolstoof_App_1.Controllers
{
    [Route("Admin")]
    public class AdminController : Controller
    {
        private readonly SignInManager<IdentityUser> _signInManager;

        public AdminController(SignInManager<IdentityUser> signInManager)
        {
            _signInManager = signInManager;
        }

        [HttpGet("")]
        public IActionResult Index()
        {
            if (User.Identity?.IsAuthenticated == true && User.IsInRole("Admin"))
            {
                return RedirectToAction("Manage", "Order");
            }

            return View();
        }

        [HttpPost("Login")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(string adminId, string password)
        {
            if (string.IsNullOrWhiteSpace(adminId) || string.IsNullOrWhiteSpace(password))
            {
                ModelState.AddModelError("", "Enter your admin ID and password.");
                return View("Index");
            }

            var result = await _signInManager.PasswordSignInAsync(adminId, password, isPersistent: false, lockoutOnFailure: true);

            if (result.Succeeded)
            {
                return RedirectToAction("Manage", "Order");
            }

            ModelState.AddModelError("", "Invalid admin ID or password.");
            return View("Index");
        }

        [HttpPost("Logout")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }
    }
}
