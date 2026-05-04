using Ecommerce.Messaging.Events;
using MassTransit;
using PaymentService.Services;
using PaymentService.Dtos;

namespace PaymentService.Consumers
{
    public class OrderPlacedConsumer : IConsumer<OrderPlacedEvent>
    {
        private readonly IPaymentManagementService _paymentManagementService;
        private readonly ILogger<OrderPlacedConsumer> _logger;

        public OrderPlacedConsumer(IPaymentManagementService paymentManagementService, ILogger<OrderPlacedConsumer> logger)
        {
            _paymentManagementService = paymentManagementService;
            _logger = logger;
        }
        public async Task Consume(ConsumeContext<OrderPlacedEvent> context)
        {
            if (await _paymentManagementService.FindByOrderIdAsync(context.Message.OrderId) is not null)
            {
                _logger.LogInformation("Payment for order {OrderId} already exists. Skipping.", context.Message.OrderId);
                 return;
            }

            await _paymentManagementService.CreatePaymentAsync(new CreatePaymentRequest()
            {
                UserId = context.Message.UserId,
                OrderId = context.Message.OrderId,
                Amount = context.Message.Amount,
                Method = "Automatic"
            });
        }
    }
}