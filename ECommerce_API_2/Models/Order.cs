namespace ECommerce_API_2.Models
{
    public enum OrderStatus
    {
        Pending,
        inproccesssing,
        shipped,
        ontheway,
        completed,
        cancelled
    }
    public enum PaymentStatus
    {
        pending,
        Completed,
        cancelled,
        refunded
    }
    public enum PaymentType
    {
        Visa,
        Cash
    }
    public class Order
    {
        public int Id { get; set; }
        public string ApplicationUserId { get; set; } = string.Empty;
        public ApplicationUser User { get; set; } = null!;
        public DateTime OrderDate { get; set; }
        public DateTime? ShippedDate {  get; set; }
        public string? Carrier {  get; set; }
        public string? TrakingNumber { get; set; }
        public OrderStatus Status { get; set; } = OrderStatus.Pending;
        public string SessionId { get; set; } = string.Empty;
        public string? PaymentId { get; set; }
        public PaymentStatus PaymentStatus { get; set; } = PaymentStatus.pending;
        public PaymentType PaymentType { get; set; } = PaymentType.Visa;
        public double TotalPrice { get; set; }
    }
}
