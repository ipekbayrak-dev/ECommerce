using Ecommerce.Messaging.Events;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace InventoryService.Consumers
{
    public class OrderPlacedConsumer(ILogger<OrderPlacedConsumer> logger) : IConsumer<OrderPlacedEvent>
    {
        public Task Consume(ConsumeContext<OrderPlacedEvent> context)
        {
            // Stock decrement will be implemented in Step 8 when OrderPlacedEvent carries item-level data.
            logger.LogInformation("OrderPlacedConsumer received OrderId {OrderId} — skipping until item payload is available.", context.Message.OrderId);
            return Task.CompletedTask;
        }
    }
}