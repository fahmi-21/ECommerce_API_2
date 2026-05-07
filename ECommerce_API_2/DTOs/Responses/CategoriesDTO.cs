namespace ECommerce_API_2.DTOs.Responses
{
    public class CategoriesDTO
    {
        public List<Category> category { get; set; } = new();
        public double totalPages { get; set; }
        public int CurrentPage { get; set; }
    }
}
