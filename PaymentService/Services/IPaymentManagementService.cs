using PaymentService.Dtos;

namespace PaymentService.Services
{
    public interface IPaymentManagementService
    {
        public Task<PaymentResponse> CreatePaymentAsync(CreatePaymentRequest request);
        public Task<PaymentResponse> GetPaymentByIdAsync(int id);
        public Task HandleWebHookAsync(string stripeEventJson, string stripeSignature);
    }
}