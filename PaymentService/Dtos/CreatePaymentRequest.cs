using PaymentService.Models;

namespace PaymentService.Dtos
{
    public class CreatePaymentRequest
    {
        public int UserId { get; set; }
        public int OrderId { get; set; }
        public decimal Amount { get; set; }
        public required string Method { get; set; }
    }
}