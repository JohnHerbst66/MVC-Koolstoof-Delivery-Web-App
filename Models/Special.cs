using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace Koolstoof_App_1.Models
{
    public class Special
    {
        public int Id { get; set; }

        public int MenuItemId { get; set; }

        [ValidateNever]
        public MenuItem MenuItem { get; set; } = null!;

        [Column(TypeName = "decimal(18,2)")]
        public decimal SpecialPrice { get; set; }

        public string? ImageUrl { get; set; }

        public SpecialEndCondition EndCondition { get; set; }

        // When true, the discount only applies to customers dining in at the restaurant.
        // Online/delivery orders through the site are still charged the item's normal price.
        public bool SitDownOnly { get; set; }

        // Used when EndCondition == SpecificDays. The special is active (and reverts each
        // day it doesn't apply) for as long as this keeps recurring, until manually ended.
        public bool ActiveMonday { get; set; }
        public bool ActiveTuesday { get; set; }
        public bool ActiveWednesday { get; set; }
        public bool ActiveThursday { get; set; }
        public bool ActiveFriday { get; set; }
        public bool ActiveSaturday { get; set; }
        public bool ActiveSunday { get; set; }

        // Used when EndCondition == DateRange.
        public DateTime? StartAt { get; set; }
        public DateTime? EndAt { get; set; }

        // True once the special is permanently done, whether the admin stopped/deleted it,
        // a date range lapsed, or the item ran out of stock. SpecificDays specials never set
        // this on their own since they're meant to recur week to week.
        public bool IsEnded { get; set; } = false;

        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
