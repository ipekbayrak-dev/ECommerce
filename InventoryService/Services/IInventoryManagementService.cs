using InventoryService.Dtos;

namespace InventoryService.Services
{
    public interface IInventoryManagementService
    {
        public Task<InventoryItemResponse> GetByProductIdAsync(int productId);
        public Task<InventoryItemResponse> AdjustStockAsync(int productId, AdjustStockRequest request);
        public Task<InventoryItemResponse> SeedAsync(int productId, SeedInventoryRequest request);
    }
}