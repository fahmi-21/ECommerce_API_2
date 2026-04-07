namespace ECommerce_API_2.Models
{
    public class ProductSubImg
    {
        public int Id { get; set; }
        public string Img {  get; set; } = string.Empty;
        public int ProductId { get; set; }
        public Product Product { get; set; } = default!;
    }
}
