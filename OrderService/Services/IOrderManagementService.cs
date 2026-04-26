using OrderService.Dtos;
using OrderService.Models;

namespace OrderService.Services
{
    public interface IOrderManagementService
    {
        public Task<List<OrderResponse>> GetAllAsync();
        public Task<OrderResponse> GetByIdAsync(int id); 
        public Task<List<OrderResponse>> GetByUserIdAsync(int userId); 
        public Task<OrderResponse> CreateAsync(CreateOrderRequest request); 
        public Task<OrderResponse> UpdateStatusAsync(int id, UpdateOrderRequest request);
        public Task<OrderResponse> CancelAsync(int id);
    }
}