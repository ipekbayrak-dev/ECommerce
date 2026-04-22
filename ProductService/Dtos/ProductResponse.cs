namespace ProductService.Dtos
{
    public class ProductResponse
    {
        public int Id { get; set; }
        public required string Name { get; set; }
        public string? Description { get; set; }
        public decimal Price { get; set; }
        public int StockQuantity { get; set; }
        public int CategoryId { get; set; }
        public required string CategoryName { get; set; }
        public DateTime CreatedAtUtc { get; set; }
    }
}