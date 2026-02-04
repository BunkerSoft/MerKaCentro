using MiniMarket.Domain.Common;
using MiniMarket.Domain.Exceptions;
using MiniMarket.Domain.ValueObjects;

namespace MiniMarket.Domain.Entities;

public class SaleItem : Entity<Guid>
{
    public Guid SaleId { get; private set; }
    public Guid ProductId { get; private set; }
    public string ProductName { get; private set; } = default!;
    public Quantity Quantity { get; private set; } = default!;
    public Money UnitPrice { get; private set; } = default!;
    public Percentage Discount { get; private set; } = default!;
    public Money Total { get; private set; } = default!;

    private SaleItem() : base()
    {
    }

    internal static SaleItem Create(
        Guid saleId,
        Guid productId,
        string productName,
        Quantity quantity,
        Money unitPrice,
        Percentage? discount = null)
    {
        var item = new SaleItem
        {
            Id = Guid.NewGuid(),
            SaleId = saleId,
            ProductId = productId,
            ProductName = productName,
            Quantity = quantity,
            UnitPrice = unitPrice,
            Discount = discount ?? Percentage.Zero()
        };

        item.CalculateTotal();
        return item;
    }

    internal void UpdateQuantity(Quantity newQuantity)
    {
        if (newQuantity <= Quantity.Zero())
        {
            throw new DomainException("La cantidad debe ser mayor a cero");
        }

        Quantity = newQuantity;
        CalculateTotal();
        SetUpdated();
    }

    internal void ApplyDiscount(Percentage discount)
    {
        Discount = discount;
        CalculateTotal();
        SetUpdated();
    }

    private void CalculateTotal()
    {
        Money subtotal = UnitPrice.Multiply(Quantity);
        Money discountAmount = Discount.ApplyTo(subtotal);
        Total = subtotal.Subtract(discountAmount);
    }
}
