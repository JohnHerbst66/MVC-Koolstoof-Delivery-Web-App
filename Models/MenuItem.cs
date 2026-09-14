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
        public string? ImageUrl { get; set; }
        public bool IsInStock { get; set; } =true;
        public bool IsSpecial { get; set; }=false;
        [Column(TypeName = "decimal(18,2)")]
        public decimal? SpecialPrice { get; set; }
        public int CategoryId { get; set; }

        [ValidateNever]
        public MenuCategory Category { get; set; } = null!;

    }
}
