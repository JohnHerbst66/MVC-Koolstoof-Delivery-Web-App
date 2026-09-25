using Koolstoof_App_1.Helpers;

using Koolstoof_App_1.Data;
using Koolstoof_App_1.Models;
using Microsoft.EntityFrameworkCore;

namespace Koolstoof_App_1.Services
{
    public static class SpecialsService
    {
        // Re-checks every not-yet-ended special against the current date/time/day/stock level,
        // and syncs MenuItem.IsSpecial / SpecialPrice to match. Called at the top of any page
        // that displays specials, so state is always correct without needing a background job.
        public static void SyncActiveSpecials(ApplicationDbContext context)
        {
            var specials = context.Specials
                .Include(s => s.MenuItem)
                .Where(s => !s.IsEnded)
                .ToList();

            var now = SouthAfricaTime.Now;
            var today = now.DayOfWeek;

            foreach (var special in specials)
            {
                bool activeNow = special.EndCondition switch
                {
                    SpecialEndCondition.SpecificDays => IsDayActive(special, today),
                    SpecialEndCondition.UntilOutOfStock => special.MenuItem.IsInStock,
                    SpecialEndCondition.DateRange => now >= (special.StartAt ?? DateTime.MinValue) && now <= (special.EndAt ?? DateTime.MaxValue),
                    SpecialEndCondition.UntilStopped => true,
                    _ => false
                };

                if (activeNow)
                {
                    special.MenuItem.IsSpecial = true;
                    special.MenuItem.SpecialPrice = special.SpecialPrice;
                    special.MenuItem.IsSitDownSpecial = special.SitDownOnly;
                    special.MenuItem.SpecialImageUrl = special.ImageUrl;
                }
                else
                {
                    special.MenuItem.IsSpecial = false;
                    special.MenuItem.SpecialPrice = null;
                    special.MenuItem.IsSitDownSpecial = false;
                    special.MenuItem.SpecialImageUrl = null;

                    // These two conditions are one-shot: once they lapse, they're done for good.
                    if (special.EndCondition == SpecialEndCondition.DateRange && special.EndAt.HasValue && now > special.EndAt.Value)
                    {
                        special.IsEnded = true;
                    }
                    if (special.EndCondition == SpecialEndCondition.UntilOutOfStock && !special.MenuItem.IsInStock)
                    {
                        special.IsEnded = true;
                    }
                    // SpecificDays intentionally does NOT set IsEnded here — it just sits
                    // inactive until its next matching day, recurring until manually stopped.
                }
            }

            context.SaveChanges();
        }

        public static string Describe(Special special)
        {
            return special.EndCondition switch
            {
                SpecialEndCondition.SpecificDays => "Active on: " + string.Join(", ", ActiveDayNames(special)),
                SpecialEndCondition.UntilOutOfStock => "Until stock lasts",
                SpecialEndCondition.DateRange => $"{special.StartAt:dd MMM yyyy HH:mm} – {special.EndAt:dd MMM yyyy HH:mm}",
                SpecialEndCondition.UntilStopped => "Until stopped",
                _ => ""
            };
        }

        private static IEnumerable<string> ActiveDayNames(Special special)
        {
            if (special.ActiveMonday) yield return "Mon";
            if (special.ActiveTuesday) yield return "Tue";
            if (special.ActiveWednesday) yield return "Wed";
            if (special.ActiveThursday) yield return "Thu";
            if (special.ActiveFriday) yield return "Fri";
            if (special.ActiveSaturday) yield return "Sat";
            if (special.ActiveSunday) yield return "Sun";
        }

        private static bool IsDayActive(Special special, DayOfWeek day) => day switch
        {
            DayOfWeek.Monday => special.ActiveMonday,
            DayOfWeek.Tuesday => special.ActiveTuesday,
            DayOfWeek.Wednesday => special.ActiveWednesday,
            DayOfWeek.Thursday => special.ActiveThursday,
            DayOfWeek.Friday => special.ActiveFriday,
            DayOfWeek.Saturday => special.ActiveSaturday,
            DayOfWeek.Sunday => special.ActiveSunday,
            _ => false
        };
    }
}
