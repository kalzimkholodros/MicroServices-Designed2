using Microsoft.EntityFrameworkCore;
using ProductService.Application.Features.Products.Commands.CreateProduct;
using ProductService.Infrastructure.Data;
using ProductService.Infrastructure.Services;
using RabbitMQCommunication.Services;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add DbContext
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Add MediatR
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(CreateProductCommand).Assembly));

// Add AutoMapper
builder.Services.AddAutoMapper(typeof(CreateProductCommand).Assembly);

// Add RabbitMQ Service
builder.Services.AddSingleton<IRabbitMQService, RabbitMQCommunication.Services.RabbitMQService>();

// Add PaymentCompletedEventHandler
builder.Services.AddSingleton<PaymentCompletedEventHandler>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Product Service API V1");
        c.RoutePrefix = string.Empty;
    });
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

// Ensure database is created
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    db.Database.EnsureCreated();
}

// Start listening to payment completed events
var paymentCompletedEventHandler = app.Services.GetRequiredService<PaymentCompletedEventHandler>();
paymentCompletedEventHandler.StartListening();

app.Run();
