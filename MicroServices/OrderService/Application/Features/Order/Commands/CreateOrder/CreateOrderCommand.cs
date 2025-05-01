using MediatR;
using OrderService.Application.DTOs;

namespace OrderService.Application.Features.Order.Commands.CreateOrder;

public class CreateOrderCommand : IRequest<OrderDto>
{
    public required CreateOrderDto Order { get; set; }
} 