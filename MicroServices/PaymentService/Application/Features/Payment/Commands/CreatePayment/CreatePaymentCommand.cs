using MediatR;
using PaymentService.Application.DTOs;

namespace PaymentService.Application.Features.Payment.Commands.CreatePayment;

public class CreatePaymentCommand : IRequest<PaymentDto>
{
    public required CreatePaymentDto Payment { get; set; }
} 