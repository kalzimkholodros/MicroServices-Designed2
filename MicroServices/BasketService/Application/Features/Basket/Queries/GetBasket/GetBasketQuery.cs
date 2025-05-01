using MediatR;
using BasketService.Application.DTOs;

namespace BasketService.Application.Features.Basket.Queries.GetBasket;

public class GetBasketQuery : IRequest<BasketDto>
{
    public required string UserId { get; set; }
} 