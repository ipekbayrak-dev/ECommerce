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
            var messageId = context.MessageId?.ToString() ?? "n/a";
            var correlationId = context.CorrelationId?.ToString() ?? "n/a";

            if (await _paymentManagementService.FindByOrderIdAsync(context.Message.OrderId) is not null)
            {
                _logger.LogWarning(
                    "Payment for order {OrderId} already exists. Skipping. MessageId {MessageId}, CorrelationId {CorrelationId}.",
                    context.Message.OrderId,
                    messageId,
                    correlationId);
                 return;
            }

            await _paymentManagementService.CreatePaymentAsync(new CreatePaymentRequest()
            {
                UserId = context.Message.UserId,
                OrderId = context.Message.OrderId,
                Amount = context.Message.Amount,
                Method = "Automatic"
            });

            _logger.LogInformation(
                "Payment created from OrderPlacedEvent for OrderId {OrderId}. MessageId {MessageId}, CorrelationId {CorrelationId}.",
                context.Message.OrderId,
                messageId,
                correlationId);
        }
    }
}