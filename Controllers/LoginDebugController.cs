using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;

namespace examen_parcial.Controllers
{
    public class LoginDebugController : Controller
    {
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly UserManager<IdentityUser> _userManager;

        public LoginDebugController(SignInManager<IdentityUser> signInManager, UserManager<IdentityUser> userManager)
        {
            _signInManager = signInManager;
            _userManager = userManager;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> TestLogin(string email, string password)
        {
            var result = new
            {
                Email = email,
                Password = password?.Length + " caracteres",
                UserExists = false,
                PasswordValid = false,
                LoginResult = "",
                UserDetails = "",
                Error = ""
            };

            try
            {
                // 1. Buscar usuario
                var user = await _userManager.FindByEmailAsync(email);
                if (user == null)
                {
                    return Json(new { 
                        success = false, 
                        message = "Usuario no encontrado",
                        details = result
                    });
                }

                result = result with { 
                    UserExists = true,
                    UserDetails = $"UserName: {user.UserName}, Email: {user.Email}, EmailConfirmed: {user.EmailConfirmed}"
                };

                // 2. Verificar contraseña
                var passwordCheck = await _userManager.CheckPasswordAsync(user, password);
                result = result with { PasswordValid = passwordCheck };

                if (!passwordCheck)
                {
                    return Json(new { 
                        success = false, 
                        message = "Contraseña incorrecta",
                        details = result
                    });
                }

                // 3. Intentar login
                var signInResult = await _signInManager.PasswordSignInAsync(user.UserName!, password, false, false);
                
                result = result with { 
                    LoginResult = $"Succeeded: {signInResult.Succeeded}, IsLockedOut: {signInResult.IsLockedOut}, IsNotAllowed: {signInResult.IsNotAllowed}, RequiresTwoFactor: {signInResult.RequiresTwoFactor}"
                };

                if (signInResult.Succeeded)
                {
                    return Json(new { 
                        success = true, 
                        message = "Login exitoso",
                        redirectUrl = "/",
                        details = result
                    });
                }
                else
                {
                    return Json(new { 
                        success = false, 
                        message = "Login falló",
                        details = result
                    });
                }
            }
            catch (Exception ex)
            {
                result = result with { Error = ex.Message };
                return Json(new { 
                    success = false, 
                    message = $"Excepción: {ex.Message}",
                    details = result
                });
            }
        }

        [HttpPost]
        public async Task<IActionResult> ListUsers()
        {
            try
            {
                var users = _userManager.Users.Take(10).Select(u => new {
                    u.Id,
                    u.Email,
                    u.UserName,
                    u.EmailConfirmed,
                    u.LockoutEnabled,
                    u.AccessFailedCount
                }).ToList();

                var usersWithRoles = new List<object>();
                foreach (var user in users)
                {
                    var userEntity = await _userManager.FindByIdAsync(user.Id);
                    var roles = await _userManager.GetRolesAsync(userEntity!);
                    var passwordCheck1 = await _userManager.CheckPasswordAsync(userEntity!, "admin123");
                    var passwordCheck2 = await _userManager.CheckPasswordAsync(userEntity!, "1234");
                    
                    usersWithRoles.Add(new {
                        user.Id,
                        user.Email,
                        user.UserName,
                        user.EmailConfirmed,
                        user.LockoutEnabled,
                        user.AccessFailedCount,
                        Roles = roles,
                        PasswordAdmin123 = passwordCheck1,
                        Password1234 = passwordCheck2
                    });
                }

                return Json(new { success = true, users = usersWithRoles });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }
    }
}