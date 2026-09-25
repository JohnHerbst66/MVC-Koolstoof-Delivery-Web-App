using Koolstoof_App_1.Data;
using Microsoft.AspNetCore.Mvc;

namespace Koolstoof_App_1.Controllers
{
    // Serves photos uploaded through the admin forms (see StoredImage).
    public class MediaController : Controller
    {
        private readonly ApplicationDbContext _context;

        public MediaController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet("/media/{id:guid}")]
        public IActionResult Get(Guid id)
        {
            var image = _context.StoredImages.Find(id);
            if (image == null)
            {
                return NotFound();
            }

            // An id never changes what it points to (a new upload gets a new id), so this
            // is safe to cache for a year — the hosting CDN and browsers then stop asking
            // the database for it.
            Response.Headers.CacheControl = "public, max-age=31536000, s-maxage=31536000, immutable";
            Response.Headers["X-Content-Type-Options"] = "nosniff";
            return File(image.Data, image.ContentType);
        }
    }
}
