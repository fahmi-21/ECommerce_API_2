namespace ECommerce_API_2.DTOs.Responses
{
    public class SuccessRespones
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public DateTime UTCDate { get; set; } = DateTime.UtcNow;
        public string Msg { get; set; } = string.Empty;
        public List<string>? OptinalData { get; set; }
        public DateTime ExpireIn { get; set; }
    }
}
