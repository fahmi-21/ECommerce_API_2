namespace ECommerce_API_2.DTOs.Requests
{
    public class BrandUpdateRequest
    {
        public string Name { get; set; } = string.Empty;
        public IFormFile? Logo { get; set; }
        public bool status { get; set; }
    }
}
