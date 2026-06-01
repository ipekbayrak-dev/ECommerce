using Ecommerce.Messaging.Events;
using MassTransit;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using OrderService.Consumers;
using OrderService.Data;
using OrderService.Dtos;
using OrderService.Models;
using OrderService.Services;
using Xunit;

namespace OrderService.Tests;

public class OrderManagementServiceTests
{
    private static OrderDbContext CreateDb(string dbName)
    {
        var options = new DbContextOptionsBuilder<OrderDbContext>()
            .UseInMemoryDatabase(dbName)
            .Options;
        return new OrderDbContext(options);
    }

    private static OrderManagementService BuildService(OrderDbContext db, IPublishEndpoint? publisher = null)
    {
        var pub = publisher ?? Mock.Of<IPublishEndpoint>();
        var httpCtx = Mock.Of<IHttpContextAccessor>(a => a.HttpContext == null);
        return new OrderManagementService(db, pub, httpCtx);
    }

    private static CreateOrderRequest ValidRequest(int userId = 1) => new()
    {
        UserId = userId,
        Items =
        [
            new CreateOrderItemRequest
            {
                ProductId = 10,
                ProductName = "Widget",
                Quantity = 2,
                UnitPrice = 50m,
                Discount = 0
            }
        ]
    };

    // ── CreateAsync ────────────────────────────────────────────────────────

    [Fact]
    public async Task CreateAsync_ValidRequest_SavesOrderAndPublishesEvent()
    {
        using var db = CreateDb(nameof(CreateAsync_ValidRequest_SavesOrderAndPublishesEvent));
        var publisher = new Mock<IPublishEndpoint>();
        var svc = BuildService(db, publisher.Object);

        var result = await svc.CreateAsync(ValidRequest());

        Assert.Equal(1, result.UserId);
        Assert.Equal(OrderStatus.Pending, result.OrderStatus);
        Assert.Equal(100m, result.Total);
        Assert.Single(result.Items);
        // MassTransit extension methods route through the 3-arg interface overload (message, pipe, token)
        publisher.Verify(p => p.Publish(
            It.IsAny<OrderPlacedEvent>(),
            It.IsAny<IPipe<PublishContext<OrderPlacedEvent>>>(),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreateAsync_CalculatesTotal_WithDiscount()
    {
        using var db = CreateDb(nameof(CreateAsync_CalculatesTotal_WithDiscount));
        var svc = BuildService(db);

        var request = new CreateOrderRequest
        {
            UserId = 1,
            Items =
            [
                new CreateOrderItemRequest { ProductId = 1, ProductName = "A", Quantity = 4, UnitPrice = 100m, Discount = 0.25m }
            ]
        };

        var result = await svc.CreateAsync(request);

        // 4 * 100 * (1 - 0.25) = 300
        Assert.Equal(300m, result.Total);
    }

    [Fact]
    public async Task CreateAsync_NullRequest_ThrowsArgumentNullException()
    {
        using var db = CreateDb(nameof(CreateAsync_NullRequest_ThrowsArgumentNullException));
        var svc = BuildService(db);

        await Assert.ThrowsAsync<ArgumentNullException>(() => svc.CreateAsync(null!));
    }

    [Fact]
    public async Task CreateAsync_InvalidUserId_ThrowsArgumentOutOfRangeException()
    {
        using var db = CreateDb(nameof(CreateAsync_InvalidUserId_ThrowsArgumentOutOfRangeException));
        var svc = BuildService(db);

        var req = ValidRequest();
        req.UserId = 0;

        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => svc.CreateAsync(req));
    }

    [Fact]
    public async Task CreateAsync_EmptyItems_ThrowsArgumentException()
    {
        using var db = CreateDb(nameof(CreateAsync_EmptyItems_ThrowsArgumentException));
        var svc = BuildService(db);

        var req = new CreateOrderRequest { UserId = 1, Items = [] };

        await Assert.ThrowsAsync<ArgumentException>(() => svc.CreateAsync(req));
    }

    [Theory]
    [InlineData(-0.01)]
    [InlineData(1.01)]
    public async Task CreateAsync_InvalidDiscount_ThrowsArgumentOutOfRangeException(double discount)
    {
        using var db = CreateDb($"discount-{discount}");
        var svc = BuildService(db);

        var req = new CreateOrderRequest
        {
            UserId = 1,
            Items =
            [
                new CreateOrderItemRequest { ProductId = 1, ProductName = "X", Quantity = 1, UnitPrice = 10m, Discount = (decimal)discount }
            ]
        };

        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => svc.CreateAsync(req));
    }

    // ── GetByIdAsync ───────────────────────────────────────────────────────

    [Fact]
    public async Task GetByIdAsync_ExistingOrder_ReturnsOrder()
    {
        using var db = CreateDb(nameof(GetByIdAsync_ExistingOrder_ReturnsOrder));
        var svc = BuildService(db);

        var created = await svc.CreateAsync(ValidRequest());

        var result = await svc.GetByIdAsync(created.Id);

        Assert.Equal(created.Id, result.Id);
    }

    [Fact]
    public async Task GetByIdAsync_NotFound_ThrowsKeyNotFoundException()
    {
        using var db = CreateDb(nameof(GetByIdAsync_NotFound_ThrowsKeyNotFoundException));
        var svc = BuildService(db);

        await Assert.ThrowsAsync<KeyNotFoundException>(() => svc.GetByIdAsync(9999));
    }

    // ── GetByUserIdAsync ───────────────────────────────────────────────────

    [Fact]
    public async Task GetByUserIdAsync_ReturnsOnlyUserOrders()
    {
        using var db = CreateDb(nameof(GetByUserIdAsync_ReturnsOnlyUserOrders));
        var svc = BuildService(db);

        await svc.CreateAsync(ValidRequest(userId: 1));
        await svc.CreateAsync(ValidRequest(userId: 1));
        await svc.CreateAsync(ValidRequest(userId: 2));

        var result = await svc.GetByUserIdAsync(1);

        Assert.Equal(2, result.Count);
        Assert.All(result, o => Assert.Equal(1, o.UserId));
    }

    // ── CancelAsync ────────────────────────────────────────────────────────

    [Fact]
    public async Task CancelAsync_PendingOrder_SetsCancelled()
    {
        using var db = CreateDb(nameof(CancelAsync_PendingOrder_SetsCancelled));
        var svc = BuildService(db);

        var created = await svc.CreateAsync(ValidRequest());

        var result = await svc.CancelAsync(created.Id);

        Assert.Equal(OrderStatus.Cancelled, result.OrderStatus);
    }

    [Fact]
    public async Task CancelAsync_NotFound_ThrowsKeyNotFoundException()
    {
        using var db = CreateDb(nameof(CancelAsync_NotFound_ThrowsKeyNotFoundException));
        var svc = BuildService(db);

        await Assert.ThrowsAsync<KeyNotFoundException>(() => svc.CancelAsync(9999));
    }

    // ── UpdateStatusAsync ──────────────────────────────────────────────────

    [Fact]
    public async Task UpdateStatusAsync_ValidTransition_UpdatesStatus()
    {
        using var db = CreateDb(nameof(UpdateStatusAsync_ValidTransition_UpdatesStatus));
        var svc = BuildService(db);

        var created = await svc.CreateAsync(ValidRequest());

        var result = await svc.UpdateStatusAsync(created.Id, new UpdateOrderRequest { OrderStatus = OrderStatus.Paid });

        Assert.Equal(OrderStatus.Paid, result.OrderStatus);
    }

    // ── GetAllAsync ────────────────────────────────────────────────────────

    [Fact]
    public async Task GetAllAsync_Pagination_ReturnsCorrectPage()
    {
        using var db = CreateDb(nameof(GetAllAsync_Pagination_ReturnsCorrectPage));
        var svc = BuildService(db);

        for (int i = 0; i < 5; i++) await svc.CreateAsync(ValidRequest());

        var page1 = await svc.GetAllAsync(1, 3);
        var page2 = await svc.GetAllAsync(2, 3);

        Assert.Equal(3, page1.Count);
        Assert.Equal(2, page2.Count);
    }
}

public class PaymentConfirmedConsumerTests
{
    private static OrderDbContext CreateDb(string name)
    {
        var options = new DbContextOptionsBuilder<OrderDbContext>()
            .UseInMemoryDatabase(name)
            .Options;
        return new OrderDbContext(options);
    }

