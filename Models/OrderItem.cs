using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace Koolstoof_App_1.Models
{
    public class OrderItem
    {
        public int Id { get; set; }

        public int OrderId { get; set; }

        [ValidateNever]
        public Order Order { get; set; } = null!;

        public int? MenuItemId { get; set; }

        [ValidateNever]
        public MenuItem? MenuItem { get; set; }

        public required string MenuItemName { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal UnitPrice { get; set; }

        public int Quantity { get; set; }

        [NotMapped]
        public decimal LineTotal => UnitPrice * Quantity;
    }
}
