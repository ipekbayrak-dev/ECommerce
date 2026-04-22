using Microsoft.EntityFrameworkCore;
using ProductService.Data;
using ProductService.Dtos;
using ProductService.Models;

namespace ProductService.Services
{
    public class ProductService : IProductService
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
        public ProductService(ProductDbContext productDbContext)
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

        public Task<ProductResponse?> GetByIdAsync(int id)
        {
            throw new NotImplementedException();
        }

        public Task DeleteAsync(int id)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<ProductResponse>> GetAllAsync(string? search, int? categoryId)
        {
            throw new NotImplementedException();
        }

        public Task<ProductResponse> UpdateAsync(int id, UpdateProductRequest request)
        {
            throw new NotImplementedException();
        }

        public Task<ProductResponse> UpdateStockAsync(int id, UpdateStockRequest request)
        {
            throw new NotImplementedException();
        }
    }
}