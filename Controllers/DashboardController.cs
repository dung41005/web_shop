using Microsoft.AspNetCore.Mvc;

namespace UC.Razor.Web.Controllers
{
	public class DashboardController : Controller
	{
		public IActionResult Index()
		{
			return View();
		}
	}
}
