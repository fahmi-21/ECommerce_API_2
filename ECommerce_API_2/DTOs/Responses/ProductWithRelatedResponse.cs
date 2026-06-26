namespace ECommerce_API_2.DTOs.Responses
{
    public class ProductWithRelatedResponse
    {
        public Product Product { get; set; } = new Product();
        public List<Product> SameCategories { get; set; } = new List<Product>();
        public List<Product> SamePrice { get; set; } = new List<Product>();
        public List<Product> RelatedProducts { get; set; } = new List<Product>();
    }
}
