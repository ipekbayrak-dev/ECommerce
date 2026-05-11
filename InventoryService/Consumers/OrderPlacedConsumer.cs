using Ecommerce.Messaging.Events;
using InventoryService.Data;
using InventoryService.Services;
using MassTransit;
using Microsoft.EntityFrameworkCore;

namespace InventoryService.Consumers
{
    public class OrderPlacedConsumer : IConsumer<OrderPlacedEvent>
    {
        private readonly IInventoryManagementService _inventoryManagementService;
        private readonly InventoryDbContext _inventoryDbContext;
        private readonly ILogger<OrderPlacedConsumer> _logger;
        public OrderPlacedConsumer(
            IInventoryManagementService inventoryManagementService,
            InventoryDbContext inventoryDbContext,
            ILogger<OrderPlacedConsumer> logger)
        {
            _inventoryManagementService = inventoryManagementService;
            _inventoryDbContext = inventoryDbContext;
            _logger = logger;
        }

        public Task Consume(ConsumeContext<OrderPlacedEvent> context)
        {
            var messageId = context.MessageId?.ToString() ?? "n/a";
            var correlationId = context.CorrelationId?.ToString() ?? "n/a";

            if (context.Message.Items == null || !context.Message.Items.Any())
            {
                _logger.LogWarning(
                    "OrderPlacedEvent for OrderId {OrderId} contains no items. Skipping inventory adjustment. MessageId {MessageId}, CorrelationId {CorrelationId}.",
                    context.Message.OrderId,
                    messageId,
                    correlationId);
                return Task.CompletedTask;
            }

            if (context.Message.Items.Any(item => item.ProductId <= 0 || item.Quantity <= 0))
            {
                _logger.LogWarning(
                    "OrderPlacedEvent for OrderId {OrderId} contains invalid item data. Skipping inventory adjustment. MessageId {MessageId}, CorrelationId {CorrelationId}.",
                    context.Message.OrderId,
                    messageId,
                    correlationId);
                return Task.CompletedTask;
            }

            return ConsumeInternalAsync(context);
        }

        private async Task ConsumeInternalAsync(ConsumeContext<OrderPlacedEvent> context)
        {
            var messageId = context.MessageId?.ToString() ?? "n/a";
            var correlationId = context.CorrelationId?.ToString() ?? "n/a";

            if (await _inventoryDbContext.ProcessedOrderEvents
                .AsNoTracking()
                .AnyAsync(x => x.OrderId == context.Message.OrderId, context.CancellationToken))
            {
                _logger.LogWarning(
                    "OrderId {OrderId} was already processed for inventory. Skipping duplicate message. MessageId {MessageId}, CorrelationId {CorrelationId}.",
                    context.Message.OrderId,
                    messageId,
                    correlationId);
                return;
            }

            await using var tx = await _inventoryDbContext.Database.BeginTransactionAsync(context.CancellationToken);

            foreach (var item in context.Message.Items)
            {
                await _inventoryManagementService.AdjustStockAsync(item.ProductId, new Dtos.AdjustStockRequest
                {
                    Delta = -item.Quantity
                });
            }

            _inventoryDbContext.ProcessedOrderEvents.Add(new Models.ProcessedOrderEvent
            {
                OrderId = context.Message.OrderId,
                ProcessedAtUtc = DateTime.UtcNow
            });

            try
            {
                await _inventoryDbContext.SaveChangesAsync(context.CancellationToken);
                await tx.CommitAsync(context.CancellationToken);
            }
            catch (DbUpdateException)
            {
                _logger.LogWarning(
                    "OrderId {OrderId} was already marked processed due to concurrent delivery. Skipping duplicate message. MessageId {MessageId}, CorrelationId {CorrelationId}.",
                    context.Message.OrderId,
                    messageId,
                    correlationId);
                await tx.RollbackAsync(context.CancellationToken);
                return;
            }

            _logger.LogInformation(
                "OrderPlacedConsumer processed OrderId {OrderId} and adjusted inventory for {ItemCount} item(s). MessageId {MessageId}, CorrelationId {CorrelationId}.",
                context.Message.OrderId,
                context.Message.Items.Count,
                messageId,
                correlationId);
        }
    }
}