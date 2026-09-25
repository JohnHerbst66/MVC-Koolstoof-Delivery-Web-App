using System.Text.Json.Serialization;

namespace Koolstoof_App_1.Models
{
    public class CartItem
    {
        public int MenuItemId { get; set; }
        public required string Name { get; set; }
        public decimal Price { get; set; }
        public int Quantity { get; set; }

        // Computed — kept out of the cart cookie to keep it small.
        [JsonIgnore]
        public decimal LineTotal => Price * Quantity;
    }
}
