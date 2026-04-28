namespace PaymentService.Models
{
    public class Payment
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int OrderId { get; set; }
        public string? StripePaymentIntentId { get; set; }
        public decimal Amount { get; set; }
        public required string Method { get; set; }
        public DateTime Date { get; set; }
        public PaymentStatus Status { get; set; } = PaymentStatus.Pending;
    }
}