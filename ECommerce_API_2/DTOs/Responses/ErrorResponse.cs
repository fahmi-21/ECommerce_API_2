namespace ECommerce_API_2.DTOs.Responses
{
    public class ErrorResponse
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public DateTime UTCDate { get; set; } = DateTime.UtcNow;
        public string ErorMsg { get; set; } = string.Empty;
        public int StatusCode { get; set; }
    }
}
