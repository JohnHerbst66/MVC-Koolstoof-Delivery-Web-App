using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace Koolstoof_App_1.Models
{
    // A question a customer answers when ordering an item, e.g. "Sauce" or "Served with".
    // The customer picks one of its choices.
    public class OptionGroup
    {
        public const int MaxGroupsPerItem = 8;

        public int Id { get; set; }

        public int MenuItemId { get; set; }

        [ValidateNever]
        public MenuItem MenuItem { get; set; } = null!;

        public required string Name { get; set; }

        // A required group must be answered before the item can be added to the cart.
        public bool IsRequired { get; set; } = true;

        public int DisplayOrder { get; set; }

        [ValidateNever]
        public ICollection<OptionChoice> Choices { get; set; } = new List<OptionChoice>();
    }
}
