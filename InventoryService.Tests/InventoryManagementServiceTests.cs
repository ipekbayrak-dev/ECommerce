using InventoryService.Data;
using InventoryService.Dtos;
using InventoryService.Services;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace InventoryService.Tests;

public class InventoryManagementServiceTests
{
    private static InventoryDbContext CreateDb(string name)
    {
        var options = new DbContextOptionsBuilder<InventoryDbContext>()
            .UseInMemoryDatabase(name)
            .Options;
        return new InventoryDbContext(options);
    }

    private static InventoryManagementService BuildService(InventoryDbContext db)
        => new InventoryManagementService(db);

    // ── SeedAsync ──────────────────────────────────────────────────────────

    [Fact]
    public async Task SeedAsync_NewProduct_CreatesInventoryItem()
    {
        using var db = CreateDb(nameof(SeedAsync_NewProduct_CreatesInventoryItem));
        var svc = BuildService(db);

        var result = await svc.SeedAsync(1, new SeedInventoryRequest { InitialQuantity = 50 });

        Assert.Equal(1, result.ProductId);
        Assert.Equal(50, result.Quantity);
    }

    [Fact]
    public async Task SeedAsync_AlreadyExists_ThrowsInvalidOperationException()
    {
        using var db = CreateDb(nameof(SeedAsync_AlreadyExists_ThrowsInvalidOperationException));
        var svc = BuildService(db);

        await svc.SeedAsync(1, new SeedInventoryRequest { InitialQuantity = 10 });

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => svc.SeedAsync(1, new SeedInventoryRequest { InitialQuantity = 5 }));
    }

    [Fact]
    public async Task SeedAsync_ZeroQuantity_Succeeds()
    {
        using var db = CreateDb(nameof(SeedAsync_ZeroQuantity_Succeeds));
        var svc = BuildService(db);

        var result = await svc.SeedAsync(42, new SeedInventoryRequest { InitialQuantity = 0 });

        Assert.Equal(0, result.Quantity);
    }

    // ── GetByProductIdAsync ────────────────────────────────────────────────

    [Fact]
    public async Task GetByProductIdAsync_Existing_ReturnsItem()
    {
        using var db = CreateDb(nameof(GetByProductIdAsync_Existing_ReturnsItem));
        var svc = BuildService(db);

        await svc.SeedAsync(7, new SeedInventoryRequest { InitialQuantity = 20 });

        var result = await svc.GetByProductIdAsync(7);

        Assert.Equal(7, result.ProductId);
        Assert.Equal(20, result.Quantity);
    }

    [Fact]
    public async Task GetByProductIdAsync_NotFound_ThrowsKeyNotFoundException()
    {
        using var db = CreateDb(nameof(GetByProductIdAsync_NotFound_ThrowsKeyNotFoundException));
        var svc = BuildService(db);

        await Assert.ThrowsAsync<KeyNotFoundException>(() => svc.GetByProductIdAsync(9999));
    }

    // ── AdjustStockAsync ───────────────────────────────────────────────────

    [Fact]
    public async Task AdjustStockAsync_PositiveDelta_IncreasesQuantity()
    {
        using var db = CreateDb(nameof(AdjustStockAsync_PositiveDelta_IncreasesQuantity));
        var svc = BuildService(db);

        await svc.SeedAsync(1, new SeedInventoryRequest { InitialQuantity = 10 });

        var result = await svc.AdjustStockAsync(1, new AdjustStockRequest { Delta = 5 });

        Assert.Equal(15, result.Quantity);
    }

    [Fact]
    public async Task AdjustStockAsync_NegativeDelta_DecreasesQuantity()
    {
        using var db = CreateDb(nameof(AdjustStockAsync_NegativeDelta_DecreasesQuantity));
        var svc = BuildService(db);

        await svc.SeedAsync(1, new SeedInventoryRequest { InitialQuantity = 10 });

        var result = await svc.AdjustStockAsync(1, new AdjustStockRequest { Delta = -4 });

        Assert.Equal(6, result.Quantity);
    }

    [Fact]
    public async Task AdjustStockAsync_WouldGoNegative_ThrowsInvalidOperationException()
    {
        using var db = CreateDb(nameof(AdjustStockAsync_WouldGoNegative_ThrowsInvalidOperationException));
        var svc = BuildService(db);

        await svc.SeedAsync(1, new SeedInventoryRequest { InitialQuantity = 3 });

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => svc.AdjustStockAsync(1, new AdjustStockRequest { Delta = -10 }));
    }

    [Fact]
    public async Task AdjustStockAsync_NotFound_ThrowsKeyNotFoundException()
    {
        using var db = CreateDb(nameof(AdjustStockAsync_NotFound_ThrowsKeyNotFoundException));
        var svc = BuildService(db);

        await Assert.ThrowsAsync<KeyNotFoundException>(
            () => svc.AdjustStockAsync(9999, new AdjustStockRequest { Delta = 1 }));
    }

    [Fact]
    public async Task AdjustStockAsync_UpdatesLastUpdatedUtc()
    {
        using var db = CreateDb(nameof(AdjustStockAsync_UpdatesLastUpdatedUtc));
        var svc = BuildService(db);

        await svc.SeedAsync(1, new SeedInventoryRequest { InitialQuantity = 5 });
        var before = DateTime.UtcNow;

        await svc.AdjustStockAsync(1, new AdjustStockRequest { Delta = 2 });
        var after = DateTime.UtcNow;

        var item = await db.InventoryItems.SingleAsync(i => i.ProductId == 1);
        Assert.InRange(item.LastUpdatedUtc, before.AddSeconds(-1), after.AddSeconds(1));
    }
}
