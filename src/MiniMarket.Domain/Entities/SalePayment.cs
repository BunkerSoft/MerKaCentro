using MiniMarket.Domain.Common;
using MiniMarket.Domain.Enums;
using MiniMarket.Domain.ValueObjects;

namespace MiniMarket.Domain.Entities;

public class SalePayment : Entity<Guid>
{
    public Guid SaleId { get; private set; }
    public PaymentMethod Method { get; private set; }
    public Money Amount { get; private set; } = default!;
    public string? Reference { get; private set; }

    private SalePayment() : base()
    {
    }

    internal static SalePayment Create(
        Guid saleId,
        PaymentMethod method,
        Money amount,
        string? reference = null)
    {
        return new SalePayment
        {
            Id = Guid.NewGuid(),
            SaleId = saleId,
            Method = method,
            Amount = amount,
            Reference = reference?.Trim()
        };
    }
}
