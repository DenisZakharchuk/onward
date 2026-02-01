using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Inventorization.Auth.Domain.DbContexts;
using Inventorization.Auth.Domain.Entities;
using Inventorization.Base.Abstractions;

namespace Inventorization.Auth.Domain.DataAccess.Seeding;

/// <summary>
/// Seeds initial data for authentication system
/// </summary>
public class AuthDbSeeder
{
    private readonly AuthDbContext _context;
    private readonly ILogger<AuthDbSeeder> _logger;
    private readonly IPasswordHasher _passwordHasher;

    public AuthDbSeeder(AuthDbContext context, ILogger<AuthDbSeeder> logger, IPasswordHasher passwordHasher)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _passwordHasher = passwordHasher ?? throw new ArgumentNullException(nameof(passwordHasher));
    }

    public async Task SeedAsync()
    {
        _logger.LogInformation("Starting database seeding...");

        // Ensure database is created
        await _context.Database.MigrateAsync();

        // Seed in order: Permissions -> Roles -> RolePermissions -> Users -> UserRoles
        await SeedPermissionsAsync();
        await SeedRolesAsync();
        await SeedRolePermissionsAsync();
        await SeedUsersAsync();
        await SeedUserRolesAsync();

        _logger.LogInformation("Database seeding completed successfully.");
    }

    private async Task SeedPermissionsAsync()
    {
        if (await _context.Permissions.AnyAsync())
        {
            _logger.LogInformation("Permissions already exist, skipping...");
            return;
        }

        _logger.LogInformation("Seeding permissions...");

        var permissions = new[]
        {
            // User permissions
            new Permission("user.create", "Create Users", "user", "create"),
            new Permission("user.read", "View Users", "user", "read"),
            new Permission("user.update", "Update Users", "user", "update"),
            new Permission("user.delete", "Delete Users", "user", "delete"),
            
            // Role permissions
            new Permission("role.create", "Create Roles", "role", "create"),
            new Permission("role.read", "View Roles", "role", "read"),
            new Permission("role.update", "Update Roles", "role", "update"),
            new Permission("role.delete", "Delete Roles", "role", "delete"),
            
            // Permission permissions
            new Permission("permission.create", "Create Permissions", "permission", "create"),
            new Permission("permission.read", "View Permissions", "permission", "read"),
            new Permission("permission.update", "Update Permissions", "permission", "update"),
            new Permission("permission.delete", "Delete Permissions", "permission", "delete"),
        };

        await _context.Permissions.AddRangeAsync(permissions);
        await _context.SaveChangesAsync();

        _logger.LogInformation($"Seeded {permissions.Length} permissions.");
    }

    private async Task SeedRolesAsync()
    {
        if (await _context.Roles.AnyAsync())
        {
            _logger.LogInformation("Roles already exist, skipping...");
            return;
        }

        _logger.LogInformation("Seeding roles...");

        var roles = new[]
        {
            new Role("Admin", "System administrator with full access"),
            new Role("Manager", "Manager with user and role management access"),
            new Role("User", "Standard user with basic read access"),
            new Role("Viewer", "Read-only access to all resources"),
        };

        await _context.Roles.AddRangeAsync(roles);
        await _context.SaveChangesAsync();

        _logger.LogInformation($"Seeded {roles.Length} roles.");
    }

    private async Task SeedRolePermissionsAsync()
    {
        if (await _context.RolePermissions.AnyAsync())
        {
            _logger.LogInformation("Role permissions already exist, skipping...");
            return;
        }

        _logger.LogInformation("Seeding role permissions...");

        var adminRole = await _context.Roles.FirstAsync(r => r.Name == "Admin");
        var managerRole = await _context.Roles.FirstAsync(r => r.Name == "Manager");
        var userRole = await _context.Roles.FirstAsync(r => r.Name == "User");
        var viewerRole = await _context.Roles.FirstAsync(r => r.Name == "Viewer");

        var allPermissions = await _context.Permissions.ToListAsync();
        var readPermissions = allPermissions.Where(p => p.Action == "read").ToList();

        // Admin gets all permissions
        var adminRolePermissions = allPermissions.Select(p => new RolePermission(adminRole.Id, p.Id)).ToList();

        // Manager gets user and role management
        var managerPermissions = allPermissions
            .Where(p => p.Resource == "user" || p.Resource == "role")
            .Select(p => new RolePermission(managerRole.Id, p.Id))
            .ToList();

        // User gets read access to users
        var userPermissions = allPermissions
            .Where(p => p.Resource == "user" && p.Action == "read")
            .Select(p => new RolePermission(userRole.Id, p.Id))
            .ToList();

        // Viewer gets all read permissions
        var viewerPermissions = readPermissions
            .Select(p => new RolePermission(viewerRole.Id, p.Id))
            .ToList();

        var allRolePermissions = adminRolePermissions
            .Concat(managerPermissions)
            .Concat(userPermissions)
            .Concat(viewerPermissions)
            .ToList();

        await _context.RolePermissions.AddRangeAsync(allRolePermissions);
        await _context.SaveChangesAsync();

        _logger.LogInformation($"Seeded {allRolePermissions.Count} role permissions.");
    }

    private async Task SeedUsersAsync()
    {
        if (await _context.Users.AnyAsync())
        {
            _logger.LogInformation("Users already exist, skipping...");
            return;
        }

        _logger.LogInformation("Seeding users...");

        // Hash the admin password using the password hasher service
        const string adminPassword = "Admin123!";
        var adminPasswordHash = _passwordHasher.HashPassword(adminPassword);

        var adminUser = new User(
            "admin@inventorization.local",
            adminPasswordHash,
            "System Administrator"
        );

        await _context.Users.AddAsync(adminUser);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Seeded admin user (Email: admin@inventorization.local, Password: Admin123!)");
    }

    private async Task SeedUserRolesAsync()
    {
        if (await _context.UserRoles.AnyAsync())
        {
            _logger.LogInformation("User roles already exist, skipping...");
            return;
        }

        _logger.LogInformation("Seeding user roles...");

        var adminUser = await _context.Users.FirstAsync(u => u.Email == "admin@inventorization.local");
        var adminRole = await _context.Roles.FirstAsync(r => r.Name == "Admin");

        var userRole = new UserRole(adminUser.Id, adminRole.Id);

        await _context.UserRoles.AddAsync(userRole);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Assigned Admin role to admin user.");
    }
}
