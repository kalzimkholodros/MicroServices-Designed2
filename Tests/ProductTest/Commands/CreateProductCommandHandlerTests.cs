using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Moq;
using ProductService.Application.DTOs;
using ProductService.Application.Features.Products.Commands.CreateProduct;
using ProductService.Domain.Entities;
using ProductService.Infrastructure.Data;
using Xunit;

namespace ProductTest.Commands;

public class CreateProductCommandHandlerTests
{
    private readonly Mock<IMapper> _mapperMock;
    private readonly ApplicationDbContext _context;
    private readonly CreateProductCommandHandler _handler;

    public CreateProductCommandHandlerTests()
    {
        // InMemory veritabanı oluştur
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: "TestDb_" + Guid.NewGuid().ToString())
            .Options;

        _context = new ApplicationDbContext(options);
        _mapperMock = new Mock<IMapper>();
        _handler = new CreateProductCommandHandler(_context, _mapperMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldCreateProduct_WhenValidCommand()
    {
        // Arrange
        var command = new CreateProductCommand
        {
            Name = "Test Product",
            Description = "Test Description",
            Price = 99.99m,
            Stock = 10
        };

        var expectedProduct = new Product
        {
            Id = Guid.NewGuid(),
            Name = command.Name,
            Description = command.Description,
            Price = command.Price,
            Stock = command.Stock,
            CreatedDate = DateTime.UtcNow
        };

        _mapperMock.Setup(m => m.Map<ProductDto>(It.IsAny<Product>()))
            .Returns(new ProductDto
            {
                Id = expectedProduct.Id,
                Name = expectedProduct.Name,
                Description = expectedProduct.Description,
                Price = expectedProduct.Price,
                Stock = expectedProduct.Stock,
                CreatedDate = expectedProduct.CreatedDate
            });

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(command.Name, result.Name);
        Assert.Equal(command.Description, result.Description);
        Assert.Equal(command.Price, result.Price);
        Assert.Equal(command.Stock, result.Stock);

        var savedProduct = await _context.Products.FirstOrDefaultAsync();
        Assert.NotNull(savedProduct);
        Assert.Equal(command.Name, savedProduct.Name);
        Assert.Equal(command.Description, savedProduct.Description);
        Assert.Equal(command.Price, savedProduct.Price);
        Assert.Equal(command.Stock, savedProduct.Stock);
    }

    [Fact]
    public async Task Handle_ShouldSetCreatedDate_WhenCreatingProduct()
    {
        // Arrange
        var command = new CreateProductCommand
        {
            Name = "Test Product",
            Description = "Test Description",
            Price = 99.99m,
            Stock = 10
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        var savedProduct = await _context.Products.FirstOrDefaultAsync();
        Assert.NotNull(savedProduct);
        Assert.True(savedProduct.CreatedDate <= DateTime.UtcNow);
        Assert.True(savedProduct.CreatedDate > DateTime.UtcNow.AddMinutes(-1));
    }
} 