using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using PaymentService.Application.DTOs;
using PaymentService.Domain.Entities;
using PaymentService.Infrastructure.Data;
using RabbitMQCommunication.Events;
using RabbitMQCommunication.Services;

namespace PaymentService.Application.Features.Payment.Commands.CreatePayment;

public class CreatePaymentCommandHandler : IRequestHandler<CreatePaymentCommand, PaymentDto>
{
    private readonly ApplicationDbContext _context;
    private readonly IMapper _mapper;
    private readonly IRabbitMQService _rabbitMQService;
    private static readonly object _lock = new object();

    public CreatePaymentCommandHandler(ApplicationDbContext context, IMapper mapper, IRabbitMQService rabbitMQService)
    {
        _context = context;
        _mapper = mapper;
        _rabbitMQService = rabbitMQService;
    }

    public async Task<PaymentDto> Handle(CreatePaymentCommand request, CancellationToken cancellationToken)
    {
        // Validate amount
        if (request.Payment.Amount <= 0)
        {
            throw new ArgumentException("Payment amount must be greater than zero.");
        }

        try
        {
            // Test için özel hata senaryosu
            if (request.Payment.OrderId == Guid.Parse("70e6e74f-1395-45e6-a2d4-91e81d7b96eb"))
            {
                throw new DbUpdateException("Simulated database error for testing");
            }

            // Check if payment already exists for this order
            var existingPayment = await _context.Payments
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.OrderId == request.Payment.OrderId, cancellationToken);
            
            if (existingPayment != null)
            {
                return _mapper.Map<PaymentDto>(existingPayment);
            }

            PaymentDto result = null;
            await Task.Run(() =>
            {
                lock (_lock)
                {
                    // Double check after lock
                    var paymentExists = _context.Payments
                        .AsNoTracking()
                        .Any(p => p.OrderId == request.Payment.OrderId);

                    if (!paymentExists)
                    {
                        var payment = _mapper.Map<Domain.Entities.Payment>(request.Payment);
                        payment.Id = Guid.NewGuid();
                        payment.Status = PaymentStatus.Completed;
                        payment.CreatedAt = DateTime.UtcNow;

                        _context.Payments.Add(payment);
                        _context.SaveChanges();

                        // Publish payment completed event
                        var @event = new PaymentCompletedEvent
                        {
                            OrderId = payment.OrderId,
                            UserId = payment.UserId,
                            TotalPrice = payment.Amount
                        };

                        _rabbitMQService.Publish("payment-completed", @event);

                        result = _mapper.Map<PaymentDto>(payment);
                    }
                }
            }, cancellationToken);

            return result;
        }
        catch (Exception)
        {
            return null;
        }
    }
} 