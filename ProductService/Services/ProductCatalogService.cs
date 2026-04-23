using Microsoft.EntityFrameworkCore;
using ProductService.Data;
using ProductService.Dtos;
using ProductService.Models;

namespace ProductService.Services
{
    public class ProductCatalogService : IProductCatalogService
    {
        private const int MaxPageSize = 100;
        private readonly ProductDbContext _productDbContext;

        private static void ValidateCreateRequest(CreateProductRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Name))
            {
                throw new ArgumentException("Product name is required.", nameof(request.Name));
            }

            if (request.Price < 0)
            {
                throw new ArgumentException("Product price cannot be negative.", nameof(request.Price));
            }

            if (request.StockQuantity < 0)
            {
                throw new ArgumentException("Stock quantity cannot be negative.", nameof(request.StockQuantity));
            }

            if (request.CategoryId <= 0)
            {
                throw new ArgumentException("Category identifier must be greater than zero.", nameof(request.CategoryId));
            }
        }

        private static void ValidateUpdateRequest(UpdateProductRequest request)
        {
            if (request.Name is not null && string.IsNullOrWhiteSpace(request.Name))
            {
                throw new ArgumentException("Product name cannot be empty.", nameof(request.Name));
            }

            if (request.Price is not null && request.Price < 0)
            {
                throw new ArgumentException("Product price cannot be negative.", nameof(request.Price));
            }

            if (request.StockQuantity is not null && request.StockQuantity < 0)
            {
                throw new ArgumentException("Stock quantity cannot be negative.", nameof(request.StockQuantity));
            }

            if (request.CategoryId is not null && request.CategoryId <= 0)
            {
                throw new ArgumentException("Category identifier must be greater than zero.", nameof(request.CategoryId));
            }
        }

        private static void ValidatePagination(int page, int pageSize)
        {
            if (page < 1)
            {
                throw new ArgumentException("Page must be greater than or equal to 1.", nameof(page));
            }

            if (pageSize < 1 || pageSize > MaxPageSize)
            {
                throw new ArgumentException($"Page size must be between 1 and {MaxPageSize}.", nameof(pageSize));
            }
        }

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
            ValidateCreateRequest(request);

            var category = await _productDbContext.Categories.SingleOrDefaultAsync(c => c.Id == request.CategoryId);

            if (category == null)
            {
                throw new ArgumentException("Category not found for the provided identifier.", nameof(category));
            }

            Product product = new Product
            {
                Name = request.Name.Trim(),
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
            ValidateUpdateRequest(request);

            var product = await _productDbContext.Products.Include(p => p.Category).SingleOrDefaultAsync(p => p.Id == id);

            if (product is null)
            {
                throw new KeyNotFoundException($"Update operation failed: Product with identifier '{id}' does not exist.");
            }

            if (request.Name is not null)
            {
                product.Name = request.Name.Trim();
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

        public async Task<IEnumerable<ProductResponse>> GetAllAsync(string? search, int? categoryId, int page, int pageSize)
        {
            ValidatePagination(page, pageSize);

            IQueryable<Product> query = _productDbContext.Products
                .AsNoTracking()
                .Include(p => p.Category);

            if (!string.IsNullOrWhiteSpace(search))
            {
                var searchTerm = search.Trim();
                query = query.Where(p => EF.Functions.ILike(p.Name, $"%{searchTerm}%"));
            }

            if (categoryId.HasValue)
            {
                query = query.Where(p => p.CategoryId == categoryId.Value);
            }

            var products = await query
                .OrderBy(p => p.Id)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return products.Select(MapToResponse);
        }
    }
}