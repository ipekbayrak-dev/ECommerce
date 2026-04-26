using OrderService.Models;

namespace OrderService.Dtos
{
    public class UpdateOrderRequest
    {
        public OrderStatus OrderStatus { get; set; }
    }
}