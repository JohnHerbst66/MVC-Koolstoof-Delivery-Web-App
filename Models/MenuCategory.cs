namespace Koolstoof_App_1.Models
{
    public class MenuCategory
    {

        public int Id { get; set; }
        public required string Name { get; set; }
        public int DisplayOrder { get; set; }
        public string? ImageUrl { get; set; }
        public bool IsUncategorized { get; set; } = false;
        public ICollection<MenuItem> MenuItems { get; set; } = new List<MenuItem>();
    }
}
