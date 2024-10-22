using Microsoft.AspNetCore.Identity;
using TP_PWEB.Models;

namespace TP_PWEB.Data
{
    public enum Roles
    {
        Client,
        Worker,
        Manager,
        Administrator
    }

    public static class inicialization
    {
        public static async Task CriaDadosIniciais(UserManager<ApplicationUser> userManager, 
                                                    RoleManager<IdentityRole> roleManager)
        {
            //Adicionar default Roles
            await roleManager.CreateAsync(new IdentityRole(Roles.Client.ToString()));
            await roleManager.CreateAsync(new IdentityRole(Roles.Worker.ToString()));
            await roleManager.CreateAsync(new IdentityRole(Roles.Manager.ToString()));
            await roleManager.CreateAsync(new IdentityRole(Roles.Administrator.ToString()));

            //Adicionar Default User - Administrador
            var defaultUser = new ApplicationUser
            {
                UserName = "admin@localhost.com",
                Email = "admin@localhost.com",
                FirstName = "Administrador",
                LastName = "Local",
                EmailConfirmed = true,
                PhoneNumberConfirmed = true
            };

            var user = await userManager.FindByEmailAsync(defaultUser.Email);
            if (user == null)
            {
                await userManager.CreateAsync(defaultUser, "Erasmus_123");
                await userManager.AddToRoleAsync(defaultUser, Roles.Administrator.ToString());
            }
        }
    }

}
