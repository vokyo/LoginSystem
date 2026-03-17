using CheckInSystem.Application.Common.Helpers;
using CheckInSystem.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CheckInSystem.Infrastructure.Data;

public static class ApplicationDbContextSeed
{
    public static async Task SeedAsync(ApplicationDbContext context)
    {
        await context.Database.MigrateAsync();

        if (!await context.Permissions.AnyAsync())
        {
            var permissions = PermissionCodes.All
                .Select(x => new Permission
                {
                    Name = x.Name,
                    Code = x.Code,
                    Description = x.Description
                })
                .ToList();

            await context.Permissions.AddRangeAsync(permissions);
            await context.SaveChangesAsync();
        }

        if (!await context.Roles.AnyAsync())
        {
            var adminRole = new Role
            {
                Name = "Administrator",
                Description = "Default system administrator role.",
                IsActive = true,
                IsSystem = true
            };

            var attendeeRole = new Role
            {
                Name = "Attendee",
                Description = "Default check-in user role.",
                IsActive = true
            };

            var permissions = await context.Permissions.ToListAsync();
            adminRole.RolePermissions = permissions
                .Select(x => new RolePermission
                {
                    Role = adminRole,
                    Permission = x
                })
                .ToList();

            await context.Roles.AddRangeAsync(adminRole, attendeeRole);
            await context.SaveChangesAsync();
        }

        if (!await context.Users.AnyAsync())
        {
            var adminRole = await context.Roles.SingleAsync(x => x.Name == "Administrator");
            var admin = new User
            {
                UserName = "admin",
                FullName = "System Administrator",
                Email = "admin@checkinsystem.local",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin123!"),
                IsActive = true
            };

            admin.UserRoles.Add(new UserRole
            {
                User = admin,
                RoleId = adminRole.Id
            });

            await context.Users.AddAsync(admin);
            await context.SaveChangesAsync();
        }
    }
}
