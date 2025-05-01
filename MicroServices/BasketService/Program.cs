using BasketService.Application.Features.Basket.Commands.AddItemToBasket;
using BasketService.Infrastructure.Services;
using RabbitMQCommunication.Services;
using System.Reflection;

namespace BasketService
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
            builder.Services.AddControllers();

            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            // Add Redis
            builder.Services.AddStackExchangeRedisCache(options =>
            {
                options.Configuration = builder.Configuration.GetConnectionString("Redis");
                options.InstanceName = "BasketService_";
            });

            // Add MediatR
            builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(AddItemToBasketCommand).Assembly));

            // Add AutoMapper
            builder.Services.AddAutoMapper(typeof(AddItemToBasketCommand).Assembly);

            // Add Redis Service
            builder.Services.AddSingleton<IRedisService, RedisService>();

            // Add RabbitMQ Service
            builder.Services.AddSingleton<IRabbitMQService, RabbitMQCommunication.Services.RabbitMQService>();

            // Add PaymentCompletedEventHandler
            builder.Services.AddSingleton<PaymentCompletedEventHandler>();

            // Add ProductService
            builder.Services.AddHttpClient<IProductService, ProductService>(client =>
            {
                client.BaseAddress = new Uri("http://localhost:5043"); // ProductService URL
            });

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI(c =>
                {
                    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Basket Service API V1");
                    c.RoutePrefix = string.Empty;
                });
            }

            app.UseHttpsRedirection();

            app.UseAuthorization();

            app.MapControllers();

            // Start listening to payment completed events
            var paymentCompletedEventHandler = app.Services.GetRequiredService<PaymentCompletedEventHandler>();
            paymentCompletedEventHandler.StartListening();

            app.Run();
        }
    }
}

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
