using Microsoft.AspNetCore.Mvc;

namespace FitStack.Controllers
{
    public class DashboardController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
