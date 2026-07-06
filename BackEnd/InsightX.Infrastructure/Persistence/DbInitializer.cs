using InsightX.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace InsightX.Infrastructure.Persistence
{
    public static class DbInitializer
    {
        public static async Task SeedAsync(IServiceProvider serviceProvider, IConfiguration configuration)
        {
            var logger = serviceProvider.GetRequiredService<ILogger<AppDbContext>>();

            try
            {
                var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
                var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();
                var context = serviceProvider.GetRequiredService<AppDbContext>();

                foreach (var role in new[] { "Owner", "Manager", "SuperAdmin" })
                {
                    if (!await roleManager.RoleExistsAsync(role))
                    {
                        await roleManager.CreateAsync(new IdentityRole(role));
                        logger.LogInformation("Created role: {Role}", role);
                    }
                }

                var adminCompany = await context.Companies.FirstOrDefaultAsync(c => c.Name == "InsightX System");
                if (adminCompany == null)
                {
                    adminCompany = new Company
                    {
                        Name = "InsightX System",
                        CreatedAt = DateTime.UtcNow
                    };
                    context.Companies.Add(adminCompany);
                    await context.SaveChangesAsync();
                    logger.LogInformation("Created system company: InsightX System");
                }

                var adminEmail = configuration["SuperAdmin:Email"];
                var adminPassword = configuration["SuperAdmin:Password"];

                if (string.IsNullOrEmpty(adminEmail) || string.IsNullOrEmpty(adminPassword))
                {
                    throw new InvalidOperationException("SuperAdmin:Email and SuperAdmin:Password configuration is missing.");
                }

                var adminUser = await userManager.FindByEmailAsync(adminEmail);
                if (adminUser == null)
                {
                    adminUser = new ApplicationUser
                    {
                        UserName = adminEmail,
                        Email = adminEmail,
                        Name = "Super Admin",
                        CompanyId = adminCompany.Id,
                        IsActivated = true
                    };

                    var createResult = await userManager.CreateAsync(adminUser, adminPassword);
                    if (createResult.Succeeded)
                    {
                        await userManager.AddToRoleAsync(adminUser, "SuperAdmin");
                        logger.LogInformation("Created Super Admin user: {Email}", adminEmail);
                    }
                    else
                    {
                        var errors = string.Join(", ", createResult.Errors.Select(e => e.Description));
                        logger.LogError("Failed to create Super Admin: {Errors}", errors);
                    }
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred while seeding the database");
                throw;
            }
        }
    }
}
