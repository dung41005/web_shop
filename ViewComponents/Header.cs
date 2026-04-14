using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using UC.eComm.Publish.Context;
using UC.eComm.Publish.Controllers;
using UC.eComm.Publish.Model;
using UC.eComm.Publish.Services;
using UC.eComm.Publish.ViewModels;
using UC.eComm.Publish.ViewModels.Shared;

namespace UC.eComm.Publish.ViewComponents
{
    [ViewComponent]
    public class Header : ViewComponent
    {
        private readonly MyDbContext _context;
        private readonly JsonService _jsonService;
        public Header(MyDbContext context, JsonService jsonService)
        {
            _context = context;
            _jsonService = jsonService;
        }
        public IViewComponentResult Invoke()
        {
            List<CartItem> cartItems = new List<CartItem>();
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
            model.Total = cartItems.Count;
            
            model.cartItems = cartItems;
            model.TotalPrice = totalPrice;
            
            return View(model);
        }
    }
}
