namespace ECommerce_API_2.DTOs.Responses
{
    public class ProductResponse
    {

        public List<Product> Products { get; set; } = new List<Product>();
        public double TotalPages { get; set; }
        public int CurrentPage { get; set; }
    }
}
