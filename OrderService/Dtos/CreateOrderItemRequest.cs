using System.ComponentModel.DataAnnotations;

namespace OrderService.Dtos
{
    public class CreateOrderItemRequest
    {
        public int ProductId { get; set; }
        public required string ProductName { get; set; }
        [Range(1, int.MaxValue, ErrorMessage = "Quantity must be at least 1.")]
        public int Quantity { get; set; }
        [Range(0.01, double.MaxValue, ErrorMessage = "UnitPrice must be greater than zero.")]
        public decimal UnitPrice { get; set; }
        public decimal Discount { get; set; }
    }
}