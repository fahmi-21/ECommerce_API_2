namespace ECommerce_API_2.DTOs.Requests
{
    public class ValidateOTPRequest
    {
        public string Email { get; set; } = string.Empty;
        public string OTP { get; set; } = string.Empty;
    }
}
