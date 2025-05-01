namespace BasketService.Domain.Entities;

public class Basket
{
    public required string UserId { get; set; }
    public List<BasketItem> Items { get; set; } = new();
    public decimal TotalPrice => Items.Sum(x => x.Price * x.Quantity);
} 