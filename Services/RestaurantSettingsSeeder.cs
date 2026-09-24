using Koolstoof_App_1.Data;
using Koolstoof_App_1.Models;

namespace Koolstoof_App_1.Services
{
    public static class RestaurantSettingsSeeder
    {
        public static async Task SeedAsync(IServiceProvider services)
        {
            var context = services.GetRequiredService<ApplicationDbContext>();

            if (!context.RestaurantSettings.Any())
            {
                context.RestaurantSettings.Add(new RestaurantSettings());
                await context.SaveChangesAsync();
            }
        }
    }
}
