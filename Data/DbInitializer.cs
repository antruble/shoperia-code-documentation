using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ShoperiaDocumentation.Models;

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

            // Folder seeding logic
            using (var context = new ApplicationDbContext(serviceProvider.GetRequiredService<DbContextOptions<ApplicationDbContext>>()))
            {
                // Ensure the database is created
                context.Database.EnsureCreated();

                // Look for any folders.
                if (context.Folders.Any())
                {
                    return;   // DB has been seeded
                }

                // Seed folders
                var rootFolders = new FolderModel[]
                {
                    new FolderModel { Name = "Libraries", ParentId = null, Level = 1 },
                    new FolderModel { Name = "Presentation", ParentId = null, Level = 1 },
                    new FolderModel { Name = "Plugins", ParentId = null, Level = 1 }
                };

                foreach (FolderModel f in rootFolders)
                {
                    context.Folders.Add(f);
                }
                try
                {
                    await context.SaveChangesAsync();

                }
                catch (Exception ex)
                {

                    throw new Exception($"{ex.Message}");
                }

                var subFolders = new FolderModel[]
                {
                    // LIBRARIES
                    new FolderModel { Name = "Nop.Data", ParentId = rootFolders[0].Id, Level = 2 },
                    new FolderModel { Name = "Nop.Services", ParentId = rootFolders[0].Id, Level = 2 },
                    new FolderModel { Name = "Nop.Core", ParentId = rootFolders[0].Id, Level = 2 },

                    //PRESENTATION
                    new FolderModel { Name = "Nop.Web", ParentId = rootFolders[1].Id, Level = 2 },
                    new FolderModel { Name = "Nop.Web.Framework", ParentId = rootFolders[1].Id, Level = 2 }
                };

                foreach (FolderModel sf in subFolders)
                {
                    context.Folders.Add(sf);
                }
                try
                {
                    await context.SaveChangesAsync();

                }
                catch (Exception ex)
                {

                    throw new Exception($"{ex.Message}");
                }

                // Add more nested folders as needed
                var nestedFolders = new FolderModel[]
                {
                    new FolderModel { Name = "Folder1", ParentId = subFolders[0].Id, Level = 3 },
                    new FolderModel { Name = "Folder2", ParentId = subFolders[0].Id, Level = 3 },
                    new FolderModel { Name = "Folder3", ParentId = subFolders[1].Id, Level = 3 },
                    new FolderModel { Name = "Folder4", ParentId = subFolders[1].Id, Level = 3 },
                    new FolderModel { Name = "Folder5", ParentId = subFolders[1].Id, Level = 3 },
                    new FolderModel { Name = "Folder6", ParentId = subFolders[1].Id, Level = 3 },
                    new FolderModel { Name = "Folder7", ParentId = subFolders[1].Id, Level = 3 },
                    new FolderModel { Name = "Folder8", ParentId = subFolders[1].Id, Level = 3 },
                    new FolderModel { Name = "Folder9", ParentId = subFolders[1].Id, Level = 3 },
                    new FolderModel { Name = "Folder10", ParentId = subFolders[1].Id, Level = 3 },
                    new FolderModel { Name = "Folder11", ParentId = subFolders[2].Id, Level = 3 },
                    new FolderModel { Name = "Folder12", ParentId = subFolders[2].Id, Level = 3 },
                    new FolderModel { Name = "Folder13", ParentId = subFolders[3].Id, Level = 3 },
                    new FolderModel { Name = "Folder14", ParentId = subFolders[3].Id, Level = 3 },
                    new FolderModel { Name = "Folder15", ParentId = subFolders[4].Id, Level = 3 },
                    new FolderModel { Name = "Folder16", ParentId = subFolders[4].Id, Level = 3 },
                    new FolderModel { Name = "Folder17", ParentId = subFolders[4].Id, Level = 3 },
                };

                foreach (FolderModel nf in nestedFolders)
                {
                    context.Folders.Add(nf);
                }
                try
                {
                    await context.SaveChangesAsync();

                }
                catch (Exception ex)
                {

                    throw new Exception($"{ex.Message}");
                }
                var files = new FileModel[]
                {
                    new FileModel { Name = "File1", ParentId = nestedFolders[0].Id },
                    new FileModel { Name = "File2", ParentId = nestedFolders[0].Id },
                    new FileModel { Name = "File3", ParentId = nestedFolders[1].Id },
                    new FileModel { Name = "File4", ParentId = nestedFolders[1].Id },
                    new FileModel { Name = "File5", ParentId = nestedFolders[1].Id },
                    new FileModel { Name = "File6", ParentId = nestedFolders[1].Id },
                    new FileModel { Name = "File7", ParentId = nestedFolders[1].Id },
                    new FileModel { Name = "File8", ParentId = nestedFolders[1].Id },
                    new FileModel { Name = "File9", ParentId = nestedFolders[1].Id },
                    new FileModel { Name = "File10", ParentId = nestedFolders[1].Id },
                    new FileModel { Name = "File11", ParentId = nestedFolders[2].Id },
                    new FileModel { Name = "File12", ParentId = nestedFolders[2].Id },
                    new FileModel { Name = "File13", ParentId = nestedFolders[3].Id },
                    new FileModel { Name = "File14", ParentId = nestedFolders[3].Id },
                    new FileModel { Name = "File15", ParentId = nestedFolders[4].Id },
                    new FileModel { Name = "File16", ParentId = nestedFolders[4].Id },
                    new FileModel { Name = "File17", ParentId = nestedFolders[4].Id },
                };
            }
        }
    }
}
