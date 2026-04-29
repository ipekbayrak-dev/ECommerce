using PaymentService.Dtos;

namespace PaymentService.Services
{
    public interface IPaymentManagementService
    {
        public Task<PaymentResponse> CreatePaymentAsync(CreatePaymentRequest request);
        public Task<PaymentResponse> GetByIdAsync(int id);
        public Task<PaymentResponse> GetByOrderIdAsync(int orderId);
        public Task HandleWebHookAsync(string stripeEventJson, string stripeSignature);
    }
}