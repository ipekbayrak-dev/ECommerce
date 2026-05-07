using System.ComponentModel.DataAnnotations;

namespace InventoryService.Dtos
{
    public class SeedInventoryRequest
    {
        [Range(0, int.MaxValue, ErrorMessage = "InitialQuantity cannot be negative.")]
        public int InitialQuantity { get; set; }
    }
}