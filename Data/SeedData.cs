using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using ClassManagement.Models;
using System;
using System.Threading.Tasks;

namespace ClassManagement.Data
{
    public static class SeedData
    {
        public static async Task Initialize(IServiceProvider serviceProvider, UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            // Admin rolünü oluşturuyoruz
            var roleExist = await roleManager.RoleExistsAsync("Admin");
            if (!roleExist)
            {
                var role = new IdentityRole("Admin");
                await roleManager.CreateAsync(role);
            }

            // Admin kullanıcısını oluşturuyoruz
            var user = await userManager.FindByEmailAsync("admin@example.com");
            if (user == null)
            {
                user = new ApplicationUser
                {
                    UserName = "admin@example.com",
                    Email = "admin@example.com",
                    FirstName = "Admin",
                    LastName = "User",
                    EmailConfirmed = true
                };

                // Güçlü bir şifre kullanın
                var result = await userManager.CreateAsync(user, "AdminPass1!"); // Güçlü şifre
                if (result.Succeeded)
                {
                    // Admin rolüne atama
                    await userManager.AddToRoleAsync(user, "Admin");
                }
            }
        }
    }
}
