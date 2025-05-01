using AutoMapper;
using BasketService.Application.DTOs;
using BasketService.Domain.Entities;
using BasketService.Infrastructure.Services;
using MediatR;

namespace BasketService.Application.Features.Basket.Queries.GetBasket;

public class GetBasketQueryHandler : IRequestHandler<GetBasketQuery, BasketDto>
{
    private readonly IRedisService _redisService;
    private readonly IMapper _mapper;

    public GetBasketQueryHandler(IRedisService redisService, IMapper mapper)
    {
        _redisService = redisService;
        _mapper = mapper;
    }

    public async Task<BasketDto> Handle(GetBasketQuery request, CancellationToken cancellationToken)
    {
        var basketKey = $"basket:{request.UserId}";
        var basket = await _redisService.GetAsync<Domain.Entities.Basket>(basketKey);

        if (basket == null)
        {
            basket = new Domain.Entities.Basket { UserId = request.UserId };
            await _redisService.SetAsync(basketKey, basket, TimeSpan.FromDays(1));
        }

        return _mapper.Map<BasketDto>(basket);
    }
} 