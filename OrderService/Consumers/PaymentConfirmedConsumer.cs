using Ecommerce.Messaging.Events;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using OrderService.Data;
using OrderService.Models;

namespace OrderService.Consumers
{
    public class PaymentConfirmedConsumer : IConsumer<PaymentConfirmedEvent>
    {
        private readonly OrderDbContext _orderDbContext;
        private readonly ILogger<PaymentConfirmedConsumer> _logger;

        public PaymentConfirmedConsumer(OrderDbContext orderDbContext, ILogger<PaymentConfirmedConsumer> logger)
        {
            _orderDbContext = orderDbContext;
            _logger = logger;
        }

        public async Task Consume(ConsumeContext<PaymentConfirmedEvent> context)
        {
            var evt = context.Message;

            var order = await _orderDbContext.Orders
                .FirstOrDefaultAsync(o => o.Id == evt.OrderId);

            if (order is null)
            {
                _logger.LogWarning("PaymentConfirmedConsumer: Order {OrderId} not found. Skipping.", evt.OrderId);
                return;
            }

            if (order.OrderStatus == OrderStatus.Paid)
            {
                _logger.LogInformation("PaymentConfirmedConsumer: Order {OrderId} already Paid. Skipping.", evt.OrderId);
                return;
            }

            order.OrderStatus = OrderStatus.Paid;
            await _orderDbContext.SaveChangesAsync();

            _logger.LogInformation("PaymentConfirmedConsumer: Order {OrderId} marked as Paid.", evt.OrderId);
        }
    }
}
