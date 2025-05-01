namespace RabbitMQCommunication.Events;

public class PaymentCompletedEvent
{
    public Guid OrderId { get; set; }
    public string UserId { get; set; }
    public decimal TotalPrice { get; set; }
    public List<OrderItem> Items { get; set; } = new();

    public class OrderItem
    {
        public string ProductId { get; set; }
        public string ProductName { get; set; }
        public int Quantity { get; set; }
    }
} 