using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Moq;
using ProductService.Data;
using ProductService.Dtos;
using ProductService.Models;
using ProductService.Services;
using Xunit;
using System.Linq;

namespace ProductService.Tests;

public class ProductCatalogServiceTests
{
    private static ProductDbContext CreateDb(string name)
    {
        var options = new DbContextOptionsBuilder<ProductDbContext>()
            .UseInMemoryDatabase(name)
            .Options;
        return new ProductDbContext(options);
    }

    private static IDistributedCache NullCache()
    {
        var cache = new Mock<IDistributedCache>();
        // GetStringAsync is an extension that calls GetAsync internally
        cache.Setup(c => c.GetAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
             .ReturnsAsync((byte[]?)null);
        // SetStringAsync calls SetAsync internally; ignore cache writes
        cache.Setup(c => c.SetAsync(It.IsAny<string>(), It.IsAny<byte[]>(),
                It.IsAny<DistributedCacheEntryOptions>(), It.IsAny<CancellationToken>()))
             .Returns(Task.CompletedTask);
        cache.Setup(c => c.RemoveAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
             .Returns(Task.CompletedTask);
        return cache.Object;
    }

    private static ProductCatalogService BuildService(ProductDbContext db)
        => new ProductCatalogService(db, NullCache());

    private static async Task<int> SeedCategory(ProductDbContext db, string name = "Electronics")
    {
        var cat = new Category { Name = name };
        db.Categories.Add(cat);
        await db.SaveChangesAsync();
        return cat.Id;
    }

    private static CreateProductRequest ValidCreateRequest(int categoryId) => new()
    {
        Name = "Widget Pro",
        Description = "A great widget",
        Price = 99.99m,
        CategoryId = categoryId
    };

    // ── CreateAsync ────────────────────────────────────────────────────────

    [Fact]
    public async Task CreateAsync_ValidRequest_ReturnsProduct()
    {
        using var db = CreateDb(nameof(CreateAsync_ValidRequest_ReturnsProduct));
        var svc = BuildService(db);
        var catId = await SeedCategory(db);

        var result = await svc.CreateAsync(ValidCreateRequest(catId));

        Assert.Equal("Widget Pro", result.Name);
        Assert.Equal(99.99m, result.Price);
        Assert.Equal("Electronics", result.CategoryName);
    }

    [Fact]
    public async Task CreateAsync_EmptyName_ThrowsArgumentException()
    {
        using var db = CreateDb(nameof(CreateAsync_EmptyName_ThrowsArgumentException));
        var svc = BuildService(db);
        var catId = await SeedCategory(db);

        var req = ValidCreateRequest(catId);
        req.Name = "  ";

        await Assert.ThrowsAsync<ArgumentException>(() => svc.CreateAsync(req));
    }

    [Fact]
    public async Task CreateAsync_NegativePrice_ThrowsArgumentException()
    {
        using var db = CreateDb(nameof(CreateAsync_NegativePrice_ThrowsArgumentException));
        var svc = BuildService(db);
        var catId = await SeedCategory(db);

        var req = ValidCreateRequest(catId);
        req.Price = -1m;

        await Assert.ThrowsAsync<ArgumentException>(() => svc.CreateAsync(req));
    }

    [Fact]
    public async Task CreateAsync_InvalidCategoryId_ThrowsArgumentException()
    {
        using var db = CreateDb(nameof(CreateAsync_InvalidCategoryId_ThrowsArgumentException));
        var svc = BuildService(db);

        var req = ValidCreateRequest(999); // category doesn't exist

        await Assert.ThrowsAsync<ArgumentException>(() => svc.CreateAsync(req));
    }

    [Fact]
    public async Task CreateAsync_ZeroCategoryId_ThrowsArgumentException()
    {
        using var db = CreateDb(nameof(CreateAsync_ZeroCategoryId_ThrowsArgumentException));
        var svc = BuildService(db);

        var req = ValidCreateRequest(0);

        await Assert.ThrowsAsync<ArgumentException>(() => svc.CreateAsync(req));
    }

    // ── GetByIdAsync ───────────────────────────────────────────────────────

    [Fact]
    public async Task GetByIdAsync_ExistingProduct_ReturnsProduct()
    {
        using var db = CreateDb(nameof(GetByIdAsync_ExistingProduct_ReturnsProduct));
        var svc = BuildService(db);
        var catId = await SeedCategory(db);

        var created = await svc.CreateAsync(ValidCreateRequest(catId));

        var result = await svc.GetByIdAsync(created.Id);

        Assert.NotNull(result);
        Assert.Equal(created.Id, result!.Id);
        Assert.Equal("Widget Pro", result.Name);
    }

    [Fact]
    public async Task GetByIdAsync_NotFound_ReturnsNull()
    {
        using var db = CreateDb(nameof(GetByIdAsync_NotFound_ReturnsNull));
        var svc = BuildService(db);

        var result = await svc.GetByIdAsync(9999);

        Assert.Null(result);
    }

    // ── GetAllAsync ────────────────────────────────────────────────────────

    [Fact]
    public async Task GetAllAsync_Pagination_ReturnsCorrectCount()
    {
        using var db = CreateDb(nameof(GetAllAsync_Pagination_ReturnsCorrectCount));
        var svc = BuildService(db);
        var catId = await SeedCategory(db);

        for (int i = 1; i <= 5; i++)
        {
            await svc.CreateAsync(new CreateProductRequest { Name = $"P{i}", Price = i, CategoryId = catId });
        }

        var page1 = (await svc.GetAllAsync(null, null, 1, 3)).ToList();
        var page2 = (await svc.GetAllAsync(null, null, 2, 3)).ToList();

        Assert.Equal(3, page1.Count);
        Assert.Equal(2, page2.Count);
    }

    // Note: GetAllAsync uses EF.Functions.ILike which requires a real PostgreSQL DB.
    // The InMemory provider does not support ILike, so we skip the search-filter test here.
    // To test search filtering, use an integration test against a real or SQLite DB.
    [Fact]
    public async Task GetAllAsync_NullSearch_ReturnsAllProducts()
    {
        using var db = CreateDb(nameof(GetAllAsync_NullSearch_ReturnsAllProducts));
        var svc = BuildService(db);
        var catId = await SeedCategory(db);

        await svc.CreateAsync(new CreateProductRequest { Name = "iPhone 15", Price = 999, CategoryId = catId });
        await svc.CreateAsync(new CreateProductRequest { Name = "Samsung S25", Price = 899, CategoryId = catId });

        var result = (await svc.GetAllAsync(null, null, 1, 10)).ToList();

        Assert.Equal(2, result.Count);
    }

    [Fact]
    public async Task GetAllAsync_InvalidPageSize_ThrowsArgumentException()
    {
        using var db = CreateDb(nameof(GetAllAsync_InvalidPageSize_ThrowsArgumentException));
        var svc = BuildService(db);

        await Assert.ThrowsAsync<ArgumentException>(() => svc.GetAllAsync(null, null, 1, 0));
    }

    [Fact]
    public async Task GetAllAsync_PageSizeTooLarge_ThrowsArgumentException()
    {
        using var db = CreateDb(nameof(GetAllAsync_PageSizeTooLarge_ThrowsArgumentException));
        var svc = BuildService(db);

        await Assert.ThrowsAsync<ArgumentException>(() => svc.GetAllAsync(null, null, 1, 101));
    }

    // ── UpdateAsync ────────────────────────────────────────────────────────

    [Fact]
    public async Task UpdateAsync_ValidRequest_UpdatesFields()
    {
        using var db = CreateDb(nameof(UpdateAsync_ValidRequest_UpdatesFields));
        var svc = BuildService(db);
        var catId = await SeedCategory(db);

        var created = await svc.CreateAsync(ValidCreateRequest(catId));

        var result = await svc.UpdateAsync(created.Id, new UpdateProductRequest { Name = "Widget Ultra", Price = 199m });

        Assert.Equal("Widget Ultra", result.Name);
        Assert.Equal(199m, result.Price);
    }

    [Fact]
    public async Task UpdateAsync_NotFound_ThrowsKeyNotFoundException()
    {
        using var db = CreateDb(nameof(UpdateAsync_NotFound_ThrowsKeyNotFoundException));
        var svc = BuildService(db);

        await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            svc.UpdateAsync(999, new UpdateProductRequest { Name = "X" }));
    }

    [Fact]
    public async Task UpdateAsync_EmptyName_ThrowsArgumentException()
    {
        using var db = CreateDb(nameof(UpdateAsync_EmptyName_ThrowsArgumentException));
        var svc = BuildService(db);
        var catId = await SeedCategory(db);

        var created = await svc.CreateAsync(ValidCreateRequest(catId));

        await Assert.ThrowsAsync<ArgumentException>(() =>
            svc.UpdateAsync(created.Id, new UpdateProductRequest { Name = "  " }));
    }

    // ── DeleteAsync ────────────────────────────────────────────────────────

    [Fact]
    public async Task DeleteAsync_ExistingProduct_RemovesFromDb()
    {
        using var db = CreateDb(nameof(DeleteAsync_ExistingProduct_RemovesFromDb));
        var svc = BuildService(db);
        var catId = await SeedCategory(db);

        var created = await svc.CreateAsync(ValidCreateRequest(catId));

        await svc.DeleteAsync(created.Id);

        Assert.Equal(0, await db.Products.CountAsync());
    }

    [Fact]
    public async Task DeleteAsync_NotFound_ThrowsKeyNotFoundException()
    {
        using var db = CreateDb(nameof(DeleteAsync_NotFound_ThrowsKeyNotFoundException));
        var svc = BuildService(db);

        await Assert.ThrowsAsync<KeyNotFoundException>(() => svc.DeleteAsync(9999));
    }

    // ── GetCategoriesAsync ─────────────────────────────────────────────────

    [Fact]
    public async Task GetCategoriesAsync_ReturnsAllCategories()
    {
        using var db = CreateDb(nameof(GetCategoriesAsync_ReturnsAllCategories));
        var svc = BuildService(db);

        await SeedCategory(db, "Electronics");
        await SeedCategory(db, "Books");

        var result = (await svc.GetCategoriesAsync()).ToList();

        Assert.Equal(2, result.Count);
    }
}
