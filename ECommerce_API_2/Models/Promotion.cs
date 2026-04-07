namespace ECommerce_API_2.Models
{
    public class Promotion
    {
        public int Id { get; set; }
        public string Code { get; set; } = string.Empty;
        public double Discount { get; set; }
        public int MaxUsage { get; set; } = 1;
        public DateTime CreateAt { get; set; } = DateTime.UtcNow;
        public DateTime ExpireDate { get; set; } = DateTime.UtcNow.AddDays(7);
        public bool IsValid => MaxUsage >= 1 && DateTime.UtcNow < ExpireDate;

        public string? ApplicationUserId { get; set; }
        public ApplicationUser? ApplicationUser { get; set; }
        public int ProductId { get; set; }
        public Product? Product { get; set; }
    }
}
