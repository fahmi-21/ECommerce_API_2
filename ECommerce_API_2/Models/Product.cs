using System.ComponentModel.DataAnnotations;

namespace ECommerce_API_2.Models
{
    public class Product
    {
        [Key]
        public int Id { get; set; }
        [Required(ErrorMessage = "Name is required")]
        [Length(5, 250)]
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        [Required(ErrorMessage = "Price is required")]
        [Range(1, 10000, ErrorMessage = "Price must be between 1 and 10000")]
        public double Price { get; set; }
        public double Discount { get; set; }
        public string Img { get; set; } = string.Empty;
        [Required]
        public DateTime Date { get; set; }
        public double Rate { get; set; }
        [Required]
        public bool Status { get; set; }
        [Required]
        public int Quantity { get; set; } = 0;
        [Required]
        public int CategoryId { get; set; }
        public Category? Category { get; set; }
        public int BrandId {  get; set; }
        public Brand? Brand { get; set; }
    }
}
