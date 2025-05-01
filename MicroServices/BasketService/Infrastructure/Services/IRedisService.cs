using System.Text.Json;

namespace BasketService.Infrastructure.Services;

public interface IRedisService
{
    Task<T?> GetAsync<T>(string key);
    Task SetAsync<T>(string key, T value, TimeSpan? expiry = null);
    Task DeleteAsync(string key);
} 