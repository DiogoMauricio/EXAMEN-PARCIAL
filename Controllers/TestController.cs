using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace examen_parcial.Controllers
{
    public class TestController : Controller
    {
        private readonly UserManager<IdentityUser> _userManager;

        public TestController(UserManager<IdentityUser> userManager)
        {
            _userManager = userManager;
        }

        public async Task<IActionResult> VerifyUser()
        {
            var user = await _userManager.FindByEmailAsync("admin@gmail.com");
            
            if (user != null)
            {
                var roles = await _userManager.GetRolesAsync(user);
                var checkPassword = await _userManager.CheckPasswordAsync(user, "Admin2024!");
                
                ViewBag.UserExists = true;
                ViewBag.Email = user.Email;
                ViewBag.UserName = user.UserName;
                ViewBag.EmailConfirmed = user.EmailConfirmed;
                ViewBag.Roles = string.Join(", ", roles);
                ViewBag.PasswordCheck = checkPassword;
            }
            else
            {
                ViewBag.UserExists = false;
            }

            return View();
        }
    }
}