using System.Net.Http.Json;
using Xunit;

namespace YarpApiGatewayTest;

public class YarpApiGatewayTests : IDisposable
{
    private readonly HttpClient _client;
    private const string BaseUrl = "http://localhost:5223";

    public YarpApiGatewayTests()
    {
        _client = new HttpClient();
        _client.Timeout = TimeSpan.FromSeconds(30);
    }

    public void Dispose()
    {
        _client.Dispose();
    }

    [Fact]
    public async Task ProductService_Should_Be_Accessible_Through_Gateway()
    {
        try
        {
            // Act
            var response = await _client.GetAsync($"{BaseUrl}/api/products");

            // Assert
            Assert.True(response.IsSuccessStatusCode, $"ProductService returned {response.StatusCode}");
        }
        catch (Exception ex)
        {
            Assert.Fail($"ProductService test failed: {ex.Message}");
        }
    }

    [Fact]
    public async Task BasketService_Should_Be_Accessible_Through_Gateway()
    {
        try
        {
            // Act
            var response = await _client.GetAsync($"{BaseUrl}/api/basket");

            // Assert
            Assert.True(response.IsSuccessStatusCode, $"BasketService returned {response.StatusCode}");
        }
        catch (Exception ex)
        {
            Assert.Fail($"BasketService test failed: {ex.Message}");
        }
    }

    [Fact]
    public async Task OrderService_Should_Be_Accessible_Through_Gateway()
    {
        try
        {
            // Act
            var response = await _client.GetAsync($"{BaseUrl}/api/orders");

            // Assert
            Assert.True(response.IsSuccessStatusCode, $"OrderService returned {response.StatusCode}");
        }
        catch (Exception ex)
        {
            Assert.Fail($"OrderService test failed: {ex.Message}");
        }
    }

    [Fact]
    public async Task PaymentService_Should_Be_Accessible_Through_Gateway()
    {
        try
        {
            // Act
            var response = await _client.GetAsync($"{BaseUrl}/api/payments");

            // Assert
            Assert.True(response.IsSuccessStatusCode, $"PaymentService returned {response.StatusCode}");
        }
        catch (Exception ex)
        {
            Assert.Fail($"PaymentService test failed: {ex.Message}");
        }
    }

    [Fact]
    public async Task ProductService_Should_Return_Products()
    {
        try
        {
            // Act
            var response = await _client.GetAsync($"{BaseUrl}/api/products");
            var content = await response.Content.ReadAsStringAsync();

            // Assert
            Assert.True(response.IsSuccessStatusCode, $"ProductService returned {response.StatusCode}");
            Assert.NotNull(content);
            Assert.NotEmpty(content);
        }
        catch (Exception ex)
        {
            Assert.Fail($"ProductService content test failed: {ex.Message}");
        }
    }

    [Fact]
    public async Task BasketService_Should_Handle_Basket_Operations()
    {
        try
        {
            // Arrange
            var userId = Guid.NewGuid().ToString();
            var productId = Guid.NewGuid().ToString();
            var quantity = 1;

            // Act - Add item to basket
            var addResponse = await _client.PostAsJsonAsync($"{BaseUrl}/api/basket", new
            {
                UserId = userId,
                ProductId = productId,
                Quantity = quantity
            });

            // Assert
            Assert.True(addResponse.IsSuccessStatusCode, $"Add to basket failed with {addResponse.StatusCode}");

            // Act - Get basket
            var getResponse = await _client.GetAsync($"{BaseUrl}/api/basket/{userId}");
            var content = await getResponse.Content.ReadAsStringAsync();

            // Assert
            Assert.True(getResponse.IsSuccessStatusCode, $"Get basket failed with {getResponse.StatusCode}");
            Assert.NotNull(content);
            Assert.NotEmpty(content);
        }
        catch (Exception ex)
        {
            Assert.Fail($"BasketService operations test failed: {ex.Message}");
        }
    }

    [Fact]
    public async Task OrderService_Should_Create_Order()
    {
        try
        {
            // Arrange
            var order = new
            {
                UserId = Guid.NewGuid().ToString(),
                UserName = "Test User",
                UserEmail = "test@example.com",
                Address = "Test Address",
                Items = new[]
                {
                    new
                    {
                        ProductId = Guid.NewGuid().ToString(),
                        ProductName = "Test Product",
                        Price = 100,
                        Quantity = 1
                    }
                },
                TotalPrice = 100
            };

            // Act
            var response = await _client.PostAsJsonAsync($"{BaseUrl}/api/orders", order);

            // Assert
            Assert.True(response.IsSuccessStatusCode, $"Create order failed with {response.StatusCode}");
        }
        catch (Exception ex)
        {
            Assert.Fail($"OrderService test failed: {ex.Message}");
        }
    }

    [Fact]
    public async Task PaymentService_Should_Process_Payment()
    {
        try
        {
            // Arrange
            var payment = new
            {
                OrderId = Guid.NewGuid(),
                UserId = Guid.NewGuid().ToString(),
                Amount = 100
            };

            // Act
            var response = await _client.PostAsJsonAsync($"{BaseUrl}/api/payments", payment);

            // Assert
            Assert.True(response.IsSuccessStatusCode, $"Process payment failed with {response.StatusCode}");
        }
        catch (Exception ex)
        {
            Assert.Fail($"PaymentService test failed: {ex.Message}");
        }
    }
}
