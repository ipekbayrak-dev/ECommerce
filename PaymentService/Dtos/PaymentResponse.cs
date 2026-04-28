using PaymentService.Models;

namespace PaymentService.Dtos
{
    public class PaymentResponse
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int OrderId { get; set; }
        public string? StripePaymentIntentId { get; set; }
        public string? ClientSecret { get; set; }
        public decimal Amount { get; set; }
        public DateTime Date { get; set; }
        public string Method { get; set; } = null!;
        public PaymentStatus Status { get; set; }
    }
}