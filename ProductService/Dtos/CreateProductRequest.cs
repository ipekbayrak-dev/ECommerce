using System.ComponentModel.DataAnnotations;

namespace ProductService.Dtos
{
    public class CreateProductRequest
    {
        [Required]
        [MaxLength(150)]
        public required string Name { get; set; }

        [MaxLength(1000)]
        public string? Description { get; set; }

        [Range(typeof(decimal), "0", "79228162514264337593543950335")]
        public decimal Price { get; set; }

        [Range(0, int.MaxValue)]
        public int StockQuantity { get; set; }

        [Range(1, int.MaxValue)]
        public int CategoryId { get; set; }
    }
}