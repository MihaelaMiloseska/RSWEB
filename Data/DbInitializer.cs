using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using RSWEB.Models;
using System.Linq;


namespace RSWEB.Data
{
    public static class DbInitializer
    {
        public static async Task SeedAsync(IServiceProvider services)
        {
            // ensure DB up-to-date
            var db = services.GetRequiredService<ApplicationDbContext>();
            await db.Database.MigrateAsync();

            var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = services.GetRequiredService<UserManager<IdentityUser>>();

            // 1) Roles
            string[] roles = { "Admin", "Professor", "Student" };
            foreach (var r in roles)
            {
                if (!await roleManager.RoleExistsAsync(r))
                    await roleManager.CreateAsync(new IdentityRole(r));
            }

            // 2) Admin user (change to yours)
            var adminEmail = "admin@rsweb.local";
            var adminPassword = "Admin123!"; // може да го смениш после

            var admin = await userManager.FindByEmailAsync(adminEmail);
            if (admin == null)
            {
                admin = new IdentityUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    EmailConfirmed = true
                };

                var createResult = await userManager.CreateAsync(admin, adminPassword);
                if (!createResult.Succeeded)
                {
                    var msg = string.Join("; ", createResult.Errors.Select(e => e.Description));
                    throw new Exception("Cannot create admin user: " + msg);
                }
            }

            // ensure Admin role
            if (!await userManager.IsInRoleAsync(admin, "Admin"))
                await userManager.AddToRoleAsync(admin, "Admin");

            // ===== Professor user =====

            // ===== Professor user =====
            var profEmail = "prof1@rsweb.local";
            var profPassword = "Prof123!";

            var profUser = await userManager.FindByEmailAsync(profEmail);

            if (profUser == null)
            {
                profUser = new IdentityUser
                {
                    UserName = profEmail,
                    Email = profEmail,
                    EmailConfirmed = true
                };

                var create = await userManager.CreateAsync(profUser, profPassword);
                if (!create.Succeeded)
                    throw new Exception("Cannot create prof: " + string.Join("; ", create.Errors.Select(e => e.Description)));
            }

            // ✅ ALWAYS ensure role
            if (!await userManager.IsInRoleAsync(profUser, "Professor"))
            {
                var addRole = await userManager.AddToRoleAsync(profUser, "Professor");
                if (!addRole.Succeeded)
                    throw new Exception("Cannot add Professor role: " + string.Join("; ", addRole.Errors.Select(e => e.Description)));
            }


            // ===== Student user =====
            var studEmail = "stud1@rsweb.local";
            var studPassword = "Stud123!";

            var studUser = await userManager.FindByEmailAsync(studEmail);
            if (studUser == null)
            {
                studUser = new IdentityUser
                {
                    UserName = studEmail,
                    Email = studEmail,
                    EmailConfirmed = true
                };

                var result = await userManager.CreateAsync(studUser, studPassword);
                if (result.Succeeded)
                    await userManager.AddToRoleAsync(studUser, "Student");
            }
            else
            {
                if (!await userManager.IsInRoleAsync(studUser, "Student"))
                    await userManager.AddToRoleAsync(studUser, "Student");
            }

            

            // Teacher record
            if (!await db.Teachers.AnyAsync(t => t.Email == profEmail))
            {
                db.Teachers.Add(new Teacher
                {
                    // пополни минимум полиња што ти се required
                    // пример:
                    FirstName = "Prof",
                    LastName = "One",
                    Email = profEmail
                });
            }

            // Student record
            if (!await db.Students.AnyAsync(s => s.Email == studEmail))
            {
                db.Students.Add(new Student
                {
                    // пополни минимум полиња што ти се required
                    // пример:
                    FirstName = "Stud",
                    LastName = "One",
                    StudentId = "123/2025",
                    Email = studEmail
                });
            }

            await db.SaveChangesAsync();


        }

    }
}
