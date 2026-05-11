using MassTransit;

namespace PaymentService.Consumers
{
    public class OrderPlacedConsumerDefinition : ConsumerDefinition<OrderPlacedConsumer>
    {
        public OrderPlacedConsumerDefinition()
        {
            EndpointName = "payment-order-placed";
        }
    }
}
