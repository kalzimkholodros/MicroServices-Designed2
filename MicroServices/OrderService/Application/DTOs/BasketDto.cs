namespace OrderService.Application.DTOs;

public class BasketDto
{
    public required string UserId { get; set; }
    public List<BasketItemDto> Items { get; set; } = new();
    public decimal TotalPrice { get; set; }
}

public class BasketItemDto
{
    public required string ProductId { get; set; }
    public required string ProductName { get; set; }
    public decimal Price { get; set; }
    public int Quantity { get; set; }
} 