using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace FitStack.Controllers
{
    public class ErrorController : Controller
    {
        private readonly ILogger<ErrorController> _logger;

        public ErrorController(ILogger<ErrorController> logger)
        {
            _logger = logger;
        }

        [Route("Error/NotFound")]
        public IActionResult NotFound()
        {
            Response.StatusCode = 404;
            return View();
        }

        [Route("Error/StatusCode/{code}")]
        public IActionResult StatusCode(int code)
        {
            switch (code)
            {
                case 404:
                    return RedirectToAction("NotFound");
                case 403:
                    ViewBag.Title = "Access Denied";
                    ViewBag.Message = "You don't have permission to access this page.";
                    ViewBag.Icon = "🔒";
                    break;
                case 500:
                    ViewBag.Title = "Server Error";
                    ViewBag.Message = "Something went wrong on our end. Our team has been notified.";
                    ViewBag.Icon = "⚠️";
                    break;
                default:
                    ViewBag.Title = "Error";
                    ViewBag.Message = "An error occurred.";
                    ViewBag.Icon = "❌";
                    break;
            }
            return View("CustomError");
        }
    }
}