using Ecommerce.Messaging.Events;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Moq;
using PaymentService.Data;
using PaymentService.Models;
using PaymentService.Services;
using Xunit;

namespace PaymentService.Tests;

public class PaymentManagementServiceTests
{
    private static PaymentDbContext CreateDb(string name)
    {
        var options = new DbContextOptionsBuilder<PaymentDbContext>()
            .UseInMemoryDatabase(name)
            .Options;
        return new PaymentDbContext(options);
    }

    private static PaymentManagementService BuildService(PaymentDbContext db, IPublishEndpoint? publisher = null)
    {
        var config = new Mock<IConfiguration>();
        config.Setup(c => c["Stripe:Currency"]).Returns("usd");
        config.Setup(c => c["Stripe:WebhookSecret"]).Returns("whsec_test");

        var pub = publisher ?? Mock.Of<IPublishEndpoint>();
        return new PaymentManagementService(db, config.Object, pub);
    }

    private static async Task<Payment> SeedPayment(PaymentDbContext db, int orderId = 1, int userId = 1,
        PaymentStatus status = PaymentStatus.Pending)
    {
        var payment = new Payment
        {
            OrderId = orderId,
            UserId = userId,
            Amount = 200m,
            Method = "Card",
            Date = DateTime.UtcNow,
            StripePaymentIntentId = "pi_test_123",
            ClientSecret = "pi_test_123_secret_abc",
            Status = status
        };
        db.Payments.Add(payment);
        await db.SaveChangesAsync();
        return payment;
    }

    // ── ConfirmByOrderAsync ────────────────────────────────────────────────

    [Fact]
    public async Task ConfirmByOrderAsync_PendingPayment_SetsCompletedAndPublishesEvent()
    {
        using var db = CreateDb(nameof(ConfirmByOrderAsync_PendingPayment_SetsCompletedAndPublishesEvent));
        var publisher = new Mock<IPublishEndpoint>();
        var svc = BuildService(db, publisher.Object);

        await SeedPayment(db, orderId: 1);

        var result = await svc.ConfirmByOrderAsync(1);

        Assert.Equal(PaymentStatus.Completed, result.Status);
        publisher.Verify(p => p.Publish(
            It.Is<PaymentConfirmedEvent>(e => e.OrderId == 1),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ConfirmByOrderAsync_AlreadyCompleted_IsIdempotent()
    {
        using var db = CreateDb(nameof(ConfirmByOrderAsync_AlreadyCompleted_IsIdempotent));
        var publisher = new Mock<IPublishEndpoint>();
        var svc = BuildService(db, publisher.Object);

        await SeedPayment(db, orderId: 1, status: PaymentStatus.Completed);

        var result = await svc.ConfirmByOrderAsync(1);

        Assert.Equal(PaymentStatus.Completed, result.Status);
        // Should NOT republish — already confirmed
        publisher.Verify(p => p.Publish(It.IsAny<PaymentConfirmedEvent>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ConfirmByOrderAsync_NotFound_ThrowsKeyNotFoundException()
    {
        using var db = CreateDb(nameof(ConfirmByOrderAsync_NotFound_ThrowsKeyNotFoundException));
        var svc = BuildService(db);

        await Assert.ThrowsAsync<KeyNotFoundException>(() => svc.ConfirmByOrderAsync(9999));
    }

    // ── GetByOrderIdAsync ──────────────────────────────────────────────────

    [Fact]
    public async Task GetByOrderIdAsync_Existing_ReturnsPayment()
    {
        using var db = CreateDb(nameof(GetByOrderIdAsync_Existing_ReturnsPayment));
        var svc = BuildService(db);

        var seeded = await SeedPayment(db, orderId: 5);

        var result = await svc.GetByOrderIdAsync(5);

        Assert.Equal(seeded.Id, result.Id);
        Assert.Equal(5, result.OrderId);
    }

    [Fact]
    public async Task GetByOrderIdAsync_NotFound_ThrowsKeyNotFoundException()
    {
        using var db = CreateDb(nameof(GetByOrderIdAsync_NotFound_ThrowsKeyNotFoundException));
        var svc = BuildService(db);

        await Assert.ThrowsAsync<KeyNotFoundException>(() => svc.GetByOrderIdAsync(9999));
    }

    // ── FindByOrderIdAsync ─────────────────────────────────────────────────

    [Fact]
    public async Task FindByOrderIdAsync_Existing_ReturnsPayment()
    {
        using var db = CreateDb(nameof(FindByOrderIdAsync_Existing_ReturnsPayment));
        var svc = BuildService(db);

        await SeedPayment(db, orderId: 3);

        var result = await svc.FindByOrderIdAsync(3);

        Assert.NotNull(result);
        Assert.Equal(3, result!.OrderId);
    }

    [Fact]
    public async Task FindByOrderIdAsync_NotFound_ReturnsNull()
    {
        using var db = CreateDb(nameof(FindByOrderIdAsync_NotFound_ReturnsNull));
        var svc = BuildService(db);

        var result = await svc.FindByOrderIdAsync(9999);

        Assert.Null(result);
    }

    // ── GetByUserIdAsync ───────────────────────────────────────────────────

    [Fact]
    public async Task GetByUserIdAsync_ReturnsOnlyUserPayments()
    {
        using var db = CreateDb(nameof(GetByUserIdAsync_ReturnsOnlyUserPayments));
        var svc = BuildService(db);

        await SeedPayment(db, orderId: 1, userId: 1);
        await SeedPayment(db, orderId: 2, userId: 1);
        await SeedPayment(db, orderId: 3, userId: 2);

        var result = await svc.GetByUserIdAsync(1);

        Assert.Equal(2, result.Count);
        Assert.All(result, p => Assert.Equal(1, p.UserId));
    }

    // ── GetByIdAsync ───────────────────────────────────────────────────────

    [Fact]
    public async Task GetByIdAsync_Existing_ReturnsPayment()
    {
        using var db = CreateDb(nameof(GetByIdAsync_Existing_ReturnsPayment));
        var svc = BuildService(db);

        var seeded = await SeedPayment(db);

        var result = await svc.GetByIdAsync(seeded.Id);

        Assert.Equal(seeded.Id, result.Id);
    }

    [Fact]
    public async Task GetByIdAsync_NotFound_ThrowsKeyNotFoundException()
    {
        using var db = CreateDb(nameof(GetByIdAsync_NotFound_ThrowsKeyNotFoundException));
        var svc = BuildService(db);

        await Assert.ThrowsAsync<KeyNotFoundException>(() => svc.GetByIdAsync(9999));
    }
}
