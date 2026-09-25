using Koolstoof_App_1.Helpers;

using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace Koolstoof_App_1.Models
{
    public class Order
    {
        public int Id { get; set; }
        public DateTime PlacedAt { get; set; } = SouthAfricaTime.Now;

        public required string CustomerName { get; set; }
        public required string CustomerPhone { get; set; }
        public required string DeliveryAddress { get; set; }

        public int DeliveryAreaId { get; set; }

        [ValidateNever]
        public DeliveryArea DeliveryArea { get; set; } = null!;

        [Column(TypeName = "decimal(18,2)")]
        public decimal DeliveryFee { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Subtotal { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Total { get; set; }

        public PaymentMethod PaymentMethod { get; set; } = PaymentMethod.CashOnDelivery;
        public bool IsPaid { get; set; } = false;

        public OrderStatus Status { get; set; } = OrderStatus.Incoming;

        // Set when an admin hides the order from the board. The row is kept for the
        // records; only a never-paid order can ever be deleted for real.
        public DateTime? ArchivedAt { get; set; }

        [ValidateNever]
        public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();

        // How long a PayFast order that hasn't been marked paid is protected from deletion,
        // because PayFast's confirmation can arrive after the customer returns to the site.
        public static readonly TimeSpan PayFastGracePeriod = TimeSpan.FromHours(24);

        // The single rule for permanent deletion. Paid orders can never be deleted, only hidden.
        public bool CanBeDeleted(out string reason)
        {
            if (IsPaid)
            {
                reason = "This order has been paid, so it is kept for your records and can only be hidden, never deleted.";
                return false;
            }

            if (PaymentMethod == PaymentMethod.PayFast && SouthAfricaTime.Now - PlacedAt < PayFastGracePeriod)
            {
                reason = "This PayFast order was placed less than 24 hours ago. The customer's payment may still be confirming, so it cannot be deleted yet. You can hide it instead.";
                return false;
            }

            reason = "";
            return true;
        }
    }
}
