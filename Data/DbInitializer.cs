using Microsoft.AspNetCore.Identity;

namespace ShoperiaDocumentation.Data
{
    public static class DbInitializer
    {
        public static async Task Initialize(IServiceProvider serviceProvider, UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            string[] roleNames = { "Admin", "User" };
            IdentityResult roleResult;

            foreach (var roleName in roleNames)
            {
                var roleExist = await roleManager.RoleExistsAsync(roleName);
                if (!roleExist)
                {
                    roleResult = await roleManager.CreateAsync(new IdentityRole(roleName));
                }
            }

            var adminUser = new IdentityUser
            {
                UserName = "admin@admin.com",
                Email = "admin@admin.com"
            };

            string adminPassword = "Admin@123";

            var admin = await userManager.FindByEmailAsync("admin@admin.com");

            if (admin == null)
            {
                var createAdmin = await userManager.CreateAsync(adminUser, adminPassword);
                if (createAdmin.Succeeded)
                {
                    await userManager.AddToRoleAsync(adminUser, "Admin");
                }
            }

            var basicUser = new IdentityUser
            {
                UserName = "user@user.com",
                Email = "user@user.com"
            };

            string userPassword = "User@123";

            var user = await userManager.FindByEmailAsync("user@user.com");

            if (user == null)
            {
                var createUser = await userManager.CreateAsync(basicUser, userPassword);
                if (createUser.Succeeded)
                {
                    await userManager.AddToRoleAsync(basicUser, "User");
                }
            }
        }
    }
}
