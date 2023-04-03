    using Microsoft.AspNetCore.Mvc;

namespace EcommercePractical.Areas.User.Controllers
{
    public class ViewController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
