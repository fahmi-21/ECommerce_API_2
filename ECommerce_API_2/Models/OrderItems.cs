namespace ECommerce_API_2.Models
{
    public class OrderItems
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public Product Product { get; set; }
        public int OrderId { get; set; }
        public Order Order { get; set; }
        public double? CurrentPrice { get; set; }
        public double? Discount { get; set; }
        public int? Count { get; set; }
    }
}
