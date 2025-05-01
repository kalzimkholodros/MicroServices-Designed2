using BasketService.Application.DTOs;
using BasketService.Application.Features.Basket.Commands.AddItemToBasket;
using BasketService.Application.Features.Basket.Queries.GetBasket;
using BasketService.Domain.Entities;
using BasketService.Infrastructure.Services;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.StackExchangeRedis;
using Moq;
using StackExchange.Redis;
using System.Text.Json;

namespace BasketTest;

public class BasketServiceTests
{
    private readonly Mock<IDistributedCache> _mockCache;
    private readonly Mock<IProductService> _mockProductService;
    private readonly RedisService _redisService;
    private readonly AddItemToBasketCommandHandler _addItemHandler;
    private readonly GetBasketQueryHandler _getBasketHandler;

    public BasketServiceTests()
    {
        _mockCache = new Mock<IDistributedCache>();
        _mockProductService = new Mock<IProductService>();
        _redisService = new RedisService(_mockCache.Object);
        
        var mapper = new AutoMapper.MapperConfiguration(cfg =>
        {
            cfg.CreateMap<BasketItemDto, BasketItem>();
            cfg.CreateMap<Basket, BasketDto>();
            cfg.CreateMap<BasketItem, BasketItemDto>();
        }).CreateMapper();

        _addItemHandler = new AddItemToBasketCommandHandler(_redisService, mapper, _mockProductService.Object);
        _getBasketHandler = new GetBasketQueryHandler(_redisService, mapper);
    }

    [Fact]
    public async Task AddItemToBasket_Should_GetProductFromProductService()
    {
        // Arrange
        var productId = "test-product-1";
        var userId = "test-user-1";
        var product = new ProductDto
        {
            Id = productId,
            Name = "Test Product",
            Price = 100,
            Description = "Test Description",
            Stock = 10
        };

        _mockProductService.Setup(x => x.GetProductById(productId))
            .ReturnsAsync(product);

        var command = new AddItemToBasketCommand
        {
            UserId = userId,
            Item = new BasketItemDto
            {
                ProductId = productId,
                ProductName = product.Name,
                Quantity = 1
            }
        };

        // Act
        var result = await _addItemHandler.Handle(command, CancellationToken.None);

        // Assert
        _mockProductService.Verify(x => x.GetProductById(productId), Times.Once);
        Assert.NotNull(result);
        Assert.Equal(userId, result.UserId);
        Assert.Single(result.Items);
        Assert.Equal(productId, result.Items[0].ProductId);
        Assert.Equal(product.Name, result.Items[0].ProductName);
        Assert.Equal(product.Price, result.Items[0].Price);
    }

    [Fact]
    public async Task GetBasket_Should_ReturnBasketFromRedis()
    {
        // Arrange
        var userId = "test-user-1";
        var basket = new Basket
        {
            UserId = userId,
            Items = new List<BasketItem>
            {
                new BasketItem
                {
                    ProductId = "test-product-1",
                    ProductName = "Test Product",
                    Price = 100,
                    Quantity = 1
                }
            }
        };

        var basketJson = JsonSerializer.Serialize(basket);
        var basketBytes = System.Text.Encoding.UTF8.GetBytes(basketJson);

        _mockCache.Setup(x => x.GetAsync($"basket:{userId}", It.IsAny<CancellationToken>()))
            .ReturnsAsync(basketBytes);

        var query = new GetBasketQuery { UserId = userId };

        // Act
        var result = await _getBasketHandler.Handle(query, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(userId, result.UserId);
        Assert.Single(result.Items);
        Assert.Equal("test-product-1", result.Items[0].ProductId);
        Assert.Equal("Test Product", result.Items[0].ProductName);
        Assert.Equal(100, result.Items[0].Price);
        Assert.Equal(1, result.Items[0].Quantity);
    }

    [Fact]
    public async Task AddItemToBasket_Should_UpdateExistingItemQuantity()
    {
        // Arrange
        var productId = "test-product-1";
        var userId = "test-user-1";
        var product = new ProductDto
        {
            Id = productId,
            Name = "Test Product",
            Price = 100,
            Description = "Test Description",
            Stock = 10
        };

        _mockProductService.Setup(x => x.GetProductById(productId))
            .ReturnsAsync(product);

        var existingBasket = new Basket
        {
            UserId = userId,
            Items = new List<BasketItem>
            {
                new BasketItem
                {
                    ProductId = productId,
                    ProductName = product.Name,
                    Price = product.Price,
                    Quantity = 1
                }
            }
        };

        var existingBasketJson = JsonSerializer.Serialize(existingBasket);
        var existingBasketBytes = System.Text.Encoding.UTF8.GetBytes(existingBasketJson);

        _mockCache.Setup(x => x.GetAsync($"basket:{userId}", It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingBasketBytes);

        var command = new AddItemToBasketCommand
        {
            UserId = userId,
            Item = new BasketItemDto
            {
                ProductId = productId,
                ProductName = product.Name,
                Quantity = 2
            }
        };

        // Act
        var result = await _addItemHandler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(userId, result.UserId);
        Assert.Single(result.Items);
        Assert.Equal(productId, result.Items[0].ProductId);
        Assert.Equal(3, result.Items[0].Quantity); // 1 + 2 = 3
    }
}
