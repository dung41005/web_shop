using Newtonsoft.Json;
using System.Xml.Linq;
using UC.eComm.Publish.Controllers;
using UC.eComm.Publish.Model;

namespace UC.eComm.Publish.Services
{
    public class JsonService
    {
        private readonly IWebHostEnvironment _webHostEnvironment;

        public JsonService(IWebHostEnvironment webHostEnvironment)
        {
            _webHostEnvironment = webHostEnvironment;
        }

        public  List<Product> GetProductsFromJsonFile()
        {
            var products = new List<Product>();
            try
            {
                string filePath = Path.Combine(_webHostEnvironment.WebRootPath, "dataproduct.json");
                string jsonString = File.ReadAllText(filePath);
                products =  JsonConvert.DeserializeObject<List<Product>>(jsonString);
            }
            catch (Exception ex){

            }
            return products;
        }
        public List<Address> GetAddressFromJsonFile()
        {
            var address = new List<Address>();
            try
            {
                string filePath = Path.Combine(_webHostEnvironment.WebRootPath, "danh_sach_xa.json");
                string jsonString = File.ReadAllText(filePath);
                address = JsonConvert.DeserializeObject<List<Address>>(jsonString);
            }
            catch (Exception ex)
            {

            }
            return address;
        }
    }
    
}
