using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
namespace Koolstoof_App_1.Models
{
    public class MenuItem
    {
        public int Id { get; set; }
        public required string Name { get; set; }
        public string? Description { get; set; }

        [Column (TypeName = "decimal(18,2)")]
        public required decimal Price { get; set; }
        public bool IsInStock { get; set; } =true;
        public bool IsSpecial { get; set; }=false;
        public bool IsDeliverable { get; set; } = true;
        public bool IsSitDownSpecial { get; set; } = false;
        [Column(TypeName = "decimal(18,2)")]
        public decimal? SpecialPrice { get; set; }
        public string? SpecialImageUrl { get; set; }
        public int CategoryId { get; set; }

        [ValidateNever]
        public MenuCategory Category { get; set; } = null!;

        // Choices a customer makes when ordering, e.g. sauce or side. Empty for most items.
        [ValidateNever]
        public ICollection<OptionGroup> OptionGroups { get; set; } = new List<OptionGroup>();

        // What a delivery customer pays for the item itself (before any option surcharges).
        // A sit-down special is in-store only, so it never applies here.
        [NotMapped]
        public decimal DeliveryPrice => IsSpecial && !IsSitDownSpecial && SpecialPrice.HasValue ? SpecialPrice.Value : Price;

    }
}
