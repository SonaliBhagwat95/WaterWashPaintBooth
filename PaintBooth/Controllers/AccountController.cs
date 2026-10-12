using PaintBooth.Models;
using PaintBooth.Repositories.Interface;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace PaintBooth.Controllers
{
    public class AccountController(IAuthRepository _authRepo, IExceptionRepository _Ex) : Controller
    {
        // ── GET /Account/Login ────────────────────────────────────
        [HttpGet]
        public IActionResult Login()
        {
            //if (User.Identity?.IsAuthenticated == true)
            //    return RedirectToAction("Index", "Dashboard");

            return View(new LoginViewModel());
        }

        // ── POST /Account/Login ───────────────────────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<JsonResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
                return Json(new { success = false, message = "Please fill in all fields." });

            try
            {
                var user = _authRepo.ValidateUser(model.Username, model.Password);

                if (user == null)
                    return Json(new { success = false, message = "Invalid username or password." });

                // Build cookie claims
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.NameIdentifier, user.EmpId.ToString()),
                    new Claim(ClaimTypes.Name,           user.Username),
                    new Claim("FullName",                user.FullName),
                    new Claim("UserRoleId",              user.UserRoleId.ToString()),
                    new Claim(ClaimTypes.Role,           user.RoleName)
                };

                var identity  = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                var principal = new ClaimsPrincipal(identity);

                await HttpContext.SignInAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme,
                    principal,
                    new AuthenticationProperties
                    {
                        IsPersistent = model.RememberMe,
                        ExpiresUtc   = model.RememberMe
                                       ? DateTimeOffset.UtcNow.AddDays(30)
                                       : DateTimeOffset.UtcNow.AddHours(8)
                    });

                // Save in session
                HttpContext.Session.SetInt32("UserId",    user.EmpId);
                HttpContext.Session.SetString("FullName", user.FullName);
                HttpContext.Session.SetString("RoleName", user.RoleName);

                return Json(new { success = true, redirectUrl = Url.Action("Index", "Dashboard") });
            }
            catch (Exception ex)
            {
                _Ex.SaveException("AuthController", "Login", ex.Message, 0);
                return Json(new { success = false, message = "Something went wrong. Please try again." });
            }
        }

        // ── GET /Account/Logout ───────────────────────────────────
        [HttpGet]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            HttpContext.Session.Clear();
            return RedirectToAction("Login", "Account");
        }
    }
}
