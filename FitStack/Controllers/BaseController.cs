using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace FitStack.Controllers
{
    [Authorize]
    public abstract class BaseController : Controller
    {
        protected void SetUserDataFromClaims()
        {
            ViewBag.UserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            ViewBag.UserName = User.FindFirst(ClaimTypes.Name)?.Value ?? "User";
            ViewBag.UserEmail = User.FindFirst(ClaimTypes.Email)?.Value ?? "";
            ViewBag.UserLevel = User.FindFirst("UserLevel")?.Value ?? "1";
            ViewBag.UserXP = User.FindFirst("UserXP")?.Value ?? "0";
            ViewBag.UserAvatar = User.FindFirst("ProfilePicture")?.Value ?? "https://images.unsplash.com/photo-1535713875002-d1d0cf377fde?w=80&h=80&fit=crop";
        }
    }
}