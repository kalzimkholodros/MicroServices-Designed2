using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Moq;
using PaymentService.Application.DTOs;
using PaymentService.Application.Features.Payment.Commands.CreatePayment;
using PaymentService.Domain.Entities;
using PaymentService.Infrastructure.Data;
using RabbitMQCommunication.Events;
using RabbitMQCommunication.Services;
using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace PaymentTest;

public class PaymentServiceTests : IDisposable
{
    private readonly ApplicationDbContext _context;
    private readonly IMapper _mapper;
    private readonly Mock<IRabbitMQService> _rabbitMQServiceMock;
    private readonly CreatePaymentCommandHandler _handler;

    public PaymentServiceTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new ApplicationDbContext(options);

        var config = new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<CreatePaymentDto, PaymentService.Domain.Entities.Payment>();
            cfg.CreateMap<PaymentService.Domain.Entities.Payment, PaymentDto>();
        });
        _mapper = config.CreateMapper();

        _rabbitMQServiceMock = new Mock<IRabbitMQService>();
        _handler = new CreatePaymentCommandHandler(_context, _mapper, _rabbitMQServiceMock.Object);
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }

    [Fact]
    public async Task CreatePayment_Should_Succeed()
    {
        // Arrange
        var request = new CreatePaymentCommand
        {
            Payment = new CreatePaymentDto
            {
                OrderId = Guid.NewGuid(),
                UserId = "user1",
                Amount = 100
            }
        };

        // Act
        var result = await _handler.Handle(request, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(request.Payment.OrderId, result.OrderId);
        Assert.Equal(request.Payment.UserId, result.UserId);
        Assert.Equal(request.Payment.Amount, result.Amount);
        Assert.Equal("Completed", result.Status.ToString());

        _rabbitMQServiceMock.Verify(x => x.Publish("payment-completed", It.IsAny<PaymentCompletedEvent>()), Times.Once);
    }

    [Fact]
    public async Task CreatePayment_Should_HandleInvalidAmount()
    {
        // Arrange
        var request = new CreatePaymentCommand
        {
            Payment = new CreatePaymentDto
            {
                OrderId = Guid.NewGuid(),
                UserId = "user1",
                Amount = -100
            }
        };

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(() => _handler.Handle(request, CancellationToken.None));
        Assert.Equal("Payment amount must be greater than zero.", exception.Message);
    }

    [Fact]
    public async Task CreatePayment_Should_HandleConcurrentPayments()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var request = new CreatePaymentCommand
        {
            Payment = new CreatePaymentDto
            {
                OrderId = orderId,
                UserId = "user1",
                Amount = 100
            }
        };

        // Act
        var tasks = Enumerable.Range(0, 5)
            .Select(_ => Task.Run(() => _handler.Handle(request, CancellationToken.None)))
            .ToArray();

        var results = await Task.WhenAll(tasks);

        // Assert
        var successfulPayments = results.Where(r => r != null).ToList();
        Assert.Single(successfulPayments);
        Assert.Equal(orderId, successfulPayments[0].OrderId);
    }

    [Fact]
    public async Task CreatePayment_Should_HandlePaymentFailure()
    {
        // Arrange
        var request = new CreatePaymentCommand
        {
            Payment = new CreatePaymentDto
            {
                OrderId = Guid.Parse("70e6e74f-1395-45e6-a2d4-91e81d7b96eb"), // Test için özel OrderId
                UserId = "user1",
                Amount = 100
            }
        };

        // Act
        var result = await _handler.Handle(request, CancellationToken.None);

        // Assert
        Assert.Null(result);
    }
}
