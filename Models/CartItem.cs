using System.Text.Json.Serialization;

namespace Koolstoof_App_1.Models
{
    public class CartItem
    {
        public int MenuItemId { get; set; }
        public required string Name { get; set; }
        public decimal Price { get; set; }
        public int Quantity { get; set; }

        // The options picked for this line (sauce, side...). Two lines of the same item with
        // different choices are different lines.
        public List<int> ChoiceIds { get; set; } = new();
        public string? Choices { get; set; }

        [JsonIgnore]
        public string LineKey => ChoiceIds.Count == 0
            ? MenuItemId.ToString()
            : $"{MenuItemId}:{string.Join("-", ChoiceIds.OrderBy(id => id))}";

        // Computed — kept out of the cart cookie to keep it small.
        [JsonIgnore]
        public decimal LineTotal => Price * Quantity;
    }
}
