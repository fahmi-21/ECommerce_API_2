namespace ECommerce_API_2.DTOs.Requests
{
    public class BrandCreateRequest
    {
        public string Name { get; set; } = string.Empty;
        public IFormFile Logo { get; set; } = null!;
        public bool status { get; set; }
    }
}
