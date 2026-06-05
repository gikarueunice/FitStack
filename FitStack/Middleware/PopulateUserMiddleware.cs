using FitStackDBL.Services;
using System.Security.Claims;

namespace FitStack.Middleware
{
    public class PopulateUserMiddleware
    {
        private readonly RequestDelegate _next;

        public PopulateUserMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                var userIdClaim = context.User?.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim != null && int.TryParse(userIdClaim.Value, out int userId))
                {
                    // Resolve service per-request
                    var userService = context.RequestServices.GetService(typeof(IUserService)) as IUserService;
                    if (userService != null)
                    {
                        var user = await userService.GetUserByIdAsync(userId);
                        if (user != null)
                        {
                            context.Items["UserAvatar"] = user.ProfilePictureUrl;
                            context.Items["UserName"] = user.FullName;
                            context.Items["UserEmail"] = user.Email;
                            context.Items["UserId"] = user.Id;
                        }
                    }
                }
            }
            catch
            {
                // swallow errors - don't block request
            }

            await _next(context);
        }
    }
}
