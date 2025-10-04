using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using examen_parcial.Data;
using examen_parcial.Models;

namespace examen_parcial.Services
{
    public static class DatabaseSeeder
    {
        public static async Task SeedAsync(IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();
            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

            // Asegurar que la base de datos esté creada
            await context.Database.EnsureCreatedAsync();

            // Crear roles
            if (!await roleManager.RoleExistsAsync("Coordinador"))
            {
                await roleManager.CreateAsync(new IdentityRole("Coordinador"));
            }

            if (!await roleManager.RoleExistsAsync("Estudiante"))
            {
                await roleManager.CreateAsync(new IdentityRole("Estudiante"));
            }

            // Crear usuario coordinador
            var coordinadorEmail = "admin@gmail.com";
            var coordinador = await userManager.FindByEmailAsync(coordinadorEmail);
            
            if (coordinador == null)
            {
                coordinador = new IdentityUser
                {
                    UserName = coordinadorEmail,
                    Email = coordinadorEmail,
                    EmailConfirmed = true
                };

                var result = await userManager.CreateAsync(coordinador, "Admin2024!");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(coordinador, "Coordinador");
                }
            }

            // Crear cursos iniciales si no existen
            if (!await context.Cursos.AnyAsync())
            {
                var cursos = new List<Curso>
                {
                    new Curso
                    {
                        Codigo = "CS101",
                        Nombre = "Introducción a la Programación",
                        Creditos = 4,
                        CupoMaximo = 30,
                        HorarioInicio = new TimeSpan(8, 0, 0), // 8:00 AM
                        HorarioFin = new TimeSpan(10, 0, 0),   // 10:00 AM
                        Activo = true
                    },
                    new Curso
                    {
                        Codigo = "CS102",
                        Nombre = "Estructura de Datos",
                        Creditos = 4,
                        CupoMaximo = 25,
                        HorarioInicio = new TimeSpan(10, 30, 0), // 10:30 AM
                        HorarioFin = new TimeSpan(12, 30, 0),    // 12:30 PM
                        Activo = true
                    },
                    new Curso
                    {
                        Codigo = "CS201",
                        Nombre = "Base de Datos",
                        Creditos = 3,
                        CupoMaximo = 20,
                        HorarioInicio = new TimeSpan(14, 0, 0), // 2:00 PM
                        HorarioFin = new TimeSpan(16, 0, 0),    // 4:00 PM
                        Activo = true
                    }
                };

                context.Cursos.AddRange(cursos);
                await context.SaveChangesAsync();
            }
        }
    }
}