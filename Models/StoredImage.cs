namespace Koolstoof_App_1.Models
{
    // An admin-uploaded photo, kept in the database. The site is hosted somewhere with
    // no permanent local disk (Vercel), and photos are shrunk in the browser before
    // upload so each is a few hundred KB — a menu's worth is tiny.
    public class StoredImage
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public required string ContentType { get; set; }
        public required byte[] Data { get; set; }
        public DateTime CreatedAt { get; set; } = Helpers.SouthAfricaTime.Now;
    }
}
