namespace Koolstoof_App_1.Models
{
    public class DeliveryArea
    {
        public int Id { get; set; }
        public required string Name { get; set; }

        [System.ComponentModel.DataAnnotations.Schema.Column(TypeName = "decimal(18,2)")]
        public decimal DeliveryFee { get; set; }
    }
}
