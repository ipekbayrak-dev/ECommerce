namespace Ecommerce.Messaging.Events
{
    public class OrderPlacedEvent
    {
        public int OrderId { get; set; }
        public int UserId { get; set; }
        public decimal Amount { get; set; }
    }
}