using Microsoft.AspNetCore.Identity;
using Server.Models;

namespace Server.Data;

public class SeedData
{
    public static async Task Initialize(IServiceProvider serviceProvider)
    {
        var userManager = (UserManager<User>)serviceProvider.GetService(typeof(UserManager<User>))!;
        var roleManager = (RoleManager<IdentityRole>)serviceProvider.GetService(typeof(RoleManager<IdentityRole>))!;

        //Seed Roles
         foreach (var role in Enum.GetValues(typeof(Constants.Identity.Roles)))
        {
            await roleManager.CreateAsync(new IdentityRole(role.ToString()));
        }
        
        //Seed Default User
        var defaultUser = new User
        {
            Email = Constants.Identity.DefaultEmail,
            EmailConfirmed = true
        };
        
        if (userManager.Users.All(u => u.Id != defaultUser.Id))
        {
            await userManager.CreateAsync(defaultUser, Constants.Identity.DefaultPassword);
            await userManager.AddToRoleAsync(defaultUser, Constants.Identity.DefaultRole.ToString());
        }
    }
}
