using Microsoft.AspNetCore.Identity;
using Teretana.Models;

namespace Sustavzapracenjenapretkauteretani.DAL
{
    /// <summary>
    /// Inicijalizira osnovne uloge i admin korisnika u bazi podataka.
    /// </summary>
    public static class RoleSeeder
    {
        public static async Task SeedRolesAsync(RoleManager<IdentityRole> roleManager, UserManager<AppUser> userManager)
        {
            // Kreiraj uloge ako ne postoje
            string[] roleNames = { "Admin", "Trainer", "User" };
            
            foreach (var roleName in roleNames)
            {
                var roleExist = await roleManager.RoleExistsAsync(roleName);
                if (!roleExist)
                {
                    await roleManager.CreateAsync(new IdentityRole { Name = roleName });
                }
            }

            // Kreiraj admin korisnika ako ne postoji
            var adminUser = await userManager.FindByEmailAsync("admin@teretana.hr");
            if (adminUser == null)
            {
                var newAdmin = new AppUser
                {
                    UserName = "admin@teretana.hr",
                    Email = "admin@teretana.hr",
                    OIB = "12345678901",
                    JMBG = "1234567890123",
                    EmailConfirmed = true
                };

                var createAdminResult = await userManager.CreateAsync(newAdmin, "Admin@123456");
                if (createAdminResult.Succeeded)
                {
                    await userManager.AddToRoleAsync(newAdmin, "Admin");
                }
            }

            // Kreiraj test trainer korisnika ako ne postoji
            var trainerUser = await userManager.FindByEmailAsync("trainer@teretana.hr");
            if (trainerUser == null)
            {
                var newTrainer = new AppUser
                {
                    UserName = "trainer@teretana.hr",
                    Email = "trainer@teretana.hr",
                    OIB = "11111111111",
                    JMBG = "1111111111111",
                    EmailConfirmed = true
                };

                var createTrainerResult = await userManager.CreateAsync(newTrainer, "Trainer@123456");
                if (createTrainerResult.Succeeded)
                {
                    await userManager.AddToRoleAsync(newTrainer, "Trainer");
                }
            }
        }
    }
}
