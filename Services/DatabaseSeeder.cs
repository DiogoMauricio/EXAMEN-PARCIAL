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
                Console.WriteLine("Rol Coordinador creado");
            }

            if (!await roleManager.RoleExistsAsync("Estudiante"))
            {
                await roleManager.CreateAsync(new IdentityRole("Estudiante"));
                Console.WriteLine("Rol Estudiante creado");
            }

            // Usuario 1: Coordinador simple
            var coord = await userManager.FindByEmailAsync("admin@test.com");
            if (coord == null)
            {
                coord = new IdentityUser
                {
                    UserName = "admin@test.com",
                    Email = "admin@test.com",
                    EmailConfirmed = true,
                    NormalizedEmail = "ADMIN@TEST.COM",
                    NormalizedUserName = "ADMIN@TEST.COM"
                };

                var result = await userManager.CreateAsync(coord, "123456");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(coord, "Coordinador");
                    Console.WriteLine("✅ Coordinador creado: admin@test.com / 123456");
                }
                else
                {
                    Console.WriteLine("❌ Error creando coordinador:");
                    foreach (var error in result.Errors)
                    {
                        Console.WriteLine($"   {error.Description}");
                    }
                }
            }

            // Usuario 2: Estudiante simple
            var student = await userManager.FindByEmailAsync("student@test.com");
            if (student == null)
            {
                student = new IdentityUser
                {
                    UserName = "student@test.com",
                    Email = "student@test.com",
                    EmailConfirmed = true,
                    NormalizedEmail = "STUDENT@TEST.COM",
                    NormalizedUserName = "STUDENT@TEST.COM"
                };

                var result = await userManager.CreateAsync(student, "123456");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(student, "Estudiante");
                    Console.WriteLine("✅ Estudiante creado: student@test.com / 123456");
                }
                else
                {
                    Console.WriteLine("❌ Error creando estudiante:");
                    foreach (var error in result.Errors)
                    {
                        Console.WriteLine($"   {error.Description}");
                    }
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
                        HorarioInicio = new TimeSpan(8, 0, 0),
                        HorarioFin = new TimeSpan(10, 0, 0),
                        Activo = true
                    },
                    new Curso
                    {
                        Codigo = "CS102",
                        Nombre = "Estructura de Datos",
                        Creditos = 4,
                        CupoMaximo = 25,
                        HorarioInicio = new TimeSpan(10, 30, 0),
                        HorarioFin = new TimeSpan(12, 30, 0),
                        Activo = true
                    },
                    new Curso
                    {
                        Codigo = "CS201",
                        Nombre = "Base de Datos",
                        Creditos = 3,
                        CupoMaximo = 20,
                        HorarioInicio = new TimeSpan(14, 0, 0),
                        HorarioFin = new TimeSpan(16, 0, 0),
                        Activo = true
                    }
                };

                context.Cursos.AddRange(cursos);
                await context.SaveChangesAsync();
                Console.WriteLine("✅ Cursos creados");
            }

            Console.WriteLine("\n🎯 CREDENCIALES DE ACCESO:");
            Console.WriteLine("👑 Coordinador: admin@test.com / 123456");
            Console.WriteLine("👤 Estudiante: student@test.com / 123456");
        }
    }
}