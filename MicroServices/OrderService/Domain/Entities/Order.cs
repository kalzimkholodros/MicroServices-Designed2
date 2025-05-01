namespace OrderService.Domain.Entities;

public class Order
{
    public Guid Id { get; set; }
    public required string UserId { get; set; }
    public required string UserName { get; set; }
    public required string UserEmail { get; set; }
    public required string Address { get; set; }
    public List<OrderItem> Items { get; set; } = new();
    public decimal TotalPrice { get; set; }
    public OrderStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class OrderItem
{
    public Guid Id { get; set; }
    public required string ProductId { get; set; }
    public required string ProductName { get; set; }
    public decimal Price { get; set; }
    public int Quantity { get; set; }
}

public enum OrderStatus
{
    Pending,
    Processing,
    Shipped,
    Delivered,
    Cancelled
} 