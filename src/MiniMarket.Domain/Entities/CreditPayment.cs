using MiniMarket.Domain.Common;
using MiniMarket.Domain.Enums;
using MiniMarket.Domain.ValueObjects;

namespace MiniMarket.Domain.Entities;

public class CreditPayment : Entity<Guid>
{
    public Guid CustomerId { get; private set; }
    public Money Amount { get; private set; } = default!;
    public PaymentMethod PaymentMethod { get; private set; }
    public string? Reference { get; private set; }
    public string? Notes { get; private set; }

    private CreditPayment() : base()
    {
    }

    internal static CreditPayment Create(
        Guid customerId,
        Money amount,
        PaymentMethod method,
        string? reference = null,
        string? notes = null)
    {
        return new CreditPayment
        {
            Id = Guid.NewGuid(),
            CustomerId = customerId,
            Amount = amount,
            PaymentMethod = method,
            Reference = reference?.Trim(),
            Notes = notes?.Trim()
        };
    }
}
