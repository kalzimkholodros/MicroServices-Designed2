using MediatR;
using ProductService.Application.DTOs;

namespace ProductService.Application.Features.Products.Queries.GetAllProducts;

public class GetAllProductsQuery : IRequest<List<ProductDto>>
{
} 