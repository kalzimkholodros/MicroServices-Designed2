using Microsoft.EntityFrameworkCore;
using ProductService.Domain.Entities;
using ProductService.Infrastructure.Data;
using RabbitMQCommunication.Events;
using RabbitMQCommunication.Services;

namespace ProductService.Infrastructure.Services;

public class PaymentCompletedEventHandler
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IRabbitMQService _rabbitMQService;

    public PaymentCompletedEventHandler(IServiceScopeFactory scopeFactory, IRabbitMQService rabbitMQService)
    {
        _scopeFactory = scopeFactory;
        _rabbitMQService = rabbitMQService;
    }

    public void StartListening()
    {
        _rabbitMQService.Subscribe<PaymentCompletedEvent>("payment-completed", HandlePaymentCompleted);
    }

    private async void HandlePaymentCompleted(PaymentCompletedEvent @event)
    {
        using var scope = _scopeFactory.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        foreach (var item in @event.Items)
        {
            var product = await context.Products.FirstOrDefaultAsync(p => p.Id.ToString() == item.ProductId);
            if (product != null)
            {
                product.Stock -= item.Quantity;
                await context.SaveChangesAsync();
            }
        }
    }
} 