using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using ShoperiaDocumentation.Data;
using ShoperiaDocumentation.Services;
using System.Text;

namespace ShoperiaDocumentation
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Configure Serilog
            Log.Logger = new LoggerConfiguration()
                .ReadFrom.Configuration(builder.Configuration)
                .Enrich.FromLogContext()
                .WriteTo.Console()
                .WriteTo.File("Logs/app_log-.txt", rollingInterval: RollingInterval.Day)
                .CreateLogger();

            builder.Host.UseSerilog();

            // Add services to the container.
            var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
            builder.Services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(connectionString));
            builder.Services.AddDatabaseDeveloperPageExceptionFilter();

            builder.Services.AddDefaultIdentity<IdentityUser>(options =>
            {
                options.SignIn.RequireConfirmedAccount = false;

                // Disable password complexity requirements for easier development
                options.Password.RequireDigit = false;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequireLowercase = false;
                options.Password.RequireUppercase = false;
                options.Password.RequiredLength = 5; // Minimum jelszóhossz
            })
            .AddRoles<IdentityRole>()
            .AddEntityFrameworkStores<ApplicationDbContext>();
            builder.Services.AddControllersWithViews();
            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowAll", builder =>
                {
                    builder.AllowAnyOrigin()
                           .AllowAnyMethod()
                           .AllowAnyHeader();
                });
            });

            // Register the FileService
            builder.Services.AddScoped<IFileService, FileService>();
            builder.Services.AddScoped<IFileProcessingService, FileProcessingService>();
            // JWT authentication configuration
            var key = Encoding.ASCII.GetBytes(builder.Configuration["Jwt:Key"]);
            builder.Services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = IdentityConstants.ApplicationScheme;  // Cookie hitelesítés alapértelmezetten
                options.DefaultChallengeScheme = IdentityConstants.ApplicationScheme;      // Cookie hitelesítés a kihívásokhoz (pl. bejelentkezés)
                options.DefaultScheme = IdentityConstants.ApplicationScheme;               // Alapértelmezett hitelesítési séma
            })
            .AddCookie(options =>
            {
                options.LoginPath = "/Identity/Account/Login";  // Cookie beállítások
                options.LogoutPath = "/Identity/Account/Logout";
                options.AccessDeniedPath = "/Identity/Account/AccessDenied";
            })
            .AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = builder.Configuration["Jwt:Issuer"],
                    ValidAudience = builder.Configuration["Jwt:Audience"],
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]))
                };
            });

            builder.Services.AddAuthorization(options =>
            {
                options.AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"));
            });

            var app = builder.Build();

            using (var scope = app.Services.CreateScope())
            {
                var services = scope.ServiceProvider;
                try
                {
                    var userManager = services.GetRequiredService<UserManager<IdentityUser>>();
                    var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();  // Ensure RoleManager is resolved
                    var logger = services.GetRequiredService<ILogger<Program>>();  // Get the logger for DbInitializer
                    DbInitializer.Initialize(services, userManager, roleManager, logger).Wait();
                }
                catch (Exception ex)
                {
                    // Log the exception
                    var logger = services.GetRequiredService<ILogger<Program>>();
                    logger.LogError(ex, "An error occurred while seeding the database.");
                }
            }

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseMigrationsEndPoint();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }
            app.UseCors("AllowAllOrigins");
            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            

            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllerRoute(
                name: "classtree-getfilecontent",
                pattern: "ClassTree/GetFileContent/{fileId?}",
                defaults: new { controller = "ClassTree", action = "GetFileContent" });

            app.MapControllerRoute(
                name: "classtree-deletefolderorfile",
                pattern: "ClassTree/DeleteFolderOrFile",
                defaults: new { controller = "ClassTree", action = "DeleteFolderOrFile" });
            
            app.MapControllerRoute(
                name: "classtree-createfolderorfile",
                pattern: "ClassTree/CreateFolderOrFile",
                defaults: new { controller = "ClassTree", action = "CreateFolderOrFile" });
            
            app.MapControllerRoute(
                name: "classtree-renamefolderorfile",
                pattern: "ClassTree/RenameFolderOrFile",
                defaults: new { controller = "ClassTree", action = "RenameFolderOrFile" });
            
            app.MapControllerRoute(
                name: "classtree-createmethod",
                pattern: "ClassTree/CreateMethod",
                defaults: new { controller = "ClassTree", action = "CreateMethod" });
            
            app.MapControllerRoute(
                name: "classtree-editmethod",
                pattern: "ClassTree/EditMethod/{id}",
                defaults: new { controller = "ClassTree", action = "EditMethod" });
            
            app.MapControllerRoute(
                name: "classtree-deletemethod",
                pattern: "ClassTree/DeleteMethod/{id}",
                defaults: new { controller = "ClassTree", action = "DeleteMethod" });

            app.MapControllerRoute(
                name: "classtree-get-method-code",
                pattern: "ClassTree/GetMethodCode/{id}",
                defaults: new { controller = "ClassTree", action = "GetMethodCode" });

            //API
            app.MapControllerRoute(
                name: "api-jsonupload",
                pattern: "api/JsonUpload/Upload",
                new { controller = "JsonUpload", action = "UploadJsonFile" });

            app.MapControllerRoute(
                name: "api-checkfileexists",
                pattern: "api/Api/CheckFileExists",
                defaults: new { controller = "Api", action = "CheckFileExists" });

            app.MapControllerRoute(
                name: "api-processjsondata",
                pattern: "api/Api/ProcessJsonData",
                defaults: new { controller = "Api", action = "ProcessJsonData" });
            //  -   FIELD EDIT
            app.MapControllerRoute(
                name: "api-update-field",
                pattern: "api/UpdateField",
                defaults: new { controller = "Api", action = "UpdateField" });

            // DATABASE
            app.MapControllerRoute(
                name: "database-get-entity",
                pattern: "/database/entities/{entityId}",
                defaults: new { controller = "Database", action = "GetEntityDetails" });

            // default routing
            app.MapControllerRoute(
                name: "classtree",
                pattern: "ClassTree/{*path}",
                defaults: new { controller = "ClassTree", action = "Index" });

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");
            //        app.MapControllerRoute(
            //name: "default",
            //pattern: "{controller=ClassTree}/{action=Index}/{path?}");



            app.MapRazorPages();
            Log.Information("Application has started successfully.");
            app.Run();
        }
    }
}
