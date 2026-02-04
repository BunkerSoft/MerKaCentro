using MiniMarket.Domain.Common;
using MiniMarket.Domain.Enums;
using MiniMarket.Domain.ValueObjects;

namespace MiniMarket.Domain.Entities;

public class StockMovement : Entity<Guid>
{
    public Guid ProductId { get; private set; }
    public MovementType Type { get; private set; }
    public Quantity Quantity { get; private set; } = default!;
    public Quantity StockAfter { get; private set; } = default!;
    public string? Reference { get; private set; }
    public string? Notes { get; private set; }

    private StockMovement() : base()
    {
    }

    internal static StockMovement Create(
        Guid productId,
        MovementType type,
        Quantity quantity,
        Quantity stockAfter,
        string? reference = null,
        string? notes = null)
    {
        return new StockMovement
        {
            Id = Guid.NewGuid(),
            ProductId = productId,
            Type = type,
            Quantity = quantity,
            StockAfter = stockAfter,
            Reference = reference,
            Notes = notes
        };
    }
}
