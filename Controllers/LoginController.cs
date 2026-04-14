using Microsoft.AspNetCore.Mvc;

namespace UC.Razor.Web.Controllers
{
	public class LoginController : Controller
	{
		public IActionResult Index()
		{
			return View();
		}
	}
}
