using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using examen_parcial.Data;

namespace examen_parcial.Controllers
{
    public class DiagnosticoController : Controller
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly ApplicationDbContext _context;

        public DiagnosticoController(UserManager<IdentityUser> userManager, ApplicationDbContext context)
        {
            _userManager = userManager;
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var diagnostico = new
            {
                DatabaseExists = await _context.Database.CanConnectAsync(),
                UsersCount = await _context.Users.CountAsync(),
                RolesCount = await _context.Roles.CountAsync(),
                CursosCount = await _context.Cursos.CountAsync(),
                Users = await _context.Users.Select(u => new { 
                    u.Email, 
                    u.UserName, 
                    u.EmailConfirmed 
                }).ToListAsync(),
                Roles = await _context.Roles.Select(r => r.Name).ToListAsync()
            };

            ViewBag.Diagnostico = diagnostico;

            // Verificar usuarios específicos
            var adminUser = await _userManager.FindByEmailAsync("admin@gmail.com");
            var testUser = await _userManager.FindByEmailAsync("test@test.com");
            
            ViewBag.AdminExists = adminUser != null;
            ViewBag.TestExists = testUser != null;
            
            if (adminUser != null)
            {
                ViewBag.AdminPasswordValid = await _userManager.CheckPasswordAsync(adminUser, "Admin2024!");
                ViewBag.AdminRoles = await _userManager.GetRolesAsync(adminUser);
            }
            
            if (testUser != null)
            {
                ViewBag.TestPasswordValid = await _userManager.CheckPasswordAsync(testUser, "1234");
                ViewBag.TestRoles = await _userManager.GetRolesAsync(testUser);
            }

            return View();
        }

        public async Task<IActionResult> TestLogin(string email, string password)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                return Json(new { success = false, message = "Usuario no encontrado" });
            }

            var passwordValid = await _userManager.CheckPasswordAsync(user, password);
            if (!passwordValid)
            {
                return Json(new { success = false, message = "Contraseña incorrecta" });
            }

            return Json(new { success = true, message = "Credenciales válidas", user = user.Email });
        }
    }
}