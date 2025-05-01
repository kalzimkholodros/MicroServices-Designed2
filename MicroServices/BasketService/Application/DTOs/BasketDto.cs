namespace BasketService.Application.DTOs;

public class BasketDto
{
    public required string UserId { get; set; }
    public List<BasketItemDto> Items { get; set; } = new();
    public decimal TotalPrice { get; set; }
} 