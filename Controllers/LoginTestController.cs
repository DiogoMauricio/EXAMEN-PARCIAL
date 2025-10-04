using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;

namespace examen_parcial.Controllers
{
    public class LoginTestController : Controller
    {
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly UserManager<IdentityUser> _userManager;

        public LoginTestController(SignInManager<IdentityUser> signInManager, UserManager<IdentityUser> userManager)
        {
            _signInManager = signInManager;
            _userManager = userManager;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(string email, string password)
        {
            try
            {
                // Verificar que el usuario existe
                var user = await _userManager.FindByEmailAsync(email);
                if (user == null)
                {
                    ViewBag.Error = $"Usuario con email {email} no encontrado.";
                    return View("Index");
                }

                // Verificar la contraseña
                var passwordValid = await _userManager.CheckPasswordAsync(user, password);
                if (!passwordValid)
                {
                    ViewBag.Error = "Contraseña incorrecta.";
                    return View("Index");
                }

                // Intentar login
                var result = await _signInManager.PasswordSignInAsync(user.UserName!, password, false, false);
                
                if (result.Succeeded)
                {
                    ViewBag.Success = $"¡Login exitoso! Bienvenido {user.Email}";
                    return RedirectToAction("Index", "Home");
                }
                else if (result.IsLockedOut)
                {
                    ViewBag.Error = "Cuenta bloqueada.";
                }
                else if (result.IsNotAllowed)
                {
                    ViewBag.Error = "Login no permitido.";
                }
                else
                {
                    ViewBag.Error = $"Error de login. Detalles: Succeeded={result.Succeeded}, IsLockedOut={result.IsLockedOut}, IsNotAllowed={result.IsNotAllowed}, RequiresTwoFactor={result.RequiresTwoFactor}";
                }
            }
            catch (Exception ex)
            {
                ViewBag.Error = $"Excepción: {ex.Message}";
            }

            return View("Index");
        }
    }
}