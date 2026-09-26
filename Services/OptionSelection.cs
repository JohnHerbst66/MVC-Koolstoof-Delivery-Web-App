using Koolstoof_App_1.Models;

namespace Koolstoof_App_1.Services
{
    public class OptionSelectionResult
    {
        public bool IsValid { get; init; }
        public string? Error { get; init; }
        public List<int> ChoiceIds { get; init; } = new();

        // Readable summary saved with the cart line and the order, e.g. "Sauce: Cheese sauce; Served with: Chips".
        public string? Text { get; init; }

        public decimal ExtraPrice { get; init; }
    }

    // The single place that decides whether a set of chosen options is acceptable for a
    // menu item. Used when adding to the cart and again at checkout, so the browser is
    // never trusted: choices must belong to the item, at most one per group, and every
    // required group must be answered.
    public static class OptionSelection
    {
        // The item must have OptionGroups and their Choices loaded.
        public static OptionSelectionResult Resolve(MenuItem item, IEnumerable<int> selectedChoiceIds)
        {
            var selected = selectedChoiceIds.Distinct().ToList();
            var groups = item.OptionGroups.OrderBy(g => g.DisplayOrder).ThenBy(g => g.Id).ToList();

            var knownIds = groups.SelectMany(g => g.Choices).Select(c => c.Id).ToHashSet();
            if (selected.Any(id => !knownIds.Contains(id)))
            {
                return Invalid($"One of the options chosen for {item.Name} is no longer available. Please choose again.");
            }

            var chosen = new List<OptionChoice>();
            var parts = new List<string>();

            foreach (var group in groups)
            {
                var picked = group.Choices.Where(c => selected.Contains(c.Id)).ToList();

                if (picked.Count > 1)
                {
                    return Invalid($"Please choose only one {group.Name.ToLowerInvariant()} for {item.Name}.");
                }

                if (picked.Count == 0)
                {
                    if (group.IsRequired)
                    {
                        return Invalid($"Please choose your {group.Name.ToLowerInvariant()} for {item.Name}.");
                    }
                    continue;
                }

                chosen.Add(picked[0]);
                parts.Add($"{group.Name}: {picked[0].Name}");
            }

            return new OptionSelectionResult
            {
                IsValid = true,
                ChoiceIds = chosen.Select(c => c.Id).ToList(),
                Text = parts.Count == 0 ? null : string.Join("; ", parts),
                ExtraPrice = chosen.Sum(c => c.ExtraPrice)
            };
        }

        private static OptionSelectionResult Invalid(string error) => new() { IsValid = false, Error = error };
    }
}
