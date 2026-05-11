using MassTransit;

namespace InventoryService.Consumers
{
    public class OrderPlacedConsumerDefinition : ConsumerDefinition<OrderPlacedConsumer>
    {
        public OrderPlacedConsumerDefinition()
        {
            EndpointName = "inventory-order-placed";
        }
    }
}