    [Fact]
    public async Task Consume_SetsOrderStatusToPaid()
    {
        using var db = CreateDb(nameof(Consume_SetsOrderStatusToPaid));
        var order = new Order
        {
            UserId = 1,
            Date = DateTime.UtcNow,
            OrderStatus = OrderStatus.Pending,
            Total = 100m
        };
        db.Orders.Add(order);
        await db.SaveChangesAsync();

        var consumer = new PaymentConfirmedConsumer(db, NullLogger<PaymentConfirmedConsumer>.Instance);
        var ctx = Mock.Of<ConsumeContext<PaymentConfirmedEvent>>(c =>
            c.Message == new PaymentConfirmedEvent { OrderId = order.Id, UserId = 1, Amount = 100m });

        await consumer.Consume(ctx);

        var updated = await db.Orders.FindAsync(order.Id);
        Assert.Equal(OrderStatus.Paid, updated!.OrderStatus);
    }

    [Fact]
    public async Task Consume_AlreadyPaid_IsIdempotent()
    {
        using var db = CreateDb(nameof(Consume_AlreadyPaid_IsIdempotent));
        var order = new Order { UserId = 1, Date = DateTime.UtcNow, OrderStatus = OrderStatus.Paid, Total = 100m };
        db.Orders.Add(order);
        await db.SaveChangesAsync();

        var consumer = new PaymentConfirmedConsumer(db, NullLogger<PaymentConfirmedConsumer>.Instance);
        var ctx = Mock.Of<ConsumeContext<PaymentConfirmedEvent>>(c =>
            c.Message == new PaymentConfirmedEvent { OrderId = order.Id, UserId = 1, Amount = 100m });

        await consumer.Consume(ctx); // should not throw

        var still = await db.Orders.FindAsync(order.Id);
        Assert.Equal(OrderStatus.Paid, still!.OrderStatus);
    }

    [Fact]
    public async Task Consume_OrderNotFound_DoesNotThrow()
    {
        using var db = CreateDb(nameof(Consume_OrderNotFound_DoesNotThrow));
        var consumer = new PaymentConfirmedConsumer(db, NullLogger<PaymentConfirmedConsumer>.Instance);
        var ctx = Mock.Of<ConsumeContext<PaymentConfirmedEvent>>(c =>
            c.Message == new PaymentConfirmedEvent { OrderId = 9999, UserId = 1, Amount = 100m });

        var ex = await Record.ExceptionAsync(() => consumer.Consume(ctx));
        Assert.Null(ex);
    }
}
