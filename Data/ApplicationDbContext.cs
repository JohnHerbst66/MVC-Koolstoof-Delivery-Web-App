using Koolstoof_App_1.Models;
using Microsoft.AspNetCore.DataProtection.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Koolstoof_App_1.Data
{
    public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : IdentityDbContext(options), IDataProtectionKeyContext
    {
        public DbSet<MenuCategory> MenuCategories { get; set; }
        public DbSet<MenuItem> MenuItems { get; set; }
        public DbSet<DeliveryArea> DeliveryAreas { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }
        public DbSet<Special> Specials { get; set; }
        public DbSet<RestaurantSettings> RestaurantSettings { get; set; }
        public DbSet<StoredImage> StoredImages { get; set; }
        public DbSet<OptionGroup> OptionGroups { get; set; }
        public DbSet<OptionChoice> OptionChoices { get; set; }

        // Login cookies and anti-forgery tokens are encrypted with keys that must
        // survive restarts and be shared across instances, so they live in the database
        // (a container's local disk is thrown away on every cold start).
        public DbSet<DataProtectionKey> DataProtectionKeys { get; set; }

        protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
        {
            base.ConfigureConventions(configurationBuilder);

            // The app stores South African wall-clock times (see SouthAfricaTime), not UTC,
            // so keep them in plain "timestamp" columns. Npgsql's default (timestamptz)
            // rejects non-UTC DateTime values.
            configurationBuilder.Properties<DateTime>().HaveColumnType("timestamp without time zone");
        }
    }
}
