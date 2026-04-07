using System.ComponentModel.DataAnnotations;

namespace ECommerce_API_2.Models
{
    public class Category
    {
        [Key]
        public int Id { get; set; }
        [Required]
        [Length(3, 50)]
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        [Required]
        public bool Status { get; set; }
    }
}
