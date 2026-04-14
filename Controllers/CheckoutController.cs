using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Net;
using System.Text;
using UC.eComm.Publish.Model;
using UC.eComm.Publish.Services;
using UC.eComm.Publish.ViewModels;
using UC.eComm.Publish.ViewModels.Shared;

namespace UC.eComm.Publish.Controllers
{

    public class CheckoutController : Controller
    {
        
        private readonly JsonService _jsonService;
        private readonly GeocodingService _geocodingService;

        public CheckoutController(JsonService jsonService, GeocodingService geocodingService)
        {
            _jsonService = jsonService;
            _geocodingService = geocodingService;
        }

        public IActionResult Index()
        {
            CheckoutModel checkoutModel = new CheckoutModel();
            List<CartItem> cartItems = new List<CartItem>();
            List<Address> addresses = new List<Address>();
            List<AddressCookie> addressCookies = new List<AddressCookie>();

            try
            {
                if (Request.Cookies["cart_cookie"] != null)
                {
                    cartItems = JsonConvert.DeserializeObject<List<CartItem>>(Request.Cookies["cart_cookie"]);
                }
                checkoutModel.cartItems = cartItems;
                //
                if (Request.Cookies["address_cookie"] != null)
                {
                    addressCookies = JsonConvert.DeserializeObject<List<AddressCookie>>(Request.Cookies["address_cookie"]);
                }
                checkoutModel.addressCookies = addressCookies;
                //
                addresses = _jsonService.GetAddressFromJsonFile();
                checkoutModel.addresses = addresses
                                    .GroupBy(a => a.ProvinceId)
                                    .Select(g => g.First())
                                    .ToList();
            }
            catch (Exception ex) { }
            
            return View(checkoutModel);
        }
        [HttpGet]
        public IActionResult Payment_Select()
        {
            HeaderViewModel model = new HeaderViewModel();
            List<CartItem> cartItems = new List<CartItem>();
            decimal totalPrice = 0;
            try
            {
                if (Request.Cookies["cart_cookie"] != null)
                {
                    cartItems = JsonConvert.DeserializeObject<List<CartItem>>(Request.Cookies["cart_cookie"]);
                }
                if (cartItems.Count > 0)
                {
                    foreach (var cartItem in cartItems)
                    {
                        totalPrice += cartItem.TotalPrice;
                    }
                }
            }
            catch (Exception ex) { }
            
            model.Total = cartItems.Count;
         
            model.cartItems = cartItems;
            model.TotalPrice = totalPrice;

            return View(model);
        }
        [HttpPost]
        public IActionResult Payment_Select(string address_id)
        {
            // Lưu ID địa chỉ được chọn để dùng sau
            TempData["SelectedAddressId"] = address_id;

            // Chuyển hướng đến GET Payment_Select để hiển thị giao diện chọn phương thức
            return RedirectToAction("Payment_Select");
        }

        public async Task<IActionResult> Payment()
        {
            OrderModel model = new OrderModel();

            List<CartItem> cartItems = new List<CartItem>();
            decimal total = 0;

            try
            {
                if (Request.Cookies["cart_cookie"] != null)
                {
                    cartItems = JsonConvert.DeserializeObject<List<CartItem>>(Request.Cookies["cart_cookie"]);
                }

                foreach (var item in cartItems)
                {
                    total += item.TotalPrice;
                }
            }
            catch (Exception ex) { }

            model.CartItems = cartItems;
            model.TotalPrice = total;
            model.OrderId = "DH" + DateTime.Now.ToString("yyyyMMddHHmmss");

            // --- Lấy địa chỉ từ cookie ---
            string selectedAddressId = TempData["SelectedAddressId"] as string; // từ bước Payment_Select
            AddressCookie selectedAddress = null;

            var addressCookies = new List<AddressCookie>();
            if (Request.Cookies["address_cookie"] != null)
                addressCookies = JsonConvert.DeserializeObject<List<AddressCookie>>(Request.Cookies["address_cookie"]);

            selectedAddress = addressCookies.FirstOrDefault(a => a.Id == selectedAddressId) ?? addressCookies.FirstOrDefault();

            model.CustomerName = selectedAddress?.RecipientName ?? "Khách hàng";

            // --- Lấy tọa độ từ địa chỉ ---
            double? lat = null, lng = null;
            if (selectedAddress != null)
            {
                string fullAddress = $"{selectedAddress.Address}, {selectedAddress.Ward}, {selectedAddress.District}, {selectedAddress.Province}";
                (lat, lng) = await _geocodingService.GetCoordinatesAsync(fullAddress);
            }

            // --- Tạo QR ---
            var qrData = new
            {
                OrderId = model.OrderId,
                Customer = model.CustomerName,
                Phone = selectedAddress?.Phone,
                Address = $"{selectedAddress?.Address}, {selectedAddress?.Ward}, {selectedAddress?.District}, {selectedAddress?.Province}",
                Total = model.TotalPrice,
                Lat = lat,
                Lng = lng
            };
            string qrJson = JsonConvert.SerializeObject(qrData);
            string qrBase64 = GenerateQrCode(qrJson);
            ViewBag.QrCode = qrBase64; // hoặc thêm vào model

            return View(model);
        }
        [HttpPost]
        public async Task<IActionResult> PlaceOrderCod(string address_id)
        {
            // 1. Lấy địa chỉ từ cookie
            var addressCookies = new List<AddressCookie>();
            if (Request.Cookies["address_cookie"] != null)
                addressCookies = JsonConvert.DeserializeObject<List<AddressCookie>>(Request.Cookies["address_cookie"]);

            var selectedAddress = addressCookies.FirstOrDefault(a => a.Id == address_id) ?? addressCookies.FirstOrDefault();
            if (selectedAddress == null) return RedirectToAction("Index");

            // 2. Lấy giỏ hàng và tính tổng
            List<CartItem> cartItems = new List<CartItem>();
            if (Request.Cookies["cart_cookie"] != null)
                cartItems = JsonConvert.DeserializeObject<List<CartItem>>(Request.Cookies["cart_cookie"]);
            decimal total = cartItems.Sum(i => i.TotalPrice);

            // 3. Tạo mã đơn
            string orderId = "DH" + DateTime.Now.ToString("yyyyMMddHHmmss");

            // 4. Lấy tọa độ
            string fullAddress = $"{selectedAddress.Address}, {selectedAddress.Ward}, {selectedAddress.District}, {selectedAddress.Province}";
            var (lat, lng) = await _geocodingService.GetCoordinatesAsync(fullAddress);

            // 5. Tạo dữ liệu QR
            var qrData = new
            {
                OrderId = orderId,
                CustomerName = selectedAddress.RecipientName,
                Phone = selectedAddress.Phone,
                Address = fullAddress,
                TotalAmount = total,
                Latitude = lat,
                Longitude = lng
            };
            string qrJson = JsonConvert.SerializeObject(qrData);
            string qrBase64 = GenerateQrCode(qrJson);

            // 6. Chuẩn bị model cho view
            var model = new OrderSuccessViewModel
            {
                OrderId = orderId,
                CustomerName = selectedAddress.RecipientName,
                Phone = selectedAddress.Phone,
                Address = fullAddress,
                TotalAmount = total,
                Latitude = lat,
                Longitude = lng,
                QrCodeBase64 = qrBase64,
                FullQrData = qrJson
            };

            // 7. (Tùy chọn) Lưu đơn hàng vào DB ở đây

            return View("OrderSuccess", model);
        }
        private string GenerateQrCode(string content)
        {
            using (var qrGenerator = new QRCoder.QRCodeGenerator())
            using (var qrCodeData = qrGenerator.CreateQrCode(content, QRCoder.QRCodeGenerator.ECCLevel.Q))
            using (var qrCode = new QRCoder.PngByteQRCode(qrCodeData))
            {
                byte[] qrCodeImage = qrCode.GetGraphic(20);
                return Convert.ToBase64String(qrCodeImage);
            }
        }
        // Hàm helper lấy danh sách địa chỉ từ cookie
        private List<AddressCookie> GetAddressCookiesFromCookie()
        {
            if (Request.Cookies["address_cookie"] != null)
            {
                return JsonConvert.DeserializeObject<List<AddressCookie>>(Request.Cookies["address_cookie"])
                       ?? new List<AddressCookie>();
            }
            return new List<AddressCookie>();
        }
        [HttpPost("get-districts")]
        public IActionResult GetDistricts(string province_id)
        {
            StringBuilder html = new StringBuilder();
            List<Address> addresses = new List<Address>();
            try
            {
                html.Append("<option value=\"\">Chọn Quận / Huyện</option>");
                addresses = _jsonService.GetAddressFromJsonFile();
                List<Address> districts = addresses
                                    .Where(d => d.ProvinceId == province_id)
                                    .GroupBy(a => a.DistrictId)
                                    .Select(g => g.First())
                                    .ToList();
                foreach (var item in districts)
                {
                    if (item.ProvinceId == province_id)
                    {
                        html.Append("<option value=\"" + item.DistrictId + "\">" + item.District + "</option>");
                    }
                }
            }
            catch (Exception ex) { }
            return Ok(html.ToString());
        }
        [HttpPost("get-wards")]
        public IActionResult GetWards(string district_id)
        {

            StringBuilder html = new StringBuilder();
            List<Address> addresses = new List<Address>();
            try
            {
                html.Append("<option value=\"\">Chọn Phường / Xã</option>");
                addresses = _jsonService.GetAddressFromJsonFile();
                List<Address> wards = addresses
                                    .Where(d => d.DistrictId == district_id)
                                    .GroupBy(a => a.WardId)
                                    .Select(g => g.First())
                                    .ToList();
                foreach (var item in wards)
                {
                    if (item.DistrictId == district_id)
                    {
                        html.Append("<option value=\"" + item.WardId + "\">" + item.Ward + "</option>");
                    }
                }
            }
            catch (Exception ex) { }

            return Ok(html.ToString());
        }
        //#endregion

