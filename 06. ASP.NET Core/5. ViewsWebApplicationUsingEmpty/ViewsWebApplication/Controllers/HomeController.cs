using Microsoft.AspNetCore.Mvc;

namespace ViewsWebApplication.Controllers
{
    [Controller]
    public class HomeController : Controller
    {
        [Route("/")]
        [Route("/Home")]
        public IActionResult Index()
        {
            return View();
        }

       
    }
}
