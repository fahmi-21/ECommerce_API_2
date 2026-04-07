namespace ECommerce_API_2.DTOs.Requests
{
    public class LoginRequest
    {
        [Required]
        [Display(Name = "Email Or UserName")]
        public string EmailOrUserName { get; set; } = string.Empty;
        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;
        public bool RemeberMe {  get; set; }

    }
}