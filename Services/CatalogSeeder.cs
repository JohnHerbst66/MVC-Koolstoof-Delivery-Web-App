using Koolstoof_App_1.Data;
using Koolstoof_App_1.Models;

namespace Koolstoof_App_1.Services
{
    public static class CatalogSeeder
    {
        // MenuCategoryController relies on this catch-all category existing: deleting a
        // category moves its items here instead of orphaning them. It used to be created
        // by hand, so a brand-new database wouldn't have it.
        public static async Task SeedAsync(IServiceProvider services)
        {
            var context = services.GetRequiredService<ApplicationDbContext>();

            if (!context.MenuCategories.Any(c => c.IsUncategorized))
            {
                context.MenuCategories.Add(new MenuCategory
                {
                    Name = "Uncategorized",
                    DisplayOrder = 999,
                    IsUncategorized = true
                });
                await context.SaveChangesAsync();
            }
        }
    }
}
