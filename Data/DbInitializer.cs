using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ShoperiaDocumentation.Models;
using Microsoft.Extensions.Logging;

namespace ShoperiaDocumentation.Data
{
    public static class DbInitializer
    {
        public static async Task Initialize(IServiceProvider serviceProvider, UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager, ILogger logger)
        {
            logger.LogInformation($"Db initializer started..");
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

            string adminPassword = "admin";

            var admin = await userManager.FindByEmailAsync("admin@admin.com");

            if (admin == null)
            {
                var createAdmin = await userManager.CreateAsync(adminUser, adminPassword);
                if (createAdmin.Succeeded)
                {
                    await userManager.AddToRoleAsync(adminUser, "Admin");
                    logger.LogInformation("Admin user created successfully.");
                }
                else
                {
                    logger.LogError("Failed to create admin user. Errors: {Errors}", string.Join(", ", createAdmin.Errors.Select(e => e.Description)));
                }
            }
            logger.LogInformation($"lefut1..");
            var basicUser = new IdentityUser
            {
                UserName = "user@user.com",
                Email = "user@user.com"
            };

            string userPassword = "user";

            var user = await userManager.FindByEmailAsync("user@user.com");

            if (user == null)
            {
                var createUser = await userManager.CreateAsync(basicUser, userPassword);
                if (createUser.Succeeded)
                {
                    await userManager.AddToRoleAsync(basicUser, "User");
                }
            }
            logger.LogInformation($"lefut2..");
            // Folder seeding logic
            using (var context = new ApplicationDbContext(serviceProvider.GetRequiredService<DbContextOptions<ApplicationDbContext>>()))
            {
                logger.LogInformation($"lefut3..");
                // Ensure the database is created
                context.Database.Migrate();
                logger.LogInformation($"lefut4..");
                // Look for any folders.
                if (context.Folders.Any())
                {
                    return;   // DB has been seeded
                }

                // Seed folders
                var rootFolders = new FolderModel[]
                {
                    new FolderModel { Name = "Libraries", ParentId = null, Status = "edited"},
                    new FolderModel { Name = "Presentation", ParentId = null, Status = "edited" },
                    new FolderModel { Name = "Plugins", ParentId = null, Status = "edited" }
                };

                context.Folders.AddRange(rootFolders);

                try
                {
                    await context.SaveChangesAsync();
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "An error occurred while seeding the root folders.");
                    throw;
                }

                var subFolders = new FolderModel[]
                {
                    // LIBRARIES
                    new FolderModel { Name = "Nop.Data", ParentId = rootFolders[0].Id, Status = "edited"},
                    new FolderModel { Name = "Nop.Services", ParentId = rootFolders[0].Id , Status = "edited"},
                    new FolderModel { Name = "Nop.Core", ParentId = rootFolders[0].Id , Status = "edited"},

                    //PRESENTATION
                    new FolderModel { Name = "Nop.Web", ParentId = rootFolders[1].Id , Status = "edited"},
                    new FolderModel { Name = "Nop.Web.Framework", ParentId = rootFolders[1].Id , Status = "edited"}
                };

                context.Folders.AddRange(subFolders);
                logger.LogInformation($"lefut5..");
                try
                {
                    await context.SaveChangesAsync();
                    logger.LogInformation($"lefut6..");
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "An error occurred while seeding the sub folders.");
                    throw;
                }

                
            }
        }
    }
}
