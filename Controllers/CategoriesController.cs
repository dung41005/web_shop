using Microsoft.AspNetCore.Mvc;
using UC.eComm.Publish.Context;
using UC.eComm.Publish.Model;
using UC.eComm.Publish.Services;

namespace UC.eComm.Publish.Controllers
{
    public class CategoriesController : Controller
    {
        //private readonly MyDbContext _context;
        private readonly JsonService _jsonService;
        public CategoriesController( JsonService jsonService)
        {
            //_context = context;
            _jsonService = jsonService;
        }
        public IActionResult Index()
        {
            List<Product> products = new List<Product>();
            try
            {
                //Lấy thông tin product theo nhóm hàng
                products = _jsonService.GetProductsFromJsonFile();
            }
            catch (Exception ex) { }
            return View(products);
            
        }

        [HttpGet("/category")]
        public IActionResult Category()
        {
            
            return View();
        }
    }
}
