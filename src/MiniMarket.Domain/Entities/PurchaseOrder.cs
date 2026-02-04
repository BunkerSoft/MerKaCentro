using MiniMarket.Domain.Common;
using MiniMarket.Domain.Enums;
using MiniMarket.Domain.Exceptions;
using MiniMarket.Domain.ValueObjects;

namespace MiniMarket.Domain.Entities;

public class PurchaseOrder : AggregateRoot<Guid>
{
    public string Number { get; private set; } = default!;
    public Guid SupplierId { get; private set; }
    public Supplier? Supplier { get; private set; }
    public Guid UserId { get; private set; }
    public User? User { get; private set; }
    public Money Total { get; private set; } = default!;
    public PurchaseOrderStatus Status { get; private set; }
    public DateTime? ReceivedAt { get; private set; }
    public string? Notes { get; private set; }

    private readonly List<PurchaseOrderItem> _items = [];
    public IReadOnlyCollection<PurchaseOrderItem> Items => _items.AsReadOnly();

    private PurchaseOrder() : base()
    {
    }

    public static PurchaseOrder Create(
        string number,
        Guid supplierId,
        Guid userId,
        string? notes = null)
    {
        if (string.IsNullOrWhiteSpace(number))
        {
            throw new DomainException("El número de orden es requerido");
        }

        return new PurchaseOrder
        {
            Id = Guid.NewGuid(),
            Number = number,
            SupplierId = supplierId,
            UserId = userId,
            Total = Money.Zero(),
            Status = PurchaseOrderStatus.Pending,
            Notes = notes?.Trim()
        };
    }

    public void AddItem(Guid productId, string productName, Quantity quantity, Money unitCost)
    {
        if (Status != PurchaseOrderStatus.Pending)
        {
            throw new DomainException("No se pueden agregar items a una orden que no está pendiente");
        }

        var existingItem = _items.FirstOrDefault(i => i.ProductId == productId);
        if (existingItem is not null)
        {
            throw new DomainException($"El producto {productName} ya está en la orden");
        }

        var item = PurchaseOrderItem.Create(Id, productId, productName, quantity, unitCost);
        _items.Add(item);
        RecalculateTotal();
    }

    public void RemoveItem(Guid productId)
    {
        if (Status != PurchaseOrderStatus.Pending)
        {
            throw new DomainException("No se pueden remover items de una orden que no está pendiente");
        }

        var item = _items.FirstOrDefault(i => i.ProductId == productId);
        if (item is null)
        {
            throw new EntityNotFoundException("Item de orden", productId);
        }

        _items.Remove(item);
        RecalculateTotal();
    }

    public void ReceiveItem(Guid productId, Quantity receivedQuantity)
    {
        if (Status == PurchaseOrderStatus.Cancelled)
        {
            throw new DomainException("No se pueden recibir items de una orden cancelada");
        }

        var item = _items.FirstOrDefault(i => i.ProductId == productId);
        if (item is null)
        {
            throw new EntityNotFoundException("Item de orden", productId);
        }

        item.Receive(receivedQuantity);
        UpdateStatus();
    }

    public void MarkAsReceived()
    {
        if (Status == PurchaseOrderStatus.Cancelled)
        {
            throw new DomainException("No se puede marcar como recibida una orden cancelada");
        }

        foreach (var item in _items.Where(i => !i.IsFullyReceived()))
        {
            item.Receive(item.Quantity.Subtract(item.ReceivedQuantity));
        }

        Status = PurchaseOrderStatus.Received;
        ReceivedAt = DateTime.UtcNow;
        SetUpdated();
    }

    public void Cancel(string? reason = null)
    {
        if (Status == PurchaseOrderStatus.Received)
        {
            throw new DomainException("No se puede cancelar una orden ya recibida");
        }

        Status = PurchaseOrderStatus.Cancelled;
        if (!string.IsNullOrWhiteSpace(reason))
        {
            Notes = $"{Notes}\n[CANCELADA] {DateTime.Now:g}: {reason}".Trim();
        }
        SetUpdated();
    }

    private void RecalculateTotal()
    {
        Total = _items.Aggregate(
            Money.Zero(),
            (acc, item) => acc.Add(item.Total));
        SetUpdated();
    }

    private void UpdateStatus()
    {
        bool allReceived = _items.All(i => i.IsFullyReceived());
        bool someReceived = _items.Any(i => i.ReceivedQuantity > Quantity.Zero());

        if (allReceived)
        {
            Status = PurchaseOrderStatus.Received;
            ReceivedAt = DateTime.UtcNow;
        }
        else if (someReceived)
        {
            Status = PurchaseOrderStatus.PartiallyReceived;
        }

        SetUpdated();
    }
}
