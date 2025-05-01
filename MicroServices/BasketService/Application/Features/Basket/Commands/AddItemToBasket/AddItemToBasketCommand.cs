using MediatR;
using BasketService.Application.DTOs;

namespace BasketService.Application.Features.Basket.Commands.AddItemToBasket;

public class AddItemToBasketCommand : IRequest<BasketDto>
{
    public required string UserId { get; set; }
    public required BasketItemDto Item { get; set; }
} 