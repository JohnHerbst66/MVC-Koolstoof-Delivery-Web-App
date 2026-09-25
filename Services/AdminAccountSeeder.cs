using System.Text;
using Koolstoof_App_1.Helpers;
using Microsoft.AspNetCore.Identity;

namespace Koolstoof_App_1.Services
{
    public static class AdminAccountSeeder
    {
        private const string AdminRole = "Admin";
        private static readonly string[] AdminIds = ["ID001", "ID002", "ID003", "ID004", "ID005"];

        public static async Task SeedAsync(IServiceProvider services)
        {
            var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = services.GetRequiredService<UserManager<IdentityUser>>();
            var configuration = services.GetRequiredService<IConfiguration>();
            var environment = services.GetRequiredService<IHostEnvironment>();
            var logger = services.GetRequiredService<ILoggerFactory>().CreateLogger("AdminAccountSeeder");

            if (!await roleManager.RoleExistsAsync(AdminRole))
            {
                await roleManager.CreateAsync(new IdentityRole(AdminRole));
            }

            var newlyCreated = new List<(string Id, string Password)>();

            foreach (var id in AdminIds)
            {
                if (await userManager.FindByNameAsync(id) != null)
                {
                    continue;
                }

                // Hosted environments (e.g. Azure) can't use the credentials file below — it
                // would land on ephemeral storage and the generated passwords would be lost,
                // locking everyone out. There, initial passwords come from configuration
                // (AdminSeed__Passwords__ID001 ... as app settings).
                var configuredPassword = configuration[$"AdminSeed:Passwords:{id}"];
                if (string.IsNullOrWhiteSpace(configuredPassword) && !environment.IsDevelopment())
                {
                    logger.LogWarning("Skipping admin account {Id}: set AdminSeed:Passwords:{Id} to create it.", id, id);
                    continue;
                }

                var generated = string.IsNullOrWhiteSpace(configuredPassword);
                var password = generated ? PasswordGenerator.GenerateStrong() : configuredPassword!;
                var user = new IdentityUser
                {
                    UserName = id,
                    Email = $"{id.ToLowerInvariant()}@koolstoof.local",
                    EmailConfirmed = true
                };

                var result = await userManager.CreateAsync(user, password);
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(user, AdminRole);
                    if (generated)
                    {
                        newlyCreated.Add((id, password));
                    }
                }
                else
                {
                    logger.LogError("Could not create admin account {Id}: {Errors}", id, string.Join(" ", result.Errors.Select(e => e.Description)));
                }
            }

            if (newlyCreated.Count > 0)
            {
                WriteCredentialsFile(newlyCreated);
            }
        }

        private static void WriteCredentialsFile(List<(string Id, string Password)> accounts)
        {
            var path = Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "admin-credentials-CONFIDENTIAL.txt");
            var sb = new StringBuilder();
            sb.AppendLine("Koolstoof Admin Accounts — generated " + DateTime.Now);
            sb.AppendLine("These were only printed once, at creation. They are not recoverable from the database afterward.");
            sb.AppendLine();
            foreach (var (id, password) in accounts)
            {
                sb.AppendLine($"{id}: {password}");
            }
            File.WriteAllText(path, sb.ToString());
        }
    }
}
