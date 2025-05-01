using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using OrderService.Application.DTOs;
using OrderService.Domain.Entities;
using OrderService.Infrastructure.Data;
using OrderService.Infrastructure.Services;

namespace OrderService.Application.Features.Order.Commands.CreateOrder;

public class CreateOrderCommandHandler : IRequestHandler<CreateOrderCommand, OrderDto>
{
    private readonly ApplicationDbContext _context;
    private readonly IMapper _mapper;
    private readonly IBasketService _basketService;

    public CreateOrderCommandHandler(ApplicationDbContext context, IMapper mapper, IBasketService basketService)
    {
        _context = context;
        _mapper = mapper;
        _basketService = basketService;
    }

    public async Task<OrderDto> Handle(CreateOrderCommand request, CancellationToken cancellationToken)
    {
        // Get basket from BasketService
        var basket = await _basketService.GetBasket(request.Order.UserId);

        // Check if basket is empty
        if (basket.Items.Count == 0)
        {
            throw new Exception("Cannot create order with empty basket");
        }

        // Create order
        var order = new Domain.Entities.Order
        {
            Id = Guid.NewGuid(),
            UserId = request.Order.UserId,
            UserName = request.Order.UserName,
            UserEmail = request.Order.UserEmail,
            Address = request.Order.Address,
            Status = OrderStatus.Pending,
            CreatedAt = DateTime.UtcNow,
            TotalPrice = basket.TotalPrice
        };

        // Add order items
        foreach (var basketItem in basket.Items)
        {
            order.Items.Add(new OrderItem
            {
                Id = Guid.NewGuid(),
                ProductId = basketItem.ProductId,
                ProductName = basketItem.ProductName,
                Price = basketItem.Price,
                Quantity = basketItem.Quantity
            });
        }

        // Save order
        await _context.Orders.AddAsync(order, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        return _mapper.Map<OrderDto>(order);
    }
} 