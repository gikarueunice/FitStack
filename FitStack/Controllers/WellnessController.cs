using Microsoft.AspNetCore.Mvc;

namespace FitStack.Controllers
{
    public class WellnessController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
