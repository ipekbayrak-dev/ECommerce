using Microsoft.EntityFrameworkCore;
using PaymentService.Data;
using PaymentService.Dtos;
using PaymentService.Models;

namespace PaymentService.Services
{
    public class PaymentManagementService : IPaymentManagementService
    {
        private readonly PaymentDbContext _paymentDbContext;
        public PaymentManagementService(PaymentDbContext paymentDbContext)
        {
            _paymentDbContext = paymentDbContext;
        }
        private static PaymentResponse MapToResponse(Payment payment)
        {
            return new PaymentResponse
            {
                Id = payment.Id,
                UserId = payment.UserId,
                OrderId = payment.OrderId,
                StripePaymentIntentId = payment.StripePaymentIntentId,
                Amount = payment.Amount,
                Date = payment.Date,
                Method = payment.Method,
                Status = payment.Status
            };
        }
        public async Task<PaymentResponse> CreatePaymentAsync(CreatePaymentRequest request)
        {
            if (request.UserId <= 0)
                throw new ArgumentException("Invalid User ID.", nameof(request.UserId));

            if (request.OrderId <= 0)
                throw new ArgumentException("Invalid Order ID.", nameof(request.OrderId));

            if (request.Amount <= 0)
                throw new ArgumentException("Amount must be greater than zero.", nameof(request.Amount));

            if (string.IsNullOrWhiteSpace(request.Method))
                throw new ArgumentException("Payment method is required.", nameof(request.Method));

            Payment payment = new Payment
            {
                UserId = request.UserId,
                OrderId = request.OrderId,
                Method = request.Method,
                Amount = request.Amount,
                Date = DateTime.UtcNow,
                StripePaymentIntentId = null,
                Status = PaymentStatus.Pending
            };

            _paymentDbContext.Add(payment);
            await _paymentDbContext.SaveChangesAsync();

            return MapToResponse(payment);
        }

        public async Task<PaymentResponse> GetPaymentByIdAsync(int id)
        {
            var payment = await _paymentDbContext.Payments.SingleOrDefaultAsync(p => p.Id == id);
            if (payment is null)
            {
                throw new KeyNotFoundException($"Payment with ID {id} not found.");
            }
            return MapToResponse(payment);
        }

        public Task HandleWebHookAsync(string stripeEventJson, string stripeSignature)
        {
            throw new NotImplementedException();
        }
    }
}