using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text;
using UC.eComm.Publish.Context;
using UC.eComm.Publish.Model;
using UC.eComm.Publish.Context;
using Microsoft.Extensions.Primitives;
using Newtonsoft.Json.Linq;
using UC.eComm.Publish.Services;
using UC.eComm.Publish.ViewModels.Shared;
using UC.eComm.Publish.ViewModels;
using Newtonsoft.Json;
using System;

namespace UC.eComm.Publish.Controllers
{

    [ApiController]
    [Route("[controller]")]
    public class CartController : Controller
    {
        //private readonly MyDbContext _context;
        private readonly JsonService _jsonService;

        public CartController( JsonService jsonService)
        {
            //_context = context;
            _jsonService = jsonService;
        }

        public IActionResult Index()
        {
            List<CartItem> cartItems = new List<CartItem>();
            //CookieModel cookieModel = new CookieModel();
            decimal totalPrice = 0;
            try
            {
                if (Request.Cookies["cart_cookie"] != null)
                {
                    cartItems = JsonConvert.DeserializeObject<List<CartItem>>(Request.Cookies["cart_cookie"]);
                }

                foreach (var cartItem in cartItems)
                {
                    totalPrice += cartItem.TotalPrice;
                }

            }
            catch (Exception ex) { }
            HeaderViewModel model = new HeaderViewModel();
            model.cartItems = cartItems;
            model.TotalPrice = totalPrice;
            return View(model);
        }

        [HttpPost("show-cart-modal")]
        public async Task<IActionResult> ShowCartModal([FromForm] int id)
        {
            StringBuilder html = new StringBuilder();
            string[] arrayImg = [];
            try
            {
                List<Product> products = _jsonService.GetProductsFromJsonFile();

                Product product  = new Product();
                foreach (var item in products) {
                    if(item.Id == id)
                    {
                        product = item;
                    }
                } 
                arrayImg = product.Image.Split('|');
                html.Append("<div class=\"modal-body p-4 c-scrollbar-light\">\r\n" +
                    "   <div class=\"row\">\r\n" +
                    "       <div class=\"col-lg-6\">\r\n" +
                    "           <div class=\"row gutters-10 flex-row-reverse\">\r\n" +
                    "               <div class=\"col\">\r\n" +
                    "                   <div class=\"aiz-carousel product-gallery \" data-nav-for=\".product-gallery-thumb\" data-fade=\"true\" data-auto-height=\"true\">"
                    );
                foreach (string img in arrayImg)
                {
                    html.Append("           <div>\r\n" +
                                "               <div class=\"carousel-box img-zoom rounded\" style=\"width: 100%; display: inline-block; position: relative; overflow: hidden;\">\r\n" +
                                "                   <img class=\"img-fluid ls-is-cached lazyloaded\" src=\"" + img + "?v=" + DateTime.Now.ToString("yyyy-mm-dd hhmmssfff") + "\" data-src=\"" + img + "\" onerror=\"this.onerror=null;this.src='/img/placeholder.jpg';\">\r\n" +
                                "                   <img role=\"presentation\" alt=\"\" src=\"" + img + "?v=" + DateTime.Now.ToString("yyyy-mm-dd hhmmssfff") + "\" class=\"zoomImg\" style=\"position: absolute; top: -335.29px; left: -4.94148px; opacity: 0; width: 750px; height: 750px; border: none; max-width: none; max-height: none;\">\r\n" +
                                "               </div>\r\n" +
                                "           </div>");
                }
                html.Append("            </div>\r\n" +
                            "       </div>\r\n" +
                            "       <div class=\"col-auto w-90px\">\r\n" +
                            "           <div class=\"aiz-carousel carousel-thumb product-gallery-thumb \" data-items=\"5\" data-nav-for=\".product-gallery\" data-vertical=\"true\" data-focus-select=\"true\">");
                foreach (string img in arrayImg)
                {
                    html.Append("           <div>\r\n" +
                                "               <div class=\"carousel-box c-pointer border p-1 rounded\" style=\"width: 100%; display: inline-block;\">\r\n" +
                                "                   <img class=\"mw-100 size-60px mx-auto ls-is-cached lazyloaded\" src=\"" + img + "?v=" + DateTime.Now.ToString("yyyy-mm-dd hhmmssfff") + "\" data-src=\"" + img + "\" onerror=\"this.onerror=null;this.src='/img/placeholder.jpg';\">\r\n" +
                                "               </div>\r\n" +
                                "           </div>     ");
                }
                html.Append("                    </div>\r\n" +
                            "                </div>\r\n" +
                            "            </div>\r\n" +
                            "        </div>\r\n\r\n" +
                            "        <div class=\"col-lg-6\">\r\n" +
                            "            <div class=\"text-left\">\r\n" +
                            "                <h2 class=\"mb-2 fs-20 fw-600\">\r\n" + product.Name + "\r\n</h2>\r\n\r\n" +
                            "                <div class=\"row no-gutters mt-3\">\r\n" +
                            "                    <div class=\"col-2\">\r\n" +
                            "                        <div class=\"opacity-50\">Giá bán:</div>\r\n" +
                            "                    </div>\r\n" +
                            "                    <div class=\"col-10\">\r\n" +
                            "                        <div class=\"\">\r\n" +
                            "                            <strong class=\"h2 fw-600 text-primary\">\r\n" + product.Price.ToString("N0") + " " + product.UnitPrice +"\r\n</strong>\r\n" +
                            "                            <span class=\"opacity-70\">/"+ product.Unit +"</span>\r\n" +
                            "                        </div>\r\n" +
                            "                    </div>\r\n" +
                            "                </div>\r\n\r\n\r\n" +
                            "                <hr>\r\n\r\n\r\n" +
                            "                <form id=\"option-choice-form\">\r\n" +
                            "                    <input type=\"hidden\" name=\"_token\" value=\"WwjH0FIgafq2gVMf5onPQZoB5YmmWEd8kbGMEkyB\">" +
                            "                    <input type=\"hidden\" name=\"id\" value=\"" + product.Id + "\">\r\n" +
                            "                    <input type=\"hidden\" name=\"digital\" value=\"0\">\r\n\r\n" +
                            "                    <!-- Quantity + Add to cart -->\r\n\r\n\r\n" +
                            "                    <div class=\"row no-gutters\">\r\n" +
                            "                        <div class=\"col-2\">\r\n" +
                            "                            <div class=\"opacity-50 mt-2\">Số lượng:</div>\r\n" +
                            "                        </div>\r\n" +
                            "                        <div class=\"col-10\">\r\n" +
                            "                            <div class=\"product-quantity d-flex align-items-center\">\r\n" +
                            "                                <div class=\"row no-gutters align-items-center aiz-plus-minus mr-3\" style=\"width: 130px;\">\r\n" +
                            "                                    <button class=\"btn col-auto btn-icon btn-sm btn-circle btn-light\" type=\"button\" data-type=\"minus\" data-field=\"quantity\" disabled=\"disabled\">\r\n" +
                            "                                        <i class=\"las la-minus\"></i>\r\n" +
                            "                                    </button>\r\n" +
                            "                                    <input type=\"number\" name=\"quantity\" class=\"col border-0 text-center flex-grow-1 fs-16 input-number\" placeholder=\"1\" value=\"1\" min=\"1\" max=\""+ product.Quantity +"\" lang=\"en\">\r\n" +
                            "                                    <button class=\"btn  col-auto btn-icon btn-sm btn-circle btn-light\" type=\"button\" data-type=\"plus\" data-field=\"quantity\">\r\n" +
                            "                                        <i class=\"las la-plus\"></i>\r\n" +
                            "                                    </button>\r\n" +
                            "                                </div>\r\n" +
                            "                                <div class=\"avialable-amount opacity-60\">\r\n(<span id=\"available-quantity\">"+ product.Quantity +"</span> Còn hàng)\r\n                                </div>\r\n" +
                            "                            </div>\r\n" +
                            "                        </div>\r\n" +
                            "                    </div>\r\n\r\n" +
                            "                    <hr>\r\n\r\n" +
                            "                    <div class=\"row no-gutters pb-3\" id=\"chosen_price_div\">\r\n" +
                            "                        <div class=\"col-2\">\r\n" +
                            "                            <div class=\"opacity-50\">Tổng giá:</div>\r\n" +
                            "                        </div>\r\n" +
                            "                        <div class=\"col-10\">\r\n" +
                            "                            <div class=\"product-price\">\r\n" +
                            "                                <strong class=\"h4 fw-600 text-primary\"><span id=\"chosen_price\">" + product.Price.ToString("N0") + "</span> " + product.UnitPrice +" </strong>\r\n" +
                            "                            </div>\r\n" +
                            "                        </div>\r\n" +
                            "                    </div>\r\n\r\n" +
                            "                </form>\r\n" +
                            "                <div class=\"mt-3\">\r\n" +
                            "                    <button type=\"button\" class=\"btn btn-primary buy-now fw-600 add-to-cart\" onclick=\"addToCart()\">\r\n" +
                            "                        <i class=\"la la-shopping-cart\"></i>\r\n" +
                            "                        <span class=\"d-none d-md-inline-block\">Thêm vào giỏ hàng</span>\r\n" +
                            "                    </button>\r\n" +
                            "                    <button type=\"button\" class=\"btn btn-secondary out-of-stock fw-600 d-none\" disabled=\"\">\r\n" +
                            "                        <i class=\"la la-cart-arrow-down\"></i>Hết hàng\r\n" +
                            "                    </button>\r\n" +
                            "                </div>\r\n\r\n" +
                            "            </div>\r\n" +
                            "        </div>\r\n" +
                            "    </div>\r\n" +
                            "</div>");
            }
            catch (Exception ex)
            {
            }
            return Ok(html.ToString());
        }

        [HttpPost("addtocart")]
        public async Task<IActionResult> AddToCart([FromBody] List<DataAddToCart> datas)
        {
            StringBuilder html = new StringBuilder();
            StringBuilder htmlCart = new StringBuilder();
            int id = 0;
            int quantity = 0;
            try
            {
                List<Product> products = _jsonService.GetProductsFromJsonFile();
                Product product = new Product();
                List<CartItem> cartItems = new List<CartItem>();
                //CookieModel cookieModel = new CookieModel();
                if (Request.Cookies["cart_cookie"] != null)
                {
                    cartItems = JsonConvert.DeserializeObject<List<CartItem>>(Request.Cookies["cart_cookie"]);
                }
                CartItem cartItem = new CartItem();
                //Lấy id và số lượng trueyèn vào
                foreach (var d in datas.ToList())
                {
                        if (d.Name == "id")
                        {
                            id = Int32.Parse(d.Value);
                        }
                        if (d.Name == "quantity")
                        {
                            quantity = Int32.Parse(d.Value);
                        }
                }
                //lấy sản phẩm có id giống id truyền vào
                foreach (var item in products.ToList())
                {
                    if (item.Id == id)
                    {
                        cartItem.Id = item.Id;
                        cartItem.Name = item.Name;
                        cartItem.Price = item.Price;
                        cartItem.UnitPrice = item.UnitPrice;
                        cartItem.Unit = item.Unit;
                        cartItem.Category = item.Category;
                        cartItem.Image = item.Image;
                        cartItem.QuantityInCart = quantity;
                        cartItem.Quantity = item.Quantity;
                        cartItem.TotalPrice = item.Price * quantity;
                        cartItem.Company = item.Company;
                        cartItem.Slug = item.Slug;
                    }
                }
                
                //thêm sản phầm vào json cookie
                cartItems.RemoveAll(item => item.Id == cartItem.Id);
                 
                cartItems.Add(cartItem);
                
                Response.Cookies.Append("cart_cookie", JsonConvert.SerializeObject(cartItems), new CookieOptions
                {
                    HttpOnly = true,
                    Expires = DateTimeOffset.UtcNow.AddMinutes(60),
                    Secure = true,
                    SameSite = SameSiteMode.Lax
                });

                //Tạo html thêm giỏ hàng thành công
                html.Append("<div class=\"modal-body p-4 added-to-cart\">\r\n" +
                            "   <div class=\"text-center text-success mb-4\">\r\n" +
                            "       <i class=\"las la-check-circle la-3x\"></i>\r\n" +
                            "       <h3>Sản phẩm đã được thêm vào giỏ hàng</h3>\r\n" +
                            "   </div>\r\n" +
                            "   <div class=\"media mb-4\">\r\n" +
                            "       <img src=\""+ cartItem.Image.Split('|')[0] +"\" data-src=\""+ cartItem.Image.Split('|')[0] + "?v=" + DateTime.Now.ToString("yyyy-mm-dd hhmmssfff") + "\" class=\"mr-3 size-100px img-fit rounded ls-is-cached lazyloaded\" alt=\"Product Image\">\r\n" +
                            "       <div class=\"media-body pt-3 text-left\">\r\n" +
                            "           <h6 class=\"fw-600\">\r\n                "+ cartItem.Name +"\r\n            </h6>\r\n" +
                            "           <div class=\"row mt-3\">\r\n" +
                            "               <div class=\"col-sm-2 opacity-60\">\r\n" +
                            "                   <div>Giá bán:</div>\r\n" +
                            "               </div>\r\n" +
                            "           <div class=\"col-sm-10\">\r\n" +
                            "               <div class=\"h6 text-primary\">\r\n" +
                            "                   <strong>\r\n                            "+ cartItem.Price.ToString("N0") +" " + cartItem.UnitPrice + "\r\n                        </strong>\r\n" +
                            "               </div>\r\n" +
                            "           </div>\r\n" +
                            "       </div>\r\n" +
                            "   </div>\r\n" +
                            "</div>\r\n" +
                            "<div class=\"bg-white rounded shadow-sm\">\r\n" +
                            "   <div class=\"border-bottom p-3\">\r\n" +
                            "       <h3 class=\"fs-16 fw-600 mb-0\">\r\n" +
                            "           <span class=\"mr-4\">Thường được mua cùng</span>\r\n" +
                            "       </h3>\r\n" +
                            "   </div>\r\n" +
                            "<div class=\"p-3\">\r\n" +
                            "<div class=\"aiz-carousel gutters-5 half-outside-arrow\" data-items=\"2\" data-xl-items=\"3\" data-lg-items=\"4\" data-md-items=\"3\" data-sm-items=\"2\" data-xs-items=\"2\" data-arrows=\"true\" data-infinite=\"true\">"
                            );
                foreach (var item in cartItems)
                {
                    products.RemoveAll(product => product.Id == item.Id);
                }
                foreach (var p in products)
                {
                    html.Append("<div>\r\n" +
                        "   <div class=\"carousel-box\" style=\"width: 100%; display: inline-block;\">\r\n" +
                        "       <div class=\"aiz-card-box border border-light rounded hov-shadow-md my-2 has-transition\">\r\n" +
                        "           <div class=\"\">\r\n" +
                        "               <a href=\"/product/"+ p.Slug +"-"+ p.Id +"\" class=\"d-block\" tabindex=\"-1\">\r\n" +
                        "                   <img class=\"img-fit mx-auto ls-is-cached lazyloaded\" src=\""+ p.Image.Split('|')[0] +"?v="+DateTime.Now.ToString("yyyy-mm-dd hhmmssfff")+"\" data-src=\""+ p.Image.Split('|')[0]  + "\" alt=\""+ p.Name +"\" onerror=\"this.onerror=null;this.src='/img/placeholder.jpg';\">\r\n" +
                        "               </a>\r\n" +
                        "           </div>\r\n" +
                        "           <div class=\"p-md-3 p-2 text-left\">\r\n" +
                        "               <div class=\"fs-15\">\r\n" +
                        "                   <span class=\"fw-700 text-primary\">"+ p.Price.ToString("N0")+" " + p.UnitPrice + "</span>\r\n" +
                        "               </div>\r\n" +
                        "               <div class=\"rating rating-sm mt-1\">\r\n" +
                        "                   <i class=\"las la-star\"></i>" +
                        "                   <i class=\"las la-star\"></i>" +
                        "                   <i class=\"las la-star\"></i>" +
                        "                   <i class=\"las la-star\"></i>" +
                        "                   <i class=\"las la-star\"></i>\r\n" +
                        "               </div>\r\n" +
                        "               <h3 class=\"fw-600 fs-16 text-truncate-2 lh-1-4 mb-0 h-37px\">\r\n" +
                        "                   <a href=\"/product/thit-lon-den-gac-bep-500g-chuan-vi-tay-bac\" class=\"d-block text-reset\" tabindex=\"-1\">"+ p.Name +"</a>\r\n" +
                        "               </h3>\r\n" +
                        "           </div>\r\n" +
                        "       </div>\r\n" +
                        "   </div>\r\n" +
                        "</div>");
                }
                html.Append("           </div>\r\n" +
                            "       </div>\r\n" +
                            "   </div>\r\n" +
                            "   <div class=\"text-center\">\r\n" +
                            "       <button class=\"btn btn-outline-primary mb-3 mb-sm-0\" data-dismiss=\"modal\">Quay lại xem tiếp</button>\r\n" +
                            "       <a href=\"/cart\" class=\"btn btn-primary mb-3 mb-sm-0\">Thanh toán</a>\r\n" +
                            "   </div>\r\n" +
                            "</div>");

                //Tạo html giỏ hàng
                htmlCart.Append("<a href=\"javascript:void(0)\" class=\"d-flex align-items-center text-reset h-100\" data-toggle=\"dropdown\" data-display=\"static\"  aria-expanded=\"false\">\r\n" +
                    "               <i class=\"la la-shopping-cart la-2x opacity-80\"></i>\r\n" +
                    "               <span class=\"flex-grow-1 ml-1\">\r\n" +
                    "                   <span class=\"badge badge-primary badge-inline badge-pill cart-count\">\r\n                "+ cartItems.Count + "\r\n            </span>\r\n" +
                    "                   <span class=\"nav-box-text d-none d-xl-block opacity-70\">Giỏ hàng</span>\r\n" +
                    "               </span>\r\n" +
                    "           </a>");
                htmlCart.Append("<div class=\"dropdown-menu dropdown-menu-right dropdown-menu-lg p-0 stop-propagation\">\r\n    \r\n" +
                                "   <div class=\"p-3 fs-15 fw-600 p-3 border-bottom\">\r\nCác mặt hàng trong giỏ hàng\r\n        </div>\r\n" +
                                "   <ul class=\"h-250px overflow-auto c-scrollbar-light list-group list-group-flush\">\r\n");

                Decimal totalPrice = 0;
                foreach (var item in cartItems.ToList()) {
                    totalPrice += item.Price * item.QuantityInCart;
                    htmlCart.Append(
                        "   <li class=\"list-group-item\">\r\n" +
                        "       <span class=\"d-flex align-items-center\">\r\n" +
                        "           <a href=\"/product/"+ item.Slug +"-"+ item.Id+"\" class=\"text-reset d-flex align-items-center flex-grow-1\">\r\n" +
                        "               <img src=\"" + item.Image.Split('|')[0] + "?v=" + DateTime.Now.ToString("yyyy-mm-dd hhmmssfff") + "\" data-src=\"" + item.Image.Split('|')[0] + "\" class=\"img-fit size-60px rounded ls-is-cached lazyloaded\" alt=\"" + item.Name + "\">\r\n" +
                        "               <span class=\"minw-0 pl-2 flex-grow-1\">\r\n" +
                        "                   <span class=\"fw-600 mb-1 text-truncate-2\">\r\n" + item.Name + "\r\n" +
                        "                   </span>\r\n" +
                        "                   <span class=\"\">"+ item.QuantityInCart + "x</span>\r\n" +
                        "                   <span class=\"\">"+ item.Price.ToString("N0") +" "+ item.UnitPrice +"</span>\r\n" +
                        "               </span>\r\n" +
                        "           </a>\r\n" +
                        "           <span class=\"\">\r\n" +
                        "               <button onclick=\"removeFromCart("+ item.Id +")\" class=\"btn btn-sm btn-icon stop-propagation\">\r\n" +
                        "                   <i class=\"la la-close\"></i>\r\n" +
                        "               </button>\r\n" +
                        "           </span>\r\n" +
                        "       </span>\r\n" +
                        "   </li>\r\n");
                                   
                }
                htmlCart.Append(
                    "       </ul>\r\n" +
                    "       <div class=\"px-3 py-2 fs-15 border-top d-flex justify-content-between\">\r\n" +
                    "           <span class=\"opacity-60\">Tạm tính</span>\r\n" +
                    "           <span class=\"fw-600\">"+ totalPrice.ToString("N0") + " VND</span>\r\n" +
                    "       </div>\r\n" +
                    "       <div class=\"px-3 py-2 text-center border-top\">\r\n" +
                    "       <ul class=\"list-inline mb-0\">\r\n" +
                    "           <li class=\"list-inline-item\">\r\n" +
                    "               <a href=\"/cart\" class=\"btn btn-soft-primary btn-sm\">\r\n                        Xem giỏ hàng\r\n                    </a>\r\n" +
                    "           </li>\r\n" +
                    "       </ul>\r\n" +
                    "   </div>\r\n" +
                    "</div>");

                var results = new
                {
                    modal_view = html.ToString(),
                    nav_cart_view = htmlCart.ToString(),
                    cart_count = cartItems.Count()
                };
                return Ok(results);
            }
            catch (Exception ex) {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("removefromcart")]
        public async Task<IActionResult> RemoveFromCart([FromForm] int id)
        {
            StringBuilder htmlCart = new StringBuilder();
            StringBuilder html = new StringBuilder();
            int quantity = 0;
            try
            {
                List<CartItem> cartItems = new List<CartItem>();
                if (Request.Cookies["cart_cookie"] != null)
                {
                    cartItems = JsonConvert.DeserializeObject<List<CartItem>>(Request.Cookies["cart_cookie"]);
                }
                
                //thêm sản phầm vào json cookie
                if (cartItems != null)
                {
                    cartItems.RemoveAll(item => item.Id == id);
                }
                
                Response.Cookies.Append("cart_cookie", JsonConvert.SerializeObject(cartItems), new CookieOptions
                {
                    HttpOnly = true,
                    Expires = DateTimeOffset.UtcNow.AddMinutes(60),
                    Secure = true,
                    SameSite = SameSiteMode.Lax
                });

                if (cartItems.Count > 0)
                {
                    //Tạo html giỏ hàng
                    htmlCart.Append("<a href=\"javascript:void(0)\" class=\"d-flex align-items-center text-reset h-100\" data-toggle=\"dropdown\" data-display=\"static\"  aria-expanded=\"false\">\r\n" +
                                    "   <i class=\"la la-shopping-cart la-2x opacity-80\"></i>\r\n" +
                                    "   <span class=\"flex-grow-1 ml-1\">\r\n" +
                                    "       <span class=\"badge badge-primary badge-inline badge-pill cart-count\">\r\n                " + cartItems.Count + "\r\n            </span>\r\n" +
                                    "       <span class=\"nav-box-text d-none d-xl-block opacity-70\">Giỏ hàng</span>\r\n" +
                                    "   </span>\r\n" +
                                    "</a>");
                    htmlCart.Append("<div class=\"dropdown-menu dropdown-menu-right dropdown-menu-lg p-0 stop-propagation\">\r\n    \r\n" +
                                    "   <div class=\"p-3 fs-15 fw-600 p-3 border-bottom\">\r\nCác mặt hàng trong giỏ hàng\r\n        </div>\r\n" +
                                    "       <ul class=\"h-250px overflow-auto c-scrollbar-light list-group list-group-flush\">\r\n");

                    Decimal totalPrice = 0;
                    foreach (var item in cartItems.ToList())
                    {
                        totalPrice += item.Price * item.QuantityInCart;
                        htmlCart.Append(
                            "<li class=\"list-group-item\">\r\n" +
                            "   <span class=\"d-flex align-items-center\">\r\n" +
                            "       <a href=\"/product/"+ item.Slug +"-"+ item.Id +"\" class=\"text-reset d-flex align-items-center flex-grow-1\">\r\n" +
                            "           <img src=\"" + item.Image.Split('|')[0] + "\" data-src=\"" + item.Image.Split('|')[0] + "\" class=\"img-fit size-60px rounded ls-is-cached lazyloaded\" alt=\"" + item.Name + "\">\r\n" +
                            "           <span class=\"minw-0 pl-2 flex-grow-1\">\r\n" +
                            "               <span class=\"fw-600 mb-1 text-truncate-2\">\r\n" + item.Name + "\r\n" +
                            "           </span>\r\n" +
                            "           <span class=\"\">" + item.QuantityInCart + "x</span>\r\n" +
                            "           <span class=\"\">" + item.Price.ToString("N0") + " " + item.UnitPrice + "</span>\r\n</span>\r\n" +
                            "       </a>\r\n" +
                            "       <span class=\"\">\r\n" +
                            "           <button onclick=\"removeFromCart(" + item.Id + ")\" class=\"btn btn-sm btn-icon stop-propagation\">\r\n" +
                            "               <i class=\"la la-close\"></i>\r\n" +
                            "           </button>\r\n" +
                            "       </span>\r\n" +
                            "   </span>\r\n" +
                            "</li>\r\n");

                    }
                    htmlCart.Append(
                        "       </ul>\r\n" +
                        "           <div class=\"px-3 py-2 fs-15 border-top d-flex justify-content-between\">\r\n" +
                        "               <span class=\"opacity-60\">Tạm tính</span>\r\n" +
                        "               <span class=\"fw-600\">" + totalPrice.ToString("N0") + " VND</span>\r\n" +
                        "           </div>\r\n" +
                        "           <div class=\"px-3 py-2 text-center border-top\">\r\n" +
                        "       <ul class=\"list-inline mb-0\">\r\n" +
                        "           <li class=\"list-inline-item\">\r\n" +
                        "               <a href=\"/cart\" class=\"btn btn-soft-primary btn-sm\">\r\n                        Xem giỏ hàng\r\n                    </a>\r\n" +
                        "           </li>\r\n" +
                        "       </ul>\r\n" +
                        "   </div>\r\n" +
                        "</div>");
                    html.Append("<div class=\"container\">\r\n" +
                                "   <div class=\"row\">\r\n" +
                                "       <div class=\"col-xxl-8 col-xl-10 mx-auto\">\r\n" +
                                "           <div class=\"shadow-sm bg-white p-3 p-lg-4 rounded text-left\">\r\n" +
                                "               <div class=\"mb-4\">\r\n" +
                                "                   <div class=\"row gutters-5 d-none d-lg-flex border-bottom mb-3 pb-3\">\r\n" +
                                "                       <div class=\"col-md-5 fw-600\">Sản phẩm</div>\r\n" +
                                "                       <div class=\"col fw-600\">Giá bán</div>\r\n" +
                                "                       <div class=\"col fw-600\">Thuế</div>\r\n" +
                                "                       <div class=\"col fw-600\">Số lượng</div>\r\n" +
                                "                       <div class=\"col fw-600\">TỔNG TIỀN</div>\r\n" +
                                "                       <div class=\"col-auto fw-600\">Xóa</div>\r\n" +
                                "                   </div>\r\n" +
                                "                   <ul class=\"list-group list-group-flush\">\r\n");
                    foreach (var item in cartItems)
                    {
                        html.Append(
                                    "                       <li class=\"list-group-item px-0 px-lg-3\">\r\n" +
                                    "                           <div class=\"row gutters-5\">\r\n" +
                                    "                               <div class=\"col-lg-5 d-flex\">\r\n" +
                                    "                                   <span class=\"mr-2 ml-0\">\r\n" +
                                    "                                       <img src=\""+ item.Image.Split('|')[0] +"\" class=\"img-fit size-60px rounded\" alt=\""+ item.Name +"\">\r\n" +
                                    "                                   </span>\r\n" +
                                    "                                   <span class=\"fs-14 opacity-60\">"+ item.Name +"</span>\r\n" +
                                    "                               </div>\r\n\r\n" +
                                    "                               <div class=\"col-lg col-4 order-1 order-lg-0 my-3 my-lg-0\">\r\n" +
                                    "                                   <span class=\"opacity-60 fs-12 d-block d-lg-none\">Giá bán</span>\r\n" +
                                    "                                   <span class=\"fw-600 fs-16\">"+ item.Price.ToString("N0")+" " + item.UnitPrice +"</span>\r\n" +
                                    "                               </div>\r\n" +
                                    "                               <div class=\"col-lg col-4 order-2 order-lg-0 my-3 my-lg-0\">\r\n" +
                                    "                                   <span class=\"opacity-60 fs-12 d-block d-lg-none\">Thuế</span>\r\n" +
                                    "                                   <span class=\"fw-600 fs-16\">0 VND</span>\r\n" +
                                    "                               </div>\r\n\r\n" +
                                    "                               <div class=\"col-lg col-6 order-4 order-lg-0\">\r\n" +
                                    "                                   <div class=\"row no-gutters align-items-center aiz-plus-minus mr-2 ml-0\">\r\n" +
                                    "                                       <span class=\"fw-600 fs-16\">"+ item.QuantityInCart +"</span>\r\n" +
                                    "                                   </div>\r\n" +
                                    "                               </div>\r\n" +
                                    "                               <div class=\"col-lg col-4 order-3 order-lg-0 my-3 my-lg-0\">\r\n" +
                                    "                                   <span class=\"opacity-60 fs-12 d-block d-lg-none\">TỔNG TIỀN</span>\r\n" +
                                    "                                   <span class=\"fw-600 fs-16 text-primary\"><span id=\"chosen_price\">"+ item.TotalPrice.ToString("N0") +"</span> "+ item.UnitPrice +"</span>\r\n" +
                                    "                               </div>\r\n" +
                                    "                               <div class=\"col-lg-auto col-6 order-5 order-lg-0 text-right\">\r\n" +
                                    "                                   <a href=\"javascript:void(0)\" onclick=\"removeFromCart("+ item.Id +")\" class=\"btn btn-icon btn-sm btn-soft-primary btn-circle\">\r\n" +
                                    "                                       <i class=\"las la-trash\"></i>\r\n" +
                                    "                                   </a>\r\n" +
                                    "                               </div>\r\n" +
                                    "                           </div>\r\n" +
                                    "                       </li>\r\n");
                    }
                        html.Append(
                                "                   </ul>\r\n" +
                                "               </div>\r\n" +
                                "               <div class=\"px-3 py-2 mb-4 border-top d-flex justify-content-between\">\r\n" +
                                "                   <span class=\"opacity-60 fs-15\">Tạm tính</span>\r\n" +
                                "                   <span class=\"fw-600 fs-17\">" + totalPrice + " VND</span>\r\n" +
                                "               </div>\r\n" +
                                "               <div class=\"row align-items-center\">\r\n" +
                                "                   <div class=\"col-md-6 text-center text-md-left order-1 order-md-0\">\r\n" +
                                "                       <a href=\"/\" class=\"btn btn-link\">\r\n" +
                                "                           <i class=\"las la-arrow-left\"></i>\r\n                                    Quay lại cửa hàng\r\n" +
                                "                       </a>\r\n" +
                                "                   </div>\r\n" +
                                "                   <div class=\"col-md-6 text-center text-md-right\">\r\n" +
                                "                       <button class=\"btn btn-primary fw-600\" onclick=\"window.location.href='/checkout'\">Nhập địa chỉ nhận hàng »</button>\r\n" +
                                "                   </div>\r\n" +
                                "               </div>\r\n" +
                                "           </div>\r\n" +
                                "       </div>\r\n" +
                                "   </div>\r\n" +
                                "</div>");
                                                                                                                                                                        
                }
                else
                {
                    htmlCart.Append("<a href=\"javascript:void(0)\" class=\"d-flex align-items-center text-reset h-100\" data-toggle=\"dropdown\" data-display=\"static\" aria-expanded=\"true\">\r\n" +
                                "       <i class=\"la la-shopping-cart la-2x opacity-80\"></i>\r\n" +
                                "       <span class=\"flex-grow-1 ml-1\">\r\n" +
                                "           <span class=\"badge badge-primary badge-inline badge-pill cart-count\">0</span>\r\n" +
                                "           <span class=\"nav-box-text d-none d-xl-block opacity-70\">Giỏ hàng</span>\r\n" +
                                "       </span>\r\n" +
                                "   </a>");
                    htmlCart.Append("<div class=\"dropdown-menu dropdown-menu-right dropdown-menu-lg p-0 stop-propagation\" aria-labelledby=\"dropdownMenuButton\">\r\n" +
                                    "   <div class=\"text-center p-3\">\r\n" +
                                    "       <i class=\"las la-frown la-3x opacity-60 mb-3\"></i>\r\n" +
                                    "       <h3 class=\"h6 fw-700\">Chưa có sản phẩm nào trong giỏ hàng</h3>\r\n" +
                                    "   </div>\r\n" +
                                    "</div>");
                    html.Append("<div class=\"container\">\r\n" +
                                "   <div class=\"row\">\r\n" +
                                "       <div class=\"col-xl-8 mx-auto\">\r\n" +
                                "           <div class=\"shadow-sm bg-white p-4 rounded\">\r\n" +
                                "               <div class=\"text-center p-3\">\r\n" +
                                "                   <i class=\"las la-frown la-3x opacity-60 mb-3\"></i>\r\n" +
                                "                   <h3 class=\"h4 fw-700\">Chưa có sản phẩm nào trong giỏ hàng</h3>\r\n" +
                                "               </div>\r\n" +
                                "           </div>\r\n" +
                                "       </div>\r\n" +
                                "   </div>\r\n" +
                                "</div>");
                }
                var results = new
                {
                    cart_view = html.ToString(),
                    nav_cart_view = htmlCart.ToString(),
                    cart_count = cartItems.Count()
                };
                return Ok(results);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
    public class DataAddToCart
    {
        public string Name { get; set; }
        public string Value { get; set; }
    }
    public class CartItem
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public decimal Price { get; set; }
        public decimal TotalPrice { get; set; }
        public string UnitPrice { get; set; }
        public string Unit { get; set; }
        public string Category { get; set; }
        public string Image { get; set; }
        public int Quantity { get; set; }
        public int QuantityInCart { get; set; }
        public string? Company { get; set; }
        public string? Slug { get; set; }
    }

}
