using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace Koolstoof_App_1.Models
{
    // One answer within an OptionGroup, e.g. "Mushroom sauce", optionally with a surcharge.
    public class OptionChoice
    {
        public const int MaxChoicesPerGroup = 25;

        public int Id { get; set; }

        public int OptionGroupId { get; set; }

        [ValidateNever]
        public OptionGroup OptionGroup { get; set; } = null!;

        public required string Name { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal ExtraPrice { get; set; }

        public int DisplayOrder { get; set; }
    }
}
