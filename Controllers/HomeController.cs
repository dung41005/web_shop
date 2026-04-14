using ClosedXML.Excel;
using GemBox.Spreadsheet;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using UC.eComm.Publish.Context;
using UC.eComm.Publish.Model;
using UC.eComm.Publish.Services;

namespace UC.eComm.Publish.Controllers
{
    public class HomeController : Controller
    {
        private readonly MyDbContext _context;
        private readonly JsonService _jsonService;
        public HomeController(MyDbContext context, JsonService jsonService)
        {
            _context = context;
            _jsonService = jsonService;
        }

        public IActionResult Index()
        {
            ObjectHome objectHome = new ObjectHome();
            List<CartItem> cartItems = new List<CartItem>();
            try
            {
                objectHome.Products = _jsonService.GetProductsFromJsonFile();
            }
            catch (Exception ex) { }
            ////
            //string filePath = "wwwroot\\Danh_sach_xa.xls"; // Thay đổi đường dẫn tới file Excel của bạn

            //var tinhList = new List<Tinh>();
            //SpreadsheetInfo.SetLicense("");

            //var workbook = ExcelFile.Load(filePath);
            //var worksheet = workbook.Worksheets[0];

            //for (int rowIndex = 1; rowIndex < worksheet.Rows.Count; rowIndex++) // Bỏ qua dòng tiêu đề
            //{
            //    var row = worksheet.Rows[rowIndex];
            //    var maTinh = row.Cells[0].Value?.ToString();
            //    var tinhName = row.Cells[1].Value?.ToString();
            //    var maHuyen = row.Cells[2].Value?.ToString();
            //    var huyenName = row.Cells[3].Value?.ToString();
            //    var maXa = row.Cells[4].Value?.ToString();
            //    var xaName = row.Cells[5].Value?.ToString();

            //    var tinh = tinhList.FirstOrDefault(t => t.MaTinh == maTinh);
            //    if (tinh == null)
            //    {
            //        tinh = new Tinh
            //        {
            //            MaTinh = maTinh,
            //            TenTinh = tinhName,
            //            Huyen = new List<Huyen>()
            //        };
            //        tinhList.Add(tinh);
            //    }

            //    var huyen = tinh.Huyen.FirstOrDefault(h => h.MaHuyen == maHuyen);
            //    if (huyen == null)
            //    {
            //        huyen = new Huyen
            //        {
            //            MaHuyen = maHuyen,
            //            TenHuyen = huyenName,
            //            Xa = new List<Xa>()
            //        };
            //        tinh.Huyen.Add(huyen);
            //    }

            //    var xa = new Xa
            //    {
            //        MaXa = maXa,
            //        TenXa = xaName
            //    };
            //    huyen.Xa.Add(xa);
            //}


            //string jsonResult = JsonConvert.SerializeObject(tinhList, Formatting.Indented);
            //Console.WriteLine(jsonResult);

            //// Nếu muốn lưu kết quả JSON vào tệp
            //System.IO.File.WriteAllText("\\wwwroot\\address.json", jsonResult);
            return View(objectHome);
        }
    }

    public class ObjectHome
    {
        public int CartCount { get; set; }
        public List<Product> Products { get; set; }
        public List<CartItem> CartItems { get; set; }
    }
    public class Xa
    {
        public string MaXa { get; set; }
        public string TenXa { get; set; }
    }

    public class Huyen
    {
        public string MaHuyen { get; set; }
        public string TenHuyen { get; set; }
        public List<Xa> Xa { get; set; }
    }

    public class Tinh
    {
        public string MaTinh { get; set; }
        public string TenTinh { get; set; }
        public List<Huyen> Huyen { get; set; }
    }
}
