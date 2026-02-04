using MiniMarket.Domain.Common;
using MiniMarket.Domain.Enums;
using MiniMarket.Domain.Exceptions;
using MiniMarket.Domain.ValueObjects;

namespace MiniMarket.Domain.Entities;

public class Sale : AggregateRoot<Guid>
{
    public string Number { get; private set; } = default!;
    public Guid? CustomerId { get; private set; }
    public Customer? Customer { get; private set; }
    public Guid CashRegisterId { get; private set; }
    public CashRegister? CashRegister { get; private set; }
    public Guid UserId { get; private set; }
    public User? User { get; private set; }
    public Money Subtotal { get; private set; } = default!;
    public Money Discount { get; private set; } = default!;
    public Money Tax { get; private set; } = default!;
    public Money Total { get; private set; } = default!;
    public PaymentMethod PaymentMethod { get; private set; }
    public Money AmountPaid { get; private set; } = default!;
    public Money Change { get; private set; } = default!;
    public SaleStatus Status { get; private set; }
    public bool IsCredit { get; private set; }
    public string? Notes { get; private set; }

    private readonly List<SaleItem> _items = [];
    public IReadOnlyCollection<SaleItem> Items => _items.AsReadOnly();

    private readonly List<SalePayment> _payments = [];
    public IReadOnlyCollection<SalePayment> Payments => _payments.AsReadOnly();

    private Sale() : base()
    {
    }

    public static Sale Create(
        string number,
        Guid cashRegisterId,
        Guid userId,
        Guid? customerId = null,
        bool isCredit = false,
        string? notes = null)
    {
        if (string.IsNullOrWhiteSpace(number))
        {
            throw new DomainException("El número de venta es requerido");
        }

        return new Sale
        {
            Id = Guid.NewGuid(),
            Number = number,
            CustomerId = customerId,
            CashRegisterId = cashRegisterId,
            UserId = userId,
            Subtotal = Money.Zero(),
            Discount = Money.Zero(),
            Tax = Money.Zero(),
            Total = Money.Zero(),
            PaymentMethod = PaymentMethod.Cash,
            AmountPaid = Money.Zero(),
            Change = Money.Zero(),
            Status = SaleStatus.Pending,
            IsCredit = isCredit,
            Notes = notes?.Trim()
        };
    }

    public void AddItem(Product product, Quantity quantity, Money? unitPrice = null, Percentage? discount = null)
    {
        if (Status != SaleStatus.Pending)
        {
            throw new DomainException("No se pueden agregar items a una venta que no está pendiente");
        }

        if (quantity <= Quantity.Zero())
        {
            throw new DomainException("La cantidad debe ser mayor a cero");
        }

        if (!product.AllowFractions && quantity.Value != Math.Floor(quantity.Value))
        {
            throw new DomainException($"El producto {product.Name} no permite fracciones");
        }

        var existingItem = _items.FirstOrDefault(i => i.ProductId == product.Id);
        if (existingItem is not null)
        {
            existingItem.UpdateQuantity(existingItem.Quantity.Add(quantity));
        }
        else
        {
            var item = SaleItem.Create(
                Id,
                product.Id,
                product.Name,
                quantity,
                unitPrice ?? product.SalePrice,
                discount);
            _items.Add(item);
        }

        RecalculateTotals();
    }

    public void RemoveItem(Guid productId)
    {
        if (Status != SaleStatus.Pending)
        {
            throw new DomainException("No se pueden remover items de una venta que no está pendiente");
        }

        var item = _items.FirstOrDefault(i => i.ProductId == productId);
        if (item is null)
        {
            throw new EntityNotFoundException("Item de venta", productId);
        }

        _items.Remove(item);
        RecalculateTotals();
    }

    public void UpdateItemQuantity(Guid productId, Quantity newQuantity)
    {
        if (Status != SaleStatus.Pending)
        {
            throw new DomainException("No se pueden modificar items de una venta que no está pendiente");
        }

        var item = _items.FirstOrDefault(i => i.ProductId == productId);
        if (item is null)
        {
            throw new EntityNotFoundException("Item de venta", productId);
        }

        if (newQuantity <= Quantity.Zero())
        {
            _items.Remove(item);
        }
        else
        {
            item.UpdateQuantity(newQuantity);
        }

        RecalculateTotals();
    }

    public void ApplyDiscount(Money discount)
    {
        if (Status != SaleStatus.Pending)
        {
            throw new DomainException("No se puede aplicar descuento a una venta que no está pendiente");
        }

        if (discount.Amount < 0)
        {
            throw new DomainException("El descuento no puede ser negativo");
        }

        if (discount > Subtotal)
        {
            throw new DomainException("El descuento no puede ser mayor al subtotal");
        }

        Discount = discount;
        RecalculateTotals();
    }

    public void AddPayment(PaymentMethod method, Money amount, string? reference = null)
    {
        if (Status != SaleStatus.Pending)
        {
            throw new DomainException("No se pueden agregar pagos a una venta que no está pendiente");
        }

        var payment = SalePayment.Create(Id, method, amount, reference);
        _payments.Add(payment);

        UpdatePaymentInfo();
    }

    public void Complete()
    {
        if (Status != SaleStatus.Pending)
        {
            throw new DomainException("Solo se pueden completar ventas pendientes");
        }

        if (!_items.Any())
        {
            throw new DomainException("No se puede completar una venta sin items");
        }

        if (!IsCredit && AmountPaid < Total)
        {
            throw new DomainException("El monto pagado es insuficiente");
        }

        Status = SaleStatus.Completed;
        SetUpdated();
    }

    public void Cancel(string? reason = null)
    {
        if (Status == SaleStatus.Cancelled)
        {
            throw new DomainException("La venta ya está cancelada");
        }

        Status = SaleStatus.Cancelled;
        if (!string.IsNullOrWhiteSpace(reason))
        {
            Notes = $"{Notes}\n[CANCELADA] {DateTime.Now:g}: {reason}".Trim();
        }
        SetUpdated();
    }

    private void RecalculateTotals()
    {
        Subtotal = _items.Aggregate(
            Money.Zero(),
            (acc, item) => acc.Add(item.Total));

        Total = Subtotal.Subtract(Discount).Add(Tax);
        SetUpdated();
    }

    private void UpdatePaymentInfo()
    {
        AmountPaid = _payments.Aggregate(
            Money.Zero(),
            (acc, payment) => acc.Add(payment.Amount));

        if (AmountPaid > Total)
        {
            Change = AmountPaid.Subtract(Total);
        }
        else
        {
            Change = Money.Zero();
        }

        PaymentMethod = _payments.Count == 1
            ? _payments.First().Method
            : PaymentMethod.Mixed;

        SetUpdated();
    }
}
