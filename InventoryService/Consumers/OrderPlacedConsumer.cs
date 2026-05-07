using Ecommerce.Messaging.Events;
using MassTransit;

namespace InventoryService.Consumers
{
    public class OrderPlacedConsumer : IConsumer<OrderPlacedEvent>
    {
        public Task Consume(ConsumeContext<OrderPlacedEvent> context)
        {
            throw new NotImplementedException();
        }
    }
}