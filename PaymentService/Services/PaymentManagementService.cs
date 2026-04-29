using Microsoft.EntityFrameworkCore;
using PaymentService.Data;
using PaymentService.Dtos;
using PaymentService.Models;
using Stripe;

namespace PaymentService.Services
{
    public class PaymentManagementService : IPaymentManagementService
    {
        private readonly PaymentDbContext _paymentDbContext;
        private readonly string _currency;
        private readonly string _webhookSecret;
        public PaymentManagementService(PaymentDbContext paymentDbContext, IConfiguration configuration)
        {
            _paymentDbContext = paymentDbContext;
            _currency = configuration["Stripe:Currency"]!;
            _webhookSecret = configuration["Stripe:WebhookSecret"]!;
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

            var options = new PaymentIntentCreateOptions
            {
                Amount = (long)(request.Amount * 100),
                Currency = _currency,
                Metadata = new Dictionary<string, string>
        {
            { "orderId", request.OrderId.ToString() },
            { "userId", request.UserId.ToString() }
        }
            };

            var service = new PaymentIntentService();
            var paymentIntent = await service.CreateAsync(options);

            var payment = new Payment
            {
                UserId = request.UserId,
                OrderId = request.OrderId,
                Method = request.Method,
                Amount = request.Amount,
                Date = DateTime.UtcNow,
                StripePaymentIntentId = paymentIntent.Id,
                Status = PaymentStatus.Pending
            };

            _paymentDbContext.Add(payment);
            await _paymentDbContext.SaveChangesAsync();

            var response = MapToResponse(payment);
            response.ClientSecret = paymentIntent.ClientSecret;
            return response;
        }

        public async Task<PaymentResponse> GetByIdAsync(int id)
        {
            var payment = await _paymentDbContext.Payments.SingleOrDefaultAsync(p => p.Id == id);
            if (payment is null)
            {
                throw new KeyNotFoundException($"Payment with ID {id} not found.");
            }
            return MapToResponse(payment);
        }

        public async Task<PaymentResponse> GetByOrderIdAsync(int orderId)
        {
            var payment = await _paymentDbContext.Payments.SingleOrDefaultAsync(p => p.OrderId == orderId);
            if (payment is null)
            {
                throw new KeyNotFoundException($"Payment for order ID {orderId} not found.");
            }
            return MapToResponse(payment);
        }

        public async Task HandleWebHookAsync(string stripeEventJson, string stripeSignature)
        {
            Event stripeEvent;
            try
            {
                stripeEvent = EventUtility.ConstructEvent(stripeEventJson, stripeSignature, _webhookSecret);
            }
            catch (StripeException ex)
            {
                throw new InvalidOperationException("Webhook signature verification failed.", ex);
            }

            if (stripeEvent.Data.Object is not PaymentIntent paymentIntent)
                return;

            var payment = await _paymentDbContext.Payments
                .SingleOrDefaultAsync(p => p.StripePaymentIntentId == paymentIntent.Id);

            if (payment is null)
                return;

            payment.Status = stripeEvent.Type switch
            {
                "payment_intent.succeeded" => PaymentStatus.Completed,
                "payment_intent.payment_failed" => PaymentStatus.Failed,
                "payment_intent.canceled" => PaymentStatus.Cancelled,
                _ => payment.Status
            };

            await _paymentDbContext.SaveChangesAsync();
        }
    }
}