using MiniMarket.Domain.Common;
using MiniMarket.Domain.Enums;
using MiniMarket.Domain.ValueObjects;

namespace MiniMarket.Domain.Entities;

public class CashMovement : Entity<Guid>
{
    public Guid CashRegisterId { get; private set; }
    public CashMovementType Type { get; private set; }
    public Money Amount { get; private set; } = default!;
    public Money BalanceAfter { get; private set; } = default!;
    public string Description { get; private set; } = default!;

    private CashMovement() : base()
    {
    }

    internal static CashMovement Create(
        Guid cashRegisterId,
        CashMovementType type,
        Money amount,
        Money balanceAfter,
        string description)
    {
        return new CashMovement
        {
            Id = Guid.NewGuid(),
            CashRegisterId = cashRegisterId,
            Type = type,
            Amount = amount,
            BalanceAfter = balanceAfter,
            Description = description
        };
    }
}
