namespace Koolstoof_App_1.Helpers
{
    public static class ImageUploadHelper
    {
        private static readonly string[] AllowedExtensions = [".jpg", ".jpeg", ".png", ".webp", ".gif"];
        private const long MaxFileSizeBytes = 5 * 1024 * 1024;

        // Saves an uploaded photo into wwwroot/images/uploads and returns the URL to store
        // on the entity (e.g. Special.ImageUrl). Returns (null, null) when no file was chosen —
        // that's not an error, it just means the admin didn't pick a new photo.
        public static async Task<(string? Url, string? Error)> SaveAsync(IFormFile? file, string webRootPath)
        {
            if (file == null || file.Length == 0)
            {
                return (null, null);
            }

            if (file.Length > MaxFileSizeBytes)
            {
                return (null, "That image is too large — please use one under 5MB.");
            }

            var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!AllowedExtensions.Contains(ext))
            {
                return (null, "Please upload a JPG, PNG, WEBP, or GIF image.");
            }

            var uploadsFolder = Path.Combine(webRootPath, "images", "uploads");
            Directory.CreateDirectory(uploadsFolder);

            var fileName = $"{Guid.NewGuid()}{ext}";
            var filePath = Path.Combine(uploadsFolder, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            return ($"/images/uploads/{fileName}", null);
        }
    }
}
