using Microsoft.AspNetCore.Identity;

namespace ComputerStore.Data
{
    public class ContextSeed
    {
        public static async Task SeedRolesAsync(UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            //Seed Roles
            await roleManager.CreateAsync(new IdentityRole(Enums.Roles.Manager.ToString()));
            await roleManager.CreateAsync(new IdentityRole(Enums.Roles.Seller.ToString()));
            await roleManager.CreateAsync(new IdentityRole(Enums.Roles.User.ToString()));
        }
    }
}
