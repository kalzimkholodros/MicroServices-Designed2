using System.Text.Json;
using OrderService.Application.DTOs;

namespace OrderService.Infrastructure.Services;

public interface IBasketService
{
    Task<BasketDto> GetBasket(string userId);
}

public class BasketService : IBasketService
{
    private readonly HttpClient _httpClient;

    public BasketService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<BasketDto> GetBasket(string userId)
    {
        var response = await _httpClient.GetAsync($"/api/basket/{userId}");
        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<BasketDto>(content, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        }) ?? throw new Exception("Basket not found");
    }
} 