using Microsoft.AspNetCore.Mvc;

namespace web_gui.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}