using CoreBanking.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoreBanking.Application.Identity
{
    public class RoleIdentity
    {
        public static async Task SeedAsync(UserManager<Customer> userManager, RoleManager<IdentityRole> roleManager, IOptions<AdminSettings> adminSettings)
        {
            var adminConfig = adminSettings.Value;

            string[] roles = { "Admin" };

            //  Ensure roles exist
            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                    await roleManager.CreateAsync(new IdentityRole(role));
            }

            // Create default admin user
            var admin = await userManager.FindByEmailAsync(adminConfig.Email);

            if (admin == null)
            {
                var newAdmin = new Customer
                {
                    UserName = adminConfig.UserName,
                    Email = adminConfig.Email,
                    EmailConfirmed = true
                };

                var result = await userManager.CreateAsync(newAdmin, adminConfig.Password);

                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(newAdmin, "Admin");
                }
                else
                {
                    Console.WriteLine($"Failed to create admin: {string.Join(", ", result.Errors.Select(e => e.Description))}");
                }
            }
            else
            {
                // Ensure admin has the Admin role even if user already exists
                if (!await userManager.IsInRoleAsync(admin, "Admin"))
                {
                    await userManager.AddToRoleAsync(admin, "Admin");
                }
            }
        }
    }
}
