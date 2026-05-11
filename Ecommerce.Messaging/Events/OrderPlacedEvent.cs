namespace Ecommerce.Messaging.Events
{
    public class OrderPlacedEvent
    {
        public int OrderId { get; set; }
        public int UserId { get; set; }
        public decimal Amount { get; set; }
        public List<OrderPlacedItem> Items { get; set; } = [];
    }

    public class OrderPlacedItem
    {
        public int ProductId { get; set; }
        public int Quantity { get; set; }
    }
}