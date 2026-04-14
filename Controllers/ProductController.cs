using Microsoft.AspNetCore.Mvc;
using UC.eComm.Publish.Context;
using UC.eComm.Publish.Model;
using UC.eComm.Publish.Services;

namespace UC.eComm.Publish.Controllers
{
    public class ProductController : Controller
    {
        private readonly MyDbContext _context;
        private readonly JsonService _jsonService;
        public ProductController(MyDbContext context, JsonService jsonService)
        {
            _context = context;
            _jsonService = jsonService;
        }
        public IActionResult Detail(string slug, int id)
        {
            DetaiProduct detailProduct = new DetaiProduct();
            List<Product> ProductByCatgory = new List<Product>();
            try
            {
                List<Product> products = _jsonService.GetProductsFromJsonFile();
                //Lấy thông tin chi tiết 1 sản phẩm
                foreach (var item in products)
                {
                    if (item.Id == id)
                    {
                        detailProduct.Product = item;
                    }
                }
                //Lấy thông tin các sản phẩm có cùng loại hàng
                foreach (var item in products)
                {
                    if (item.Category == detailProduct.Product.Category && item.Id != detailProduct.Product.Id)
                    {
                        ProductByCatgory.Add(item);
                    }
                }
                detailProduct.ProductByCatgory = ProductByCatgory;
                detailProduct.Products = products;
            }
            catch (Exception ex){ }
            return View(detailProduct);
        }
    }

    public class DetaiProduct
    {
        public Product Product { get; set; }
        public List<Product> ProductByCatgory { get; set; }
        public List<Product> Products { get; set; }
    }
}
