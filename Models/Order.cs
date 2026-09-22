using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace Koolstoof_App_1.Models
{
    public class Order
    {
        public int Id { get; set; }
        public DateTime PlacedAt { get; set; } = DateTime.Now;

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

        [ValidateNever]
        public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
    }
}
