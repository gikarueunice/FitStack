using FitStack.ViewModels;
using FitStackDBL;
using FitStackDBL.Model;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver.Core.Configuration;
using System.Security.Claims;

namespace FitStack.Controllers
{
    public class AuthController : Controller
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<AuthController> _logger;
        private readonly Bl _bl;

        public AuthController(IConfiguration configuration, ILogger<AuthController> logger)
        {
            _configuration = configuration;
            _logger = logger;

            var ConnectionString = _configuration.GetConnectionString("DefaultConnection");
            _bl = new Bl(ConnectionString);
        }
        [HttpGet]
        [AllowAnonymous]
        public IActionResult Register()
        {
            var model = new RegisterViewModel();
            return View(model);
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                // Check if email already exists
                var existingUser = await _bl.UsersRepository.GetByEmail(model.Email);
                if (existingUser != null)
                {
                    ModelState.AddModelError("Email", "Email already exists");
                    return View(model);
                }

                // Validate password
                if (!FitStack.Helpers.PasswordHelper.IsValidPassword(model.Password))
                {
                    ModelState.AddModelError("Password", "Password must be at least 6 characters");
                    return View(model);
                }



                // Create user
                var user = new Users
                {
                    FullName = model.FullName.Trim(),
                    Email = model.Email.Trim().ToLower(),
                    Password = FitStack.Helpers.PasswordHelper.HashPassword(model.Password),
                    IsActive = true
                };

                var userId = await _bl.UsersRepository.CreateUser(user);

                // On successful registration redirect to login
                if (userId > 0)
                {
                    return RedirectToAction("Login", "Auth");
                }

                ModelState.AddModelError("", "Registration failed. Please try again.");
                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Registration failed for email: {model.Email}");
                ModelState.AddModelError("", $"Registration failed: {ex.Message}");
                return View(model);
            }
        }
        [HttpGet]
        [AllowAnonymous]
        public IActionResult Login(string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model, string returnUrl = null)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                // Get user by email
                var user = await _bl.UsersRepository.GetByEmail(model.Email);

                // Validate user
                if (user == null)
                {
                    ModelState.AddModelError("", "Invalid email or password");
                    return View(model);
                }

                if (!user.IsActive)
                {
                    ModelState.AddModelError("", "Account is deactivated. Please contact administrator.");
                    return View(model);
                }

                // Verify password
                if (!FitStack.Helpers.PasswordHelper.VerifyPassword(model.Password, user.Password))
                {
                    ModelState.AddModelError("", "Invalid email or password");
                    return View(model);
                }

                // Update login statistics
                await _bl.UsersRepository.UpdateLoginStats(user.Id);

                // Create claims and sign in
                await SignInUser(user, model.RememberMe);

                // Redirect to returnUrl or home after successful login
                if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                {
                    return Redirect(returnUrl);
                }

                return RedirectToAction("Index", "Home");

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Login failed for email: {model.Email}");
                ModelState.AddModelError("", "Login failed. Please try again.");
                return View(model);
            }
        }

        private async Task SignInUser(Users user, bool rememberMe = false)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.FullName),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim("LastLogin", user.LastLogin?.ToString() ?? DateTime.UtcNow.ToString())
            };

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

            var authProperties = new AuthenticationProperties
            {
                IsPersistent = rememberMe,
                ExpiresUtc = rememberMe ? DateTimeOffset.UtcNow.AddDays(7) : DateTimeOffset.UtcNow.AddHours(1)
            };

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity),
                authProperties);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]

        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Index", "Home");
        }
    }
}
