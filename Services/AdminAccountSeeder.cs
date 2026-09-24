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

                var password = PasswordGenerator.GenerateStrong();
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
                    newlyCreated.Add((id, password));
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
