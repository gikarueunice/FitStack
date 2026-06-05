
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using FitStackDBL.Services;

namespace FitStack.Controllers
    {
        [Authorize]
        public abstract class BaseController : Controller
        {
            private IUserService? _userService;

            protected IUserService UserService => _userService ??= HttpContext.RequestServices.GetRequiredService<IUserService>();

            protected async Task LoadUserDataAsync()
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (int.TryParse(userIdClaim, out int userId))
                {
                    var user = await UserService.GetUserByIdAsync(userId);
                    if (user != null)
                    {
                        ViewBag.UserId = user.Id;
                        ViewBag.UserName = user.FullName;
                        ViewBag.UserEmail = user.Email;
                        ViewBag.UserAvatar = !string.IsNullOrEmpty(user.ProfilePictureUrl) ? user.ProfilePictureUrl : "";
                        ViewBag.UserInitial = GetUserInitial(user.FullName);
                        ViewBag.UserLevel = user.Level;
                        ViewBag.UserXP = user.XP;
                        ViewBag.ProfilePictureUrl = user.ProfilePictureUrl;
                    }
                }
            }

            private string GetUserInitial(string? fullName)
            {
                if (string.IsNullOrEmpty(fullName)) return "U";
                return fullName.Trim().FirstOrDefault().ToString().ToUpper();
            }
        }
    }
