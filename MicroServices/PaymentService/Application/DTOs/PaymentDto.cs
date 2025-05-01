namespace PaymentService.Application.DTOs;

public class PaymentDto
{
    public Guid Id { get; set; }
    public Guid OrderId { get; set; }
    public required string UserId { get; set; }
    public decimal Amount { get; set; }
    public required string Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class CreatePaymentDto
{
    public Guid OrderId { get; set; }
    public required string UserId { get; set; }
    public decimal Amount { get; set; }
} 