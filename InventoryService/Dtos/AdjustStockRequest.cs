using System.ComponentModel.DataAnnotations;
using InventoryService.Dtos.Validation;

namespace InventoryService.Dtos
{
    public class AdjustStockRequest
    {
        [NonZero(ErrorMessage = "Delta cannot be zero.")]
        [Range(-100000, 100000, ErrorMessage = "Stock adjustment delta must be between -100,000 and 100,000 units.")]
        public int Delta { get; set; }
    }
}