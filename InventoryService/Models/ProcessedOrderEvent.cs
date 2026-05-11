namespace InventoryService.Models
{
    public class ProcessedOrderEvent
    {
        public int OrderId { get; set; }
        public DateTime ProcessedAtUtc { get; set; }
    }
}