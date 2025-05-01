using BasketService.Application.DTOs;
using BasketService.Application.Features.Basket.Commands.AddItemToBasket;
using BasketService.Application.Features.Basket.Queries.GetBasket;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace BasketService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class BasketController : ControllerBase
{
    private readonly IMediator _mediator;

    public BasketController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("{userId}")]
    public async Task<ActionResult<BasketDto>> GetBasket(string userId)
    {
        var query = new GetBasketQuery { UserId = userId };
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    [HttpPost("add-item")]
    public async Task<ActionResult<BasketDto>> AddItem([FromBody] AddItemToBasketCommand command)
    {
        var result = await _mediator.Send(command);
        return Ok(result);
    }
} 