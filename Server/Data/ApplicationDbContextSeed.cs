using Microsoft.AspNetCore.Identity;
using Server.Constants;
using Server.Models;

namespace Server.Data;

public class ApplicationDbContextSeed
{
    public static async Task SeedEssentialsAsync(UserManager<User> userManager,
        RoleManager<IdentityRole> roleManager)
    {
        //Seed Roles
        await roleManager.CreateAsync(new IdentityRole(Authorization.Roles.Admin.ToString()));
        await roleManager.CreateAsync(new IdentityRole(Authorization.Roles.User.ToString()));
        
        //Seed Default User
        var defaultUser = new User
        {
            UserName = Authorization.DefaultUsername,
            Email = Authorization.DefaultEmail,
            EmailConfirmed = true,
            PhoneNumberConfirmed = true
        };
        
        if (userManager.Users.All(u => u.Id != defaultUser.Id))
        {
            await userManager.CreateAsync(defaultUser, Authorization.DefaultPassword);
            await userManager.AddToRoleAsync(defaultUser, Authorization.DefaultRole.ToString());
        }
    }
}