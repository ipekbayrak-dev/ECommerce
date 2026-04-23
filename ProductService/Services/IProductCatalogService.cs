using ProductService.Dtos;

namespace ProductService.Services
{
    public interface IProductCatalogService
    {
        public Task<ProductResponse> CreateAsync(CreateProductRequest request);
        public Task<ProductResponse?> GetByIdAsync(int id);
        public Task<IEnumerable<ProductResponse>> GetAllAsync(string? search, int? categoryId, int page, int pageSize);
        public Task<ProductResponse> UpdateAsync(int id, UpdateProductRequest request);
        public Task DeleteAsync(int id);
        public Task<ProductResponse> UpdateStockAsync(int id, UpdateStockRequest request);
    }
}