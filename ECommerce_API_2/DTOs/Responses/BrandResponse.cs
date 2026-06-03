using System.Security.Cryptography.Pkcs;

namespace ECommerce_API_2.DTOs.Responses
{
    public class BrandResponse
    {
        public List<Brand> Brand { get; set; } = new List<Brand>();
        public double TotalPages { get; set; }
        public int CurrentPage { get; set; }

    }
}
