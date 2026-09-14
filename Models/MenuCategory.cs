namespace Koolstoof_App_1.Models
{
    public class MenuCategory
    {

        public int Id { get; set; }
        public required string Name { get; set; }
        public int DisplayOrder { get; set; }
        public ICollection<MenuItem> MenuItems { get; set; } = new List<MenuItem>();
    }
}
