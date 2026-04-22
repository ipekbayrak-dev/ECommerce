using ProductService.Dtos;

namespace ProductService.Services
{
    public interface IProductService
    {
        public Task<ProductResponse> CreateAsync(CreateProductRequest request);
        public Task<ProductResponse?> GetByIdAsync(int id);
        public Task<IEnumerable<ProductResponse>> GetAllAsync(string? search, int? categoryId);
        public Task<ProductResponse> UpdateAsync(int id, UpdateProductRequest request);
        public Task DeleteAsync(int id);
        public Task<ProductResponse> UpdateStockAsync(int id, UpdateStockRequest request);
    }
}