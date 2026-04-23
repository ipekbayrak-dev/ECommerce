using System.ComponentModel.DataAnnotations;

namespace ProductService.Dtos
{
    public class UpdateStockRequest
    {
        [Range(0, int.MaxValue)]
        public int StockQuantity { get; set; }
    }
}