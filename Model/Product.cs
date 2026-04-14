namespace UC.eComm.Publish.Model
{
    public class Product
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public decimal Price { get; set; }
        public string UnitPrice { get; set; }
        public string Unit { get; set; }
        public string Category { get; set; }
        public string Image { get; set; }
        public int Quantity { get; set; }
        public string? Slug { get; set; }
        public string Feedback { get; set; }
        public string Company { get; set; }
        public string CompanyAddress { get; set; }
    }
    
}
