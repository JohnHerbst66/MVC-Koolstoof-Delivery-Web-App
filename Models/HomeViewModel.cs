namespace Koolstoof_App_1.Models
{
    public class HomeViewModel
    {
        public List<MenuItem> Specials { get; set; } = new();
        public List<MenuItem> MostSoldItems { get; set; } = new();
        public List<MenuCategory> Categories { get; set; } = new();
    }
}
