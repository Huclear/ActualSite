using ActualSite.Domain;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages.Manage;
using System.Net;
using System.Security.Claims;

namespace ActualSite.Controllers
{
    public class AuthController : Controller
    {
        private readonly IHashingService _hashingService;
        private readonly DishesContext dishesContext;
        public AuthController(IHashingService hashingService, DishesContext dishesContext)
        {
            _hashingService = hashingService;
            this.dishesContext = dishesContext;
        }


        [HttpGet]
        public IActionResult SignIn()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> SignIn(string email, string password)
        {
            var user = await dishesContext.DishesUsers.FirstOrDefaultAsync(u => u.Email.Equals(email));
            if (user != null)
            {
                var role = await dishesContext.UserRoles.FirstOrDefaultAsync(r => r.IdRole.Equals(user.RoleId));
                if (role is null)
                    return Conflict("Cannot obtain user role");
                if (_hashingService.VerifyHash(password, new SaltedHash(user.Password, user.Salt)))
                {
                    var claims = new List<Claim>()
                    {
                        new Claim(ClaimTypes.Name, email),
                        new Claim(ClaimTypes.Role, role.RoleName)
                    };
                    await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(new ClaimsIdentity(claims, "Cookies")));
                    return RedirectToAction("Index", "Home");
                }
            }
            else ViewBag.ErrorMessage = "User with that email does not exists";
            return RedirectToAction(nameof(SignUp));
        }

        [HttpGet]
        public IActionResult SignUp()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> SignUp(string name, string phone, string email, string login, string password)
        {
            if ((await dishesContext.DishesUsers.FirstOrDefaultAsync(u => u.Email == email)) == null)
            {
                var hash = _hashingService.GenerateHash(password);
                DishesUser user = new DishesUser(name, phone, email, login, hash.hash, hash.salt);
                await dishesContext.AddAsync(user);
                await dishesContext.SaveChangesAsync();
            }
            return RedirectToAction("SignIn");
        }
    }
}
