using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Moq;
using OrderService.Application.DTOs;
using OrderService.Application.Features.Order.Commands.CreateOrder;
using OrderService.Domain.Entities;
using OrderService.Infrastructure.Data;
using OrderService.Infrastructure.Services;
using System.Text.Json;
using Xunit;

namespace OrderTest;

public class UnitTest1
{
    private readonly ApplicationDbContext _context;
    private readonly IMapper _mapper;
    private readonly Mock<IBasketService> _mockBasketService;

    public UnitTest1()
    {
        // Setup in-memory database
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: "OrderTestDb")
            .Options;
        _context = new ApplicationDbContext(options);

        // Setup AutoMapper
        var config = new MapperConfiguration(cfg =>
        {
            cfg.AddProfile<OrderService.Application.Mapping.OrderProfile>();
        });
        _mapper = config.CreateMapper();

        // Setup mock BasketService
        _mockBasketService = new Mock<IBasketService>();
    }

    [Fact]
    public async Task CreateOrder_Should_CreateOrderFromBasket()
    {
        // Arrange
        var userId = "user1";
        var basket = new BasketDto
        {
            UserId = userId,
            Items = new List<BasketItemDto>
            {
                new BasketItemDto
                {
                    ProductId = "product1",
                    ProductName = "Test Product 1",
                    Price = 100,
                    Quantity = 2
                },
                new BasketItemDto
                {
                    ProductId = "product2",
                    ProductName = "Test Product 2",
                    Price = 200,
                    Quantity = 1
                }
            },
            TotalPrice = 400
        };

        _mockBasketService.Setup(x => x.GetBasket(userId))
            .ReturnsAsync(basket);

        var command = new CreateOrderCommand
        {
            Order = new CreateOrderDto
            {
                UserId = userId,
                UserName = "Test User",
                UserEmail = "test@example.com",
                Address = "Test Address"
            }
        };

        var handler = new CreateOrderCommandHandler(_context, _mapper, _mockBasketService.Object);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(userId, result.UserId);
        Assert.Equal("Test User", result.UserName);
        Assert.Equal("test@example.com", result.UserEmail);
        Assert.Equal("Test Address", result.Address);
        Assert.Equal(400, result.TotalPrice);
        Assert.Equal("Pending", result.Status);
        Assert.Equal(2, result.Items.Count);

        var savedOrder = await _context.Orders
            .Include(o => o.Items)
            .FirstOrDefaultAsync(o => o.Id == result.Id);

        Assert.NotNull(savedOrder);
        Assert.Equal(2, savedOrder.Items.Count);
        Assert.Equal("product1", savedOrder.Items[0].ProductId);
        Assert.Equal(2, savedOrder.Items[0].Quantity);
        Assert.Equal("product2", savedOrder.Items[1].ProductId);
        Assert.Equal(1, savedOrder.Items[1].Quantity);
    }

    [Fact]
    public async Task CreateOrder_Should_ThrowException_WhenBasketIsEmpty()
    {
        // Arrange
        var userId = "user1";
        var emptyBasket = new BasketDto
        {
            UserId = userId,
            Items = new List<BasketItemDto>(),
            TotalPrice = 0
        };

        _mockBasketService.Setup(x => x.GetBasket(userId))
            .ReturnsAsync(emptyBasket);

        var command = new CreateOrderCommand
        {
            Order = new CreateOrderDto
            {
                UserId = userId,
                UserName = "Test User",
                UserEmail = "test@example.com",
                Address = "Test Address"
            }
        };

        var handler = new CreateOrderCommandHandler(_context, _mapper, _mockBasketService.Object);

        // Act & Assert
        await Assert.ThrowsAsync<Exception>(() => handler.Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task CreateOrder_Should_HandleBasketServiceError()
    {
        // Arrange
        var userId = "user1";
        _mockBasketService.Setup(x => x.GetBasket(userId))
            .ThrowsAsync(new Exception("Basket service error"));

        var command = new CreateOrderCommand
        {
            Order = new CreateOrderDto
            {
                UserId = userId,
                UserName = "Test User",
                UserEmail = "test@example.com",
                Address = "Test Address"
            }
        };

        var handler = new CreateOrderCommandHandler(_context, _mapper, _mockBasketService.Object);

        // Act & Assert
        await Assert.ThrowsAsync<Exception>(() => handler.Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task CreateOrder_Should_UpdateExistingOrder()
    {
        // Arrange
        var userId = "user1";
        var existingOrder = new OrderService.Domain.Entities.Order
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            UserName = "Existing User",
            UserEmail = "existing@example.com",
            Address = "Existing Address",
            Status = OrderStatus.Pending,
            CreatedAt = DateTime.UtcNow,
            TotalPrice = 100
        };

        await _context.Orders.AddAsync(existingOrder);
        await _context.SaveChangesAsync();

        var basket = new BasketDto
        {
            UserId = userId,
            Items = new List<BasketItemDto>
            {
                new BasketItemDto
                {
                    ProductId = "product1",
                    ProductName = "Test Product 1",
                    Price = 100,
                    Quantity = 2
                }
            },
            TotalPrice = 200
        };

        _mockBasketService.Setup(x => x.GetBasket(userId))
            .ReturnsAsync(basket);

        var command = new CreateOrderCommand
        {
            Order = new CreateOrderDto
            {
                UserId = userId,
                UserName = "Updated User",
                UserEmail = "updated@example.com",
                Address = "Updated Address"
            }
        };

        var handler = new CreateOrderCommandHandler(_context, _mapper, _mockBasketService.Object);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(userId, result.UserId);
        Assert.Equal("Updated User", result.UserName);
        Assert.Equal("updated@example.com", result.UserEmail);
        Assert.Equal("Updated Address", result.Address);
        Assert.Equal(200, result.TotalPrice);
        Assert.Equal("Pending", result.Status);
        Assert.Single(result.Items);

        var updatedOrder = await _context.Orders
            .Include(o => o.Items)
            .FirstOrDefaultAsync(o => o.Id == result.Id);

        Assert.NotNull(updatedOrder);
        Assert.Single(updatedOrder.Items);
        Assert.Equal("product1", updatedOrder.Items[0].ProductId);
        Assert.Equal(2, updatedOrder.Items[0].Quantity);
    }
}
