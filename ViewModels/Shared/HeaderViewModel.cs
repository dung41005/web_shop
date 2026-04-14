using UC.eComm.Publish.Controllers;

namespace UC.eComm.Publish.ViewModels.Shared
{
    public class HeaderViewModel
    {
        public int Total { get; set; }
        public decimal TotalPrice { get; set; }
        public List<CartItem> cartItems { get; set; } = new List<CartItem>();
    }
}
