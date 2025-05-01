namespace RabbitMQCommunication.Events;

public class StockUpdatedEvent
{
    public List<StockItem> Items { get; set; } = new();

    public class StockItem
    {
        public string ProductId { get; set; }
        public int Quantity { get; set; }
    }
} 