using MediatR;
using ProductService.Application.DTOs;

namespace ProductService.Application.Features.Products.Queries.GetProductById;

public class GetProductByIdQuery : IRequest<ProductDto>
{
    public Guid Id { get; set; }
} 