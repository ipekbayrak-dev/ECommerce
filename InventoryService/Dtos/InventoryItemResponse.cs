namespace InventoryService.Dtos
{
    public class InventoryItemResponse
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public int Quantity { get; set; }
        public DateTime LastUpdatedUtc { get; set; }
    }
}