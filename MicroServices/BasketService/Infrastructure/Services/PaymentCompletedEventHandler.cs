using BasketService.Infrastructure.Services;
using RabbitMQCommunication.Events;
using RabbitMQCommunication.Services;

namespace BasketService.Infrastructure.Services;

public class PaymentCompletedEventHandler
{
    private readonly IRedisService _redisService;
    private readonly IRabbitMQService _rabbitMQService;

    public PaymentCompletedEventHandler(IRedisService redisService, IRabbitMQService rabbitMQService)
    {
        _redisService = redisService;
        _rabbitMQService = rabbitMQService;
    }

    public void StartListening()
    {
        _rabbitMQService.Subscribe<PaymentCompletedEvent>("payment-completed", HandlePaymentCompleted);
    }

    private async void HandlePaymentCompleted(PaymentCompletedEvent @event)
    {
        // Clear the basket
        await _redisService.DeleteAsync($"basket:{@event.UserId}");
    }
} 