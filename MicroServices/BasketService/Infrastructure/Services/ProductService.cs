using System.Text.Json;
using BasketService.Application.DTOs;

namespace BasketService.Infrastructure.Services;

public interface IProductService
{
    Task<ProductDto> GetProductById(string productId);
}

public class ProductService : IProductService
{
    private readonly HttpClient _httpClient;

    public ProductService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<ProductDto> GetProductById(string productId)
    {
        var response = await _httpClient.GetAsync($"/api/products/{productId}");
        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<ProductDto>(content, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        }) ?? throw new Exception("Product not found");
    }
} 