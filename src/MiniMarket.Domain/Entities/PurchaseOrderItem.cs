using MiniMarket.Domain.Common;
using MiniMarket.Domain.Exceptions;
using MiniMarket.Domain.ValueObjects;

namespace MiniMarket.Domain.Entities;

public class PurchaseOrderItem : Entity<Guid>
{
    public Guid PurchaseOrderId { get; private set; }
    public Guid ProductId { get; private set; }
    public string ProductName { get; private set; } = default!;
    public Quantity Quantity { get; private set; } = default!;
    public Quantity ReceivedQuantity { get; private set; } = default!;
    public Money UnitCost { get; private set; } = default!;
    public Money Total { get; private set; } = default!;

    private PurchaseOrderItem() : base()
    {
    }

    internal static PurchaseOrderItem Create(
        Guid purchaseOrderId,
        Guid productId,
        string productName,
        Quantity quantity,
        Money unitCost)
    {
        return new PurchaseOrderItem
        {
            Id = Guid.NewGuid(),
            PurchaseOrderId = purchaseOrderId,
            ProductId = productId,
            ProductName = productName,
            Quantity = quantity,
            ReceivedQuantity = Quantity.Zero(),
            UnitCost = unitCost,
            Total = unitCost.Multiply(quantity)
        };
    }

    internal void Receive(Quantity quantity)
    {
        if (quantity <= Quantity.Zero())
        {
            throw new DomainException("La cantidad recibida debe ser mayor a cero");
        }

        Quantity newReceived = ReceivedQuantity.Add(quantity);
        if (newReceived > Quantity)
        {
            throw new DomainException("La cantidad recibida excede la cantidad ordenada");
        }

        ReceivedQuantity = newReceived;
        SetUpdated();
    }

    public bool IsFullyReceived() => ReceivedQuantity >= Quantity;

    public Quantity GetPendingQuantity() => Quantity.Subtract(ReceivedQuantity);
}
