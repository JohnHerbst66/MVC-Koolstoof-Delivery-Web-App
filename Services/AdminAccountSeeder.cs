using System.Security.Cryptography;
using System.Text;
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

                var password = GenerateStrongPassword();
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

        private static string GenerateStrongPassword()
        {
            const string upper = "ABCDEFGHJKLMNPQRSTUVWXYZ";
            const string lower = "abcdefghijkmnopqrstuvwxyz";
            const string digits = "23456789";
            const string special = "!@#$%^&*";
            const string all = upper + lower + digits + special;

            var chars = new char[14];
            chars[0] = upper[RandomNumberGenerator.GetInt32(upper.Length)];
            chars[1] = lower[RandomNumberGenerator.GetInt32(lower.Length)];
            chars[2] = digits[RandomNumberGenerator.GetInt32(digits.Length)];
            chars[3] = special[RandomNumberGenerator.GetInt32(special.Length)];

            for (int i = 4; i < chars.Length; i++)
            {
                chars[i] = all[RandomNumberGenerator.GetInt32(all.Length)];
            }

            // Shuffle so the guaranteed character classes aren't always in the same positions.
            for (int i = chars.Length - 1; i > 0; i--)
            {
                int j = RandomNumberGenerator.GetInt32(i + 1);
                (chars[i], chars[j]) = (chars[j], chars[i]);
            }

            return new string(chars);
        }
    }
}
