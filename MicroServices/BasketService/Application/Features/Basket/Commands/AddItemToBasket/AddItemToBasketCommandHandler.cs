using AutoMapper;
using BasketService.Application.DTOs;
using BasketService.Domain.Entities;
using BasketService.Infrastructure.Services;
using MediatR;

namespace BasketService.Application.Features.Basket.Commands.AddItemToBasket;

public class AddItemToBasketCommandHandler : IRequestHandler<AddItemToBasketCommand, BasketDto>
{
    private readonly IRedisService _redisService;
    private readonly IMapper _mapper;
    private readonly IProductService _productService;

    public AddItemToBasketCommandHandler(IRedisService redisService, IMapper mapper, IProductService productService)
    {
        _redisService = redisService;
        _mapper = mapper;
        _productService = productService;
    }

    public async Task<BasketDto> Handle(AddItemToBasketCommand request, CancellationToken cancellationToken)
    {
        var basketKey = $"basket:{request.UserId}";
        var basket = await _redisService.GetAsync<Domain.Entities.Basket>(basketKey) ?? new Domain.Entities.Basket { UserId = request.UserId };

        var product = await _productService.GetProductById(request.Item.ProductId);
        var basketItem = _mapper.Map<BasketItem>(request.Item);
        basketItem.ProductName = product.Name;
        basketItem.Price = product.Price;

        var existingItem = basket.Items.FirstOrDefault(x => x.ProductId == request.Item.ProductId);
        if (existingItem != null)
        {
            existingItem.Quantity += request.Item.Quantity;
        }
        else
        {
            basket.Items.Add(basketItem);
        }

        await _redisService.SetAsync(basketKey, basket, TimeSpan.FromDays(1));
        return _mapper.Map<BasketDto>(basket);
    }
} 