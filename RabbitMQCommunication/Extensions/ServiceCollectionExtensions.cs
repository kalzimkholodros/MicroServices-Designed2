using Microsoft.Extensions.DependencyInjection;
using RabbitMQCommunication.Services;

namespace RabbitMQCommunication.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddRabbitMQ(this IServiceCollection services)
    {
        services.AddSingleton<IRabbitMQService, RabbitMQService>();
        return services;
    }
} 