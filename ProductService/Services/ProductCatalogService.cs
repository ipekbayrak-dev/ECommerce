using Microsoft.EntityFrameworkCore;
using ProductService.Data;
using ProductService.Dtos;
using ProductService.Models;

namespace ProductService.Services
{
    public class ProductCatalogService : IProductCatalogService
    {
        private readonly ProductDbContext _productDbContext;
        private static ProductResponse MapToResponse(Product product)
        {
            return new ProductResponse
            {
                Id = product.Id,
                Name = product.Name,
                Description = product.Description,
                Price = product.Price,
                StockQuantity = product.StockQuantity,
                CategoryId = product.CategoryId,
                CategoryName = product.Category.Name,
                CreatedAtUtc = product.CreatedAtUtc
            };
        }
        public ProductCatalogService(ProductDbContext productDbContext)
        {
            _productDbContext = productDbContext;
        }
        public async Task<ProductResponse> CreateAsync(CreateProductRequest request)
        {
            var category = await _productDbContext.Categories.SingleOrDefaultAsync(c => c.Id == request.CategoryId);

            if (category == null)
            {
                throw new ArgumentException("Category not found for the provided identifier.", nameof(category));
            }

            Product product = new Product
            {
                Name = request.Name,
                Description = request.Description,
                Price = request.Price,
                StockQuantity = request.StockQuantity,
                Category = category,
                CreatedAtUtc = DateTime.UtcNow
            };

            _productDbContext.Add(product);
            await _productDbContext.SaveChangesAsync();
            return MapToResponse(product);
        }

        public async Task<ProductResponse?> GetByIdAsync(int id)
        {
            var product = await _productDbContext.Products
                .AsNoTracking()
                .Include(p => p.Category)
                .SingleOrDefaultAsync(p => p.Id == id);

            if (product is null)
            {
                return null;
            }

            return MapToResponse(product);
        }

        public async Task DeleteAsync(int id)
        {
            var product = await _productDbContext.Products.SingleOrDefaultAsync(p => p.Id == id);

            if (product is null)
            {
                throw new KeyNotFoundException($"Delete operation failed: Product with identifier '{id}' does not exist.");
            }

            _productDbContext.Remove(product);
            await _productDbContext.SaveChangesAsync();
        }

        public async Task<ProductResponse> UpdateAsync(int id, UpdateProductRequest request)
        {
            var product = await _productDbContext.Products.Include(p => p.Category).SingleOrDefaultAsync(p => p.Id == id);

            if (product is null)
            {
                throw new KeyNotFoundException($"Update operation failed: Product with identifier '{id}' does not exist.");
            }

            if (request.Name is not null)
            {
                product.Name = request.Name;
            }

            if (request.Description is not null)
            {
                product.Description = request.Description;
            }

            if (request.Price is not null)
            {
                product.Price = (decimal)request.Price;
            }

            if (request.StockQuantity is not null)
            {
                product.StockQuantity = (int)request.StockQuantity;
            }

            if (request.CategoryId is not null)
            {
                var category = await _productDbContext.Categories.SingleOrDefaultAsync(c => c.Id == request.CategoryId);

                if (category is null)
                {
                    throw new KeyNotFoundException($"Update operation failed: Category with identifier '{request.CategoryId}' does not exist.");
                }

                product.CategoryId = (int)request.CategoryId;
                product.Category = category;
            }

            await _productDbContext.SaveChangesAsync();

            return MapToResponse(product);
        }

        public async Task<ProductResponse> UpdateStockAsync(int id, UpdateStockRequest request)
        {
            var product = await _productDbContext.Products.Include(p => p.Category).SingleOrDefaultAsync(p => p.Id == id);

            if (product is null)
            {
                throw new KeyNotFoundException($"Update operation failed: Product with identifier '{id}' does not exist.");
            }

            if (request.StockQuantity < 0)
            {
                throw new ArgumentException($"Update operation failed: Stock quantity cannot be negative. Received: {request.StockQuantity}");
            }

            product.StockQuantity = (int)request.StockQuantity;

            await _productDbContext.SaveChangesAsync();

            return MapToResponse(product);

        }

        public async Task<IEnumerable<ProductResponse>> GetAllAsync(string? search, int? categoryId)
        {
            IQueryable<Product> query = _productDbContext.Products
                .AsNoTracking()
                .Include(p => p.Category);

            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(p => p.Name.Contains(search.Trim()));
            }

            if (categoryId.HasValue)
            {
                query = query.Where(p => p.CategoryId == categoryId.Value);
            }

            var products = await query.ToListAsync();
            return products.Select(MapToResponse);
        }
    }
}