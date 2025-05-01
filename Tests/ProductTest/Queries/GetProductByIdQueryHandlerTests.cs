using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Moq;
using ProductService.Application.DTOs;
using ProductService.Application.Features.Products.Queries.GetProductById;
using ProductService.Domain.Entities;
using ProductService.Infrastructure.Data;
using Xunit;

namespace ProductTest.Queries;

public class GetProductByIdQueryHandlerTests
{
    private readonly Mock<IMapper> _mapperMock;
    private readonly ApplicationDbContext _context;
    private readonly GetProductByIdQueryHandler _handler;

    public GetProductByIdQueryHandlerTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: "TestDb_" + Guid.NewGuid().ToString())
            .Options;

        _context = new ApplicationDbContext(options);
        _mapperMock = new Mock<IMapper>();
        _handler = new GetProductByIdQueryHandler(_context, _mapperMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnProduct_WhenProductExists()
    {
        // Arrange
        var existingProduct = new Product
        {
            Id = Guid.NewGuid(),
            Name = "Test Product",
            Description = "Test Description",
            Price = 99.99m,
            Stock = 10,
            CreatedDate = DateTime.UtcNow
        };

        await _context.Products.AddAsync(existingProduct);
        await _context.SaveChangesAsync();

        var query = new GetProductByIdQuery { Id = existingProduct.Id };

        _mapperMock.Setup(m => m.Map<ProductDto>(It.IsAny<Product>()))
            .Returns(new ProductDto
            {
                Id = existingProduct.Id,
                Name = existingProduct.Name,
                Description = existingProduct.Description,
                Price = existingProduct.Price,
                Stock = existingProduct.Stock,
                CreatedDate = existingProduct.CreatedDate
            });

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(existingProduct.Id, result.Id);
        Assert.Equal(existingProduct.Name, result.Name);
        Assert.Equal(existingProduct.Description, result.Description);
        Assert.Equal(existingProduct.Price, result.Price);
        Assert.Equal(existingProduct.Stock, result.Stock);
    }

    [Fact]
    public async Task Handle_ShouldThrowKeyNotFoundException_WhenProductDoesNotExist()
    {
        // Arrange
        var query = new GetProductByIdQuery { Id = Guid.NewGuid() };

        // Act & Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            _handler.Handle(query, CancellationToken.None));
    }
} 