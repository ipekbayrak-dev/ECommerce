using InventoryService.Data;
using InventoryService.Dtos;
using InventoryService.Models;
using Microsoft.EntityFrameworkCore;

namespace InventoryService.Services
{
    public class InventoryManagementService : IInventoryManagementService
    {
        private readonly InventoryDbContext _inventoryDbContext;
        private static InventoryItemResponse MapToResponse(InventoryItem inventoryItem)
        {
            return new InventoryItemResponse
            {
                Id = inventoryItem.Id,
                ProductId = inventoryItem.ProductId,
                Quantity = inventoryItem.Quantity,
                LastUpdatedUtc = inventoryItem.LastUpdatedUtc
            };
        }
        public InventoryManagementService(InventoryDbContext inventoryDbContext)
        {
            _inventoryDbContext = inventoryDbContext;
        }
        public async Task<InventoryItemResponse> AdjustStockAsync(int productId, AdjustStockRequest request)
        {
            var item = await _inventoryDbContext.InventoryItems.SingleOrDefaultAsync(p => p.ProductId == productId);

            if (item is null)
            {
                throw new KeyNotFoundException($"Inventory item with Product ID {productId} was not found.");
            }

            int projectedQuantity = item.Quantity + request.Delta;

            if (projectedQuantity < 0)
            {
                throw new InvalidOperationException(
                    $"Inventory underflow detected. Current stock is {item.Quantity}, but the requested delta ({request.Delta}) would result in a negative balance."
                );
            }

            item.Quantity = projectedQuantity;
            item.LastUpdatedUtc = DateTime.UtcNow;
            await _inventoryDbContext.SaveChangesAsync();

            return MapToResponse(item);
        }

        public async Task<InventoryItemResponse> GetByProductIdAsync(int productId)
        {
            var item = await _inventoryDbContext.InventoryItems
                .AsNoTracking()
                .SingleOrDefaultAsync(p => p.ProductId == productId);

            if (item is null)
            {
                throw new KeyNotFoundException($"Inventory item with Product ID {productId} was not found.");
            }

            return MapToResponse(item);
        }

        public async Task<InventoryItemResponse> SeedAsync(int productId, SeedInventoryRequest request)
        {
            var item = await _inventoryDbContext.InventoryItems
                .SingleOrDefaultAsync(p => p.ProductId == productId);

            if (item is not null)
            {
                throw new InvalidOperationException($"Product {productId} is already initialized. Use AdjustStock for updates.");

            }

            item = new InventoryItem
            {
                ProductId = productId,
                Quantity = request.InitialQuantity,
                LastUpdatedUtc = DateTime.UtcNow
            };

            _inventoryDbContext.InventoryItems.Add(item);
            await _inventoryDbContext.SaveChangesAsync();

            return MapToResponse(item);
        }
    }
}