using Microsoft.AspNetCore.Mvc;

namespace UC.eComm.Publish.Controllers
{
    public class ShopsController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
        public IActionResult Create()
        {
            return View();
        }
    }
}
