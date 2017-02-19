using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using SimpleBank.Web.ViewModels;
using System.Security.Claims;
using Microsoft.AspNetCore.Http.Authentication;
using SimpleBank.Web.Data;
using Microsoft.EntityFrameworkCore;
using SimpleBank.Service.Data;
using SimpleBank.Service.IServices;

namespace SimpleBank.Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly IUserService _userService;

        public HomeController(
            IUserService userService
            )
        {
            _userService = userService;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Error()
        {
            return View();
        }

        // POST: /Home/Login
        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                var loggedUser = await _userService.GetUserByAccountNoAndPassword(model.AccountNumber, model.Password);
                if (loggedUser != null)
                {
                    var claims = new List<Claim>
                    {
                        new Claim("UserId", loggedUser.ID.ToString(), ClaimValueTypes.Integer),
                        new Claim(ClaimTypes.Name, loggedUser.AccountNumber, ClaimValueTypes.String),
                        new Claim(ClaimTypes.GivenName, loggedUser.AccountName, ClaimValueTypes.String),
                        new Claim(ClaimTypes.Role, "SimpleBank_DefaultUser", ClaimValueTypes.String)
                    };

                    var userIdentity = new ClaimsIdentity(claims, "local");
                    var userPrincipal = new ClaimsPrincipal(userIdentity);

                    await HttpContext.Authentication.SignInAsync("SimpleBankCookies", userPrincipal);

                    return RedirectToAction("Index", "Account");
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Invalid login attempt.");
                    return View("Index", model);
                }
            }

            // If we got this far, something failed, redisplay form
            return View("Index", model);
        }

        // POST: /Home/Logout
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.Authentication.SignOutAsync("SimpleBankCookies");
            return RedirectToAction("Index");
        }
    }
}