        //Thêm xóa sửa địa chỉ
        #region
        [HttpPost("add-address")]
        public IActionResult AddAddress([FromBody] List<DataAddToCart> datas)
        {
            StringBuilder html = new StringBuilder();
            List<AddressCookie> addressCookies = new List<AddressCookie>();
            AddressCookie addressCookie = new AddressCookie();
            List<Address> addresses = new List<Address>();
            
            try
            {
                if (Request.Cookies["address_cookie"] != null)
                {
                    addressCookies = JsonConvert.DeserializeObject<List<AddressCookie>>(Request.Cookies["address_cookie"]);
                }
                foreach (var item in datas) {
                    if(item.Name == "recipient_name" && !string.IsNullOrEmpty(item.Value)) addressCookie.RecipientName = item.Value;
                    if (item.Name == "address" && !string.IsNullOrEmpty(item.Value)) addressCookie.Address = item.Value;
                    if (item.Name == "province_id" && !string.IsNullOrEmpty(item.Value)) addressCookie.ProvinceId = item.Value;
                    if (item.Name == "district_id" && !string.IsNullOrEmpty(item.Value)) addressCookie.DistrictId = item.Value;
                    if (item.Name == "ward_id" && !string.IsNullOrEmpty(item.Value)) addressCookie.WardId = item.Value;
                    if (item.Name == "phone" && !string.IsNullOrEmpty(item.Value)) addressCookie.Phone = item.Value;
                }
                addresses = _jsonService.GetAddressFromJsonFile();
                
                addressCookie.Province = addresses.FirstOrDefault(a => a.ProvinceId == addressCookie.ProvinceId)?.Province;
                addressCookie.District = addresses.FirstOrDefault(a => a.DistrictId == addressCookie.DistrictId)?.District;
                addressCookie.Ward = addresses.FirstOrDefault(a => a.WardId == addressCookie.WardId)?.Ward;
                addressCookie.Id = DateTime.Now.ToString("ddmmyyyyhhmmss");

                addressCookies.Add(addressCookie);
                Response.Cookies.Append("address_cookie", JsonConvert.SerializeObject(addressCookies), new CookieOptions
                {
                    HttpOnly = true,
                    Expires = DateTimeOffset.UtcNow.AddMonths(12),
                    Secure = true,
                    SameSite = SameSiteMode.Lax
                });
                foreach( var item in addressCookies)
                {
                    html.Append("       <div class=\"col-md-6 mb-3\">\r\n" +
                                "           <label class=\"aiz-megabox d-block bg-white mb-0\">\r\n");
                    if (item.Id == addressCookie.Id) {
                        html.Append("           <input type=\"radio\" name=\"address_id\" value=\"" + item.Id + "\" checked required>\r\n");
                    } else {
                        html.Append("           <input type=\"radio\" name=\"address_id\" value=\"" + item.Id + "\" required>\r\n");
                    }
                    html.Append("               <span class=\"d-flex p-3 aiz-megabox-elem\">\r\n" +
                                "                   <span class=\"aiz-rounded-check flex-shrink-0 mt-1\"></span>\r\n" +
                                "                   <span class=\"flex-grow-1 pl-3 text-left\">\r\n" +
                                "                       <div>\r\n" +
                                "                           <span class=\"opacity-60\">Địa chỉ:</span>\r\n" +
                                "                           <span class=\"fw-600 ml-2\">"+ item.Address +"</span>\r\n" +
                                "                       </div>\r\n" +
                                "                       <div>\r\n" +
                                "                           <span class=\"opacity-60\">Phường / Xã:</span>\r\n" +
                                "                           <span class=\"fw-600 ml-2\">"+ item.Ward +"</span>\r\n" +
                                "                       </div>\r\n" +
                                "                       <div>\r\n" +
                                "                           <span class=\"opacity-60\">Quận / Huyện:</span>\r\n" +
                                "                           <span class=\"fw-600 ml-2\">"+ item.District +"</span>\r\n" +
                                "                       </div>\r\n" +
                                "                       <div>\r\n" +
                                "                           <span class=\"opacity-60\">Tỉnh / Thành phố:</span>\r\n" +
                                "                           <span class=\"fw-600 ml-2\">"+ item.Province +"</span>\r\n" +
                                "                       </div>\r\n" +
                                "                       <div>\r\n" +
                                "                           <span class=\"opacity-60\">Điện thoại:</span>\r\n" +
                                "                           <span class=\"fw-600 ml-2\">"+ item.Phone +"</span>\r\n" +
                                "                       </div>\r\n" +
                                "                   </span>\r\n" +
                                "               </span>\r\n" +
                                "           </label>\r\n" +
                                "           <div class=\"dropdown position-absolute right-0 top-0\">\r\n" +
                                "               <button class=\"btn bg-gray px-2\" type=\"button\" data-toggle=\"dropdown\">\r\n" +
                                "                   <i class=\"la la-ellipsis-v\"></i>\r\n" +
                                "               </button>\r\n" +
                                "               <div class=\"dropdown-menu dropdown-menu-right\" aria-labelledby=\"dropdownMenuButton\">\r\n" +
                                "                   <a class=\"dropdown-item\" onclick=\"showModalAdress(" + item.Id + ")\">\r\nCập nhật\r\n</a>\r\n" +
                                "                   <a class=\"dropdown-item\" onclick=\"removeAddress(" + item.Id + ")\">\r\nXóa\r\n</a>\r\n" +
                                "               </div>\r\n" +
                                "           </div>\r\n" +
                                "       </div>");
                }
                html.Append("           <input type=\"hidden\" name=\"checkout_type\" value=\"logged\">" +
                            "           <div class=\"col-md-6 mx-auto mb-3\">\r\n" +
                            "               <div class=\"border p-3 rounded mb-3 c-pointer text-center bg-white h-100 d-flex flex-column justify-content-center\" onclick=\"add_new_address()\">\r\n" +
                            "                   <i class=\"las la-plus la-2x mb-3\"></i>\r\n" +
                            "                   <div class=\"alpha-7\">Thêm địa chỉ mới</div>\r\n" +
                            "               </div>\r\n" +
                            "           </div>");
                
            }
            catch (Exception ex) { }

            var results = new
            {
                modal_view = html.ToString(),
            };
            return Ok(results);
        }
        [HttpGet("show-modal-edit-address/{id}")]
        public IActionResult ShowModalEditAddress(string id)
        {
            StringBuilder html = new StringBuilder();
            List<AddressCookie> addressCookies = new List<AddressCookie>();
            AddressCookie addressCookie = new AddressCookie();
            List<Address> addresses = new List<Address>();
            List<Address> provinces = new List<Address>();
            List<Address> districts = new List<Address>();
            List<Address> wards = new List<Address>();

            try
            {
                if (Request.Cookies["address_cookie"] != null)
                {
                    addressCookies = JsonConvert.DeserializeObject<List<AddressCookie>>(Request.Cookies["address_cookie"]);
                }

                for (int i = 0; i < addressCookies.Count; i++)
                {
                    if (addressCookies[i].Id == id) addressCookie = addressCookies[i];
                }
                addresses = _jsonService.GetAddressFromJsonFile();

                provinces = addresses
                                    .GroupBy(a => a.ProvinceId)
                                    .Select(g => g.First())
                                    .ToList();

                // Bắt đầu form
                html.Append("<form class=\"form-default\" role=\"form\" id=\"option-choice-form1\">\r\n" +
                    "           <input type=\"hidden\" name=\"_token\" value=\"hqOwpB9VbwFfE4tpjNhkzb0nDii9quqLOAeF0Wvu\">" +
                    "           <input type=\"hidden\" name=\"id\" value=\"" + addressCookie.Id + "\">" +
                    "           <div class=\"p-3\">\r\n");

                // 🔥 THÊM TRƯỜNG HỌ TÊN NGƯỜI NHẬN
                html.Append("               <div class=\"row\">\r\n" +
                    "                   <div class=\"col-md-3\">\r\n" +
                    "                       <label>Họ tên người nhận</label>\r\n" +
                    "                   </div>\r\n" +
                    "                   <div class=\"col-md-9\">\r\n" +
                    "                       <input type=\"text\" class=\"form-control mb-3\" placeholder=\"Nguyễn Văn A\" name=\"recipient_name\" value=\"" + (addressCookie.RecipientName ?? "") + "\" required>\r\n" +
                    "                   </div>\r\n" +
                    "               </div>\r\n");

                // Địa chỉ
                html.Append("               <div class=\"row\">\r\n" +
                    "                   <div class=\"col-md-3\">\r\n" +
                    "                       <label>Địa chỉ</label>\r\n" +
                    "                   </div>\r\n" +
                    "                   <div class=\"col-md-9\">\r\n" +
                    "                       <textarea class=\"form-control mb-3\" placeholder=\"Địa chỉ của bạn\" rows=\"2\" name=\"address\" required=\"\">" + addressCookie.Address + "</textarea>\r\n" +
                    "                   </div>\r\n" +
                    "               </div>\r\n");

                // Tỉnh / Thành phố
                html.Append("               <div class=\"row\">\r\n" +
                    "                   <div class=\"col-md-3\">\r\n" +
                    "                       <label>Tỉnh / Thành phố</label>\r\n" +
                    "                   </div>\r\n" +
                    "                   <div class=\"col-md-9\">\r\n" +
                    "                       <div class=\"dropdown bootstrap-select form-control mb-3 aiz-\">\r\n" +
                    "                           <select class=\"form-control mb-3 aiz-selectpicker\" name=\"province_id\" id=\"edit_state\" data-live-search=\"true\" required tabindex=\"-98\">\r\n");

                foreach (var item in provinces)
                {
                    if (item.ProvinceId == addressCookie.ProvinceId)
                    {
                        html.Append("                   <option value=\"" + item.ProvinceId + "\" selected>\r\n" + item.Province + "\r\n                                        </option>\r\n");
                    }
                    else
                    {
                        html.Append("                   <option value=\"" + item.ProvinceId + "\">\r\n" + item.Province + "\r\n                                        </option>\r\n");
                    }
                }

                html.Append("                           </select>\r\n" +
                    "                       </div>\r\n" +
                    "                   </div>\r\n" +
                    "               </div>\r\n\r\n");

                // Quận / Huyện
                html.Append("               <div class=\"row\">\r\n" +
                    "                   <div class=\"col-md-3\">\r\n" +
                    "                       <label>Quận / Huyện</label>\r\n" +
                    "                   </div>\r\n" +
                    "                   <div class=\"col-md-9\">\r\n" +
                    "                       <div class=\"dropdown bootstrap-select form-control mb-3 aiz-\">\r\n" +
                    "                           <select class=\"form-control mb-3 aiz-selectpicker\" data-live-search=\"true\" name=\"district_id\" required tabindex=\"-98\">\r\n");

                districts = addresses.Where(a => a.ProvinceId == addressCookie.ProvinceId)
                                     .GroupBy(a => a.DistrictId)
                                     .Select(g => g.First())
                                     .ToList();
                foreach (var item in districts)
                {
                    if (item.DistrictId == addressCookie.DistrictId)
                    {
                        html.Append("               <option value=\"" + item.DistrictId + "\" selected>\r\n                                            " + item.District + "\r\n                                        </option>\r\n");
                    }
                    else
                    {
                        html.Append("              <option value=\"" + item.DistrictId + "\">\r\n                                            " + item.District + "\r\n                                        </option>\r\n");
                    }
                }

                html.Append("                           </select>\r\n" +
                    "                       </div>\r\n" +
                    "                   </div>\r\n" +
                    "               </div>\r\n\r\n");

                // Phường / Xã
                html.Append("               <div class=\"row\">\r\n" +
                    "                   <div class=\"col-md-3\">\r\n" +
                    "                       <label>Phường / Xã</label>\r\n" +
                    "                   </div>\r\n" +
                    "                   <div class=\"col-md-9\">\r\n" +
                    "                       <div class=\"dropdown bootstrap-select form-control mb-3 aiz- dropup\">\r\n" +
                    "                           <select class=\"form-control mb-3 aiz-selectpicker\" data-live-search=\"true\" name=\"ward_id\" required tabindex=\"-98\">\r\n");

                wards = addresses.Where(a => a.DistrictId == addressCookie.DistrictId)
                                 .ToList();
                foreach (var item in wards)
                {
                    if (item.WardId == addressCookie.WardId)
                    {
                        html.Append("                               <option value=\"" + item.WardId + "\" selected>\r\n                                            " + item.Ward + "\r\n                                        </option>\r\n");
                    }
                    else
                    {
                        html.Append("                               <option value=\"" + item.WardId + "\">\r\n                                            " + item.Ward + "\r\n                                        </option>\r\n");
                    }
                }

                html.Append("                           </select>\r\n" +
                    "                       </div>\r\n" +
                    "                   </div>\r\n" +
                    "               </div>\r\n\r\n\r\n");

                // Mã bưu điện (ẩn)
                html.Append("               <div class=\"row d-none\">\r\n" +
                    "                   <div class=\"col-md-3\">\r\n" +
                    "                       <label>Mã bưu điện</label>\r\n" +
                    "                   </div>\r\n" +
                    "                   <div class=\"col-md-9\">\r\n" +
                    "                       <input type=\"text\" class=\"form-control mb-3\" placeholder=\"Mã bưu chính\" value=\"\" name=\"postal_code\" required>\r\n" +
                    "                   </div>\r\n" +
                    "               </div>\r\n");

                // Điện thoại
                html.Append("               <div class=\"row\">\r\n" +
                    "                   <div class=\"col-md-3\">\r\n" +
                    "                       <label>Điện thoại</label>\r\n" +
                    "                   </div>\r\n" +
                    "                   <div class=\"col-md-9\">\r\n" +
                    "                       <input type=\"text\" class=\"form-control mb-3\" placeholder=\"+84\" value=\"" + addressCookie.Phone + "\" name=\"phone\" required>\r\n" +
                    "                   </div>\r\n" +
                    "               </div>\r\n");

                // Nút Lưu
                html.Append("               <div class=\"form-group text-right\">\r\n" +
                    "                   <button type=\"button\" class=\"btn btn-sm btn-primary\" onclick=\"editAddress()\">Lưu</button>\r\n" +
                    "               </div>\r\n" +
                    "           </div>\r\n" +
                    "       </form>");
            }
            catch (Exception ex) { }

            var results = new
            {
                modal_view = html.ToString(),
            };
            return Ok(results);
        }
        [HttpPost("edit-address")]
        public IActionResult EditAddress([FromBody] List<DataAddToCart> datas)
        {
            StringBuilder html = new StringBuilder();
            List<AddressCookie> addressCookies = new List<AddressCookie>();
            AddressCookie addressCookie = new AddressCookie();
            List<Address> addresses = new List<Address>();

            try
            {
                if (Request.Cookies["address_cookie"] != null)
                {
                    addressCookies = JsonConvert.DeserializeObject<List<AddressCookie>>(Request.Cookies["address_cookie"]);
                }
                foreach (var item in datas)
                {
                    if (item.Name == "id" && !string.IsNullOrEmpty(item.Value)) addressCookie.Id = item.Value;
                    if (item.Name == "recipient_name" && !string.IsNullOrEmpty(item.Value)) addressCookie.RecipientName = item.Value;
                    if (item.Name == "address" && !string.IsNullOrEmpty(item.Value)) addressCookie.Address = item.Value;
                    if (item.Name == "province_id" && !string.IsNullOrEmpty(item.Value)) addressCookie.ProvinceId = item.Value;
                    if (item.Name == "district_id" && !string.IsNullOrEmpty(item.Value)) addressCookie.DistrictId = item.Value;
                    if (item.Name == "ward_id" && !string.IsNullOrEmpty(item.Value)) addressCookie.WardId = item.Value;
                    if (item.Name == "phone" && !string.IsNullOrEmpty(item.Value)) addressCookie.Phone = item.Value;
                }
                
                addresses = _jsonService.GetAddressFromJsonFile();

                addressCookie.Province = addresses.FirstOrDefault(a => a.ProvinceId == addressCookie.ProvinceId)?.Province;
                addressCookie.District = addresses.FirstOrDefault(a => a.DistrictId == addressCookie.DistrictId)?.District;
                addressCookie.Ward = addresses.FirstOrDefault(a => a.WardId == addressCookie.WardId)?.Ward;

                for (int i = 0; i < addressCookies.Count; i++)
                {
                    if (addressCookies[i].Id == addressCookie.Id)
                    {
                        //addressCookies.RemoveAt(i);
                        addressCookies[i] = addressCookie;
                    }
                }

                Response.Cookies.Append("address_cookie", JsonConvert.SerializeObject(addressCookies), new CookieOptions
                {
                    HttpOnly = true,
                    Expires = DateTimeOffset.UtcNow.AddMonths(12),
                    Secure = true,
                    SameSite = SameSiteMode.Lax
                });
                foreach (var item in addressCookies)
                {
                    html.Append("       <div class=\"col-md-6 mb-3\">\r\n" +
                                "           <label class=\"aiz-megabox d-block bg-white mb-0\">\r\n");
                    if (item.Id == addressCookie.Id)
                    {
                        html.Append("           <input type=\"radio\" name=\"address_id\" value=\"" + item.Id + "\" checked required>\r\n");
                    }
                    else
                    {
                        html.Append("           <input type=\"radio\" name=\"address_id\" value=\"" + item.Id + "\" required>\r\n");
                    }
                    html.Append("               <span class=\"d-flex p-3 aiz-megabox-elem\">\r\n" +
                                "                   <span class=\"aiz-rounded-check flex-shrink-0 mt-1\"></span>\r\n" +
                                "                   <span class=\"flex-grow-1 pl-3 text-left\">\r\n" +
                                "                       <div>\r\n" +
                                "                           <span class=\"opacity-60\">Người nhận:</span>\r\n" +
                                "                           <span class=\"fw-600 ml-2\">" + item.RecipientName + "</span>\r\n" +
                                "                       </div>\r\n" +
                                "                       <div>\r\n" +
                                "                           <span class=\"opacity-60\">Địa chỉ:</span>\r\n" +
                                "                           <span class=\"fw-600 ml-2\">" + item.Address + "</span>\r\n" +
                                "                       </div>\r\n" +
                                "                       <div>\r\n" +
                                "                           <span class=\"opacity-60\">Phường / Xã:</span>\r\n" +
                                "                           <span class=\"fw-600 ml-2\">" + item.Ward + "</span>\r\n" +
                                "                       </div>\r\n" +
                                "                       <div>\r\n" +
                                "                           <span class=\"opacity-60\">Quận / Huyện:</span>\r\n" +
                                "                           <span class=\"fw-600 ml-2\">" + item.District + "</span>\r\n" +
                                "                       </div>\r\n" +
                                "                       <div>\r\n" +
                                "                           <span class=\"opacity-60\">Tỉnh / Thành phố:</span>\r\n" +
                                "                           <span class=\"fw-600 ml-2\">" + item.Province + "</span>\r\n" +
                                "                       </div>\r\n" +
                                "                       <div>\r\n" +
                                "                           <span class=\"opacity-60\">Điện thoại:</span>\r\n" +
                                "                           <span class=\"fw-600 ml-2\">" + item.Phone + "</span>\r\n" +
                                "                       </div>\r\n" +
                                "                   </span>\r\n" +
                                "               </span>\r\n" +
                                "           </label>\r\n" +
                                "           <div class=\"dropdown position-absolute right-0 top-0\">\r\n" +
                                "               <button class=\"btn bg-gray px-2\" type=\"button\" data-toggle=\"dropdown\">\r\n" +
                                "                   <i class=\"la la-ellipsis-v\"></i>\r\n" +
                                "               </button>\r\n" +
                                "               <div class=\"dropdown-menu dropdown-menu-right\" aria-labelledby=\"dropdownMenuButton\">\r\n" +
                                "                   <a class=\"dropdown-item\" onclick=\"showModalAdress(" + item.Id + ")\">\r\nCập nhật\r\n</a>\r\n" +
                                "                   <a class=\"dropdown-item\" onclick=\"removeAddress(" + item.Id + ")\">\r\nXóa\r\n</a>\r\n" +
                                "               </div>\r\n" +
                                "           </div>\r\n" +
                                "       </div>");
                }
                html.Append("           <input type=\"hidden\" name=\"checkout_type\" value=\"logged\">" +
                            "           <div class=\"col-md-6 mx-auto mb-3\">\r\n" +
                            "               <div class=\"border p-3 rounded mb-3 c-pointer text-center bg-white h-100 d-flex flex-column justify-content-center\" onclick=\"add_new_address()\">\r\n" +
                            "                   <i class=\"las la-plus la-2x mb-3\"></i>\r\n" +
                            "                   <div class=\"alpha-7\">Thêm địa chỉ mới</div>\r\n" +
                            "               </div>\r\n" +
                            "           </div>");

            }
            catch (Exception ex) { }

            var results = new
            {
                modal_view = html.ToString(),
            };
            return Ok(results);
        }
        [HttpPost("remove-address/{id}")]
        public IActionResult RemoveAddress(string id)
        {
            StringBuilder html = new StringBuilder();
            List<AddressCookie> addressCookies = new List<AddressCookie>();
           
            try
            {
                if (Request.Cookies["address_cookie"] != null)
                {
                    addressCookies = JsonConvert.DeserializeObject<List<AddressCookie>>(Request.Cookies["address_cookie"]);
                }
                for (int i = 0; i < addressCookies.Count; i++)
                {
                    if (addressCookies[i].Id == id)
                    {
                        addressCookies.RemoveAt(i);
                    }
                }

                Response.Cookies.Append("address_cookie", JsonConvert.SerializeObject(addressCookies), new CookieOptions
                {
                    HttpOnly = true,
                    Expires = DateTimeOffset.UtcNow.AddMonths(12),
                    Secure = true,
                    SameSite = SameSiteMode.Lax
                });
                int index = 0;
                foreach (var item in addressCookies)
                {
                    html.Append("       <div class=\"col-md-6 mb-3\">\r\n" +
                                "           <label class=\"aiz-megabox d-block bg-white mb-0\">\r\n");
                    if (index == 0)
                    {
                        html.Append("           <input type=\"radio\" name=\"address_id\" value=\"" + item.Id + "\" checked required>\r\n");
                        index++;
                    }
                    else
                    {
                        html.Append("           <input type=\"radio\" name=\"address_id\" value=\"" + item.Id + "\" required>\r\n");
                    }
                    html.Append("               <span class=\"d-flex p-3 aiz-megabox-elem\">\r\n" +
                                "                   <span class=\"aiz-rounded-check flex-shrink-0 mt-1\"></span>\r\n" +
                                "                   <span class=\"flex-grow-1 pl-3 text-left\">\r\n" +
                                "                       <div>\r\n" +
                                "                           <span class=\"opacity-60\">Địa chỉ:</span>\r\n" +
                                "                           <span class=\"fw-600 ml-2\">" + item.Address + "</span>\r\n" +
                                "                       </div>\r\n" +
                                "                       <div>\r\n" +
                                "                           <span class=\"opacity-60\">Phường / Xã:</span>\r\n" +
                                "                           <span class=\"fw-600 ml-2\">" + item.Ward + "</span>\r\n" +
                                "                       </div>\r\n" +
                                "                       <div>\r\n" +
                                "                           <span class=\"opacity-60\">Quận / Huyện:</span>\r\n" +
                                "                           <span class=\"fw-600 ml-2\">" + item.District + "</span>\r\n" +
                                "                       </div>\r\n" +
                                "                       <div>\r\n" +
                                "                           <span class=\"opacity-60\">Tỉnh / Thành phố:</span>\r\n" +
                                "                           <span class=\"fw-600 ml-2\">" + item.Province + "</span>\r\n" +
                                "                       </div>\r\n" +
                                "                       <div>\r\n" +
                                "                           <span class=\"opacity-60\">Điện thoại:</span>\r\n" +
                                "                           <span class=\"fw-600 ml-2\">" + item.Phone + "</span>\r\n" +
                                "                       </div>\r\n" +
                                "                   </span>\r\n" +
                                "               </span>\r\n" +
                                "           </label>\r\n" +
                                "           <div class=\"dropdown position-absolute right-0 top-0\">\r\n" +
                                "               <button class=\"btn bg-gray px-2\" type=\"button\" data-toggle=\"dropdown\">\r\n" +
                                "                   <i class=\"la la-ellipsis-v\"></i>\r\n" +
                                "               </button>\r\n" +
                                "               <div class=\"dropdown-menu dropdown-menu-right\" aria-labelledby=\"dropdownMenuButton\">\r\n" +
                                "                   <a class=\"dropdown-item\" onclick=\"editAddress(" + item.Id + ")\">\r\nCập nhật\r\n</a>\r\n" +
                                "                   <a class=\"dropdown-item\" onclick=\"removeAddress(" + item.Id + ")\">\r\nXóa\r\n</a>\r\n" +
                                "               </div>\r\n" +
                                "           </div>\r\n" +
                                "       </div>");
                }
                html.Append("           <input type=\"hidden\" name=\"checkout_type\" value=\"logged\">" +
                            "           <div class=\"col-md-6 mx-auto mb-3\">\r\n" +
                            "               <div class=\"border p-3 rounded mb-3 c-pointer text-center bg-white h-100 d-flex flex-column justify-content-center\" onclick=\"add_new_address()\">\r\n" +
                            "                   <i class=\"las la-plus la-2x mb-3\"></i>\r\n" +
                            "                   <div class=\"alpha-7\">Thêm địa chỉ mới</div>\r\n" +
                            "               </div>\r\n" +
                            "           </div>");

            }
            catch (Exception ex) { }

            var results = new
            {
                modal_view = html.ToString(),
            };
            return Ok(results);
        }

    }
    #endregion
    public class Address
    {
        public string WardId { get; set; }
        public string Ward { get; set; }
        public string EnglishName { get; set; }
        public string Level { get; set; }
        public string DistrictId { get; set; }
        public string District { get; set; }
        public string ProvinceId { get; set; }
        public string Province { get; set; }
    }
    public class CheckoutModel
    {
        public List<Address> addresses = new List<Address>();
        public List<CartItem> cartItems = new List<CartItem>();
        public List<AddressCookie> addressCookies = new List<AddressCookie>();
    }

    public class AddressCookie
    {
        public string Id { get; set; }
        public string RecipientName { get; set; }
        public string Address { get; set; }
        public string Province { get; set; }
        public string District { get; set; }
        public string Ward { get; set; }
        public string ProvinceId { get; set; }
        public string DistrictId { get; set; }
        public string WardId { get; set; }
        public string Phone { get; set; }
    }
    public class OrderModel
    {
        public string OrderId { get; set; }
        public string CustomerName { get; set; }
        public decimal TotalPrice { get; set; }
        public List<CartItem> CartItems { get; set; } = new List<CartItem>();
    }
    public class DeliveryQrData
    {
        public string OrderId { get; set; }
        public string CustomerName { get; set; }
        public string Phone { get; set; }
        public string Address { get; set; }
        public decimal TotalAmount { get; set; }
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
    }
    public class OrderSuccessViewModel
    {
        public string OrderId { get; set; }
        public string CustomerName { get; set; }
        public string Phone { get; set; }
        public string Address { get; set; }
        public decimal TotalAmount { get; set; }
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
        public string QrCodeBase64 { get; set; }
        public string FullQrData { get; set; } // Chuỗi JSON thô để debug
    }
}

