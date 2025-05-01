namespace BasketService.Application.DTOs;

public class ProductDto
{
    public required string Id { get; set; }
    public required string Name { get; set; }
    public decimal Price { get; set; }
    public required string Description { get; set; }
    public int Stock { get; set; }
} 