using Microsoft.AspNetCore.Identity;

namespace Najda.Data;

// Creates the three roles (Donor, Hospital, Admin) if they don't exist yet,
// and one starting admin account so you can log into the admin area.
// Safe to run every startup — it only creates what's missing.
public static class IdentitySeeder
{
    // Change these if you like — this is the account you'll log in with as admin.
    private const string AdminEmail = "admin@najda.jo";
    private const string AdminPassword = "Admin#123";   // meets the default Identity rules

    public static readonly string[] Roles = { "Admin", "Hospital", "Donor" };

    public static async Task SeedAsync(IServiceProvider services)
    {
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = services.GetRequiredService<UserManager<IdentityUser>>();

        // 1) roles
        foreach (var role in Roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
                await roleManager.CreateAsync(new IdentityRole(role));
        }

        // 2) admin account
        var admin = await userManager.FindByEmailAsync(AdminEmail);
        if (admin is null)
        {
            admin = new IdentityUser
            {
                UserName = AdminEmail,
                Email = AdminEmail,
                EmailConfirmed = true
            };
            var result = await userManager.CreateAsync(admin, AdminPassword);
            if (result.Succeeded)
                await userManager.AddToRoleAsync(admin, "Admin");
        }
        else if (!await userManager.IsInRoleAsync(admin, "Admin"))
        {
            await userManager.AddToRoleAsync(admin, "Admin");
        }
    }
}