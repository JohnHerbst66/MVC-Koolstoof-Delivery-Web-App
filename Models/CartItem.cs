namespace Koolstoof_App_1.Models
{
    public class CartItem
    {
        public int MenuItemId { get; set; }
        public required string Name { get; set; }
        public decimal Price { get; set; }
        public int Quantity { get; set; }

        public decimal LineTotal => Price * Quantity;
    }
}
