using Koolstoof_App_1.Data;
using Koolstoof_App_1.Models;

namespace Koolstoof_App_1.Helpers
{
    public static class ImageUploadHelper
    {
        // Content type comes from the extension we accept, never from the client, so an
        // uploaded file can only ever be served back as one of these image types.
        private static readonly Dictionary<string, string> AllowedTypes = new()
        {
            [".jpg"] = "image/jpeg",
            [".jpeg"] = "image/jpeg",
            [".png"] = "image/png",
            [".webp"] = "image/webp",
            [".gif"] = "image/gif"
        };

        // Vercel rejects request bodies over 4.5 MB, and the upload forms shrink photos in
        // the browser first, so anything near this size means something is wrong.
        private const long MaxFileSizeBytes = 4 * 1024 * 1024;

        // Saves an uploaded photo into the database and returns the URL to store on the
        // entity (e.g. Special.ImageUrl). Returns (null, null) when no file was chosen —
        // that's not an error, it just means the admin didn't pick a new photo.
        public static async Task<(string? Url, string? Error)> SaveAsync(IFormFile? file, ApplicationDbContext context)
        {
            if (file == null || file.Length == 0)
            {
                return (null, null);
            }

            if (file.Length > MaxFileSizeBytes)
            {
                return (null, "That image is too large — please use one under 4MB.");
            }

            var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!AllowedTypes.TryGetValue(ext, out var contentType))
            {
                return (null, "Please upload a JPG, PNG, WEBP, or GIF image.");
            }

            using var memory = new MemoryStream();
            await file.CopyToAsync(memory);

            var image = new StoredImage { ContentType = contentType, Data = memory.ToArray() };
            context.StoredImages.Add(image);
            await context.SaveChangesAsync();

            return ($"/media/{image.Id}", null);
        }
    }
}
