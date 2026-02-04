using FluentAssertions;
using MiniMarket.Domain.Entities;
using MiniMarket.Domain.Enums;
using MiniMarket.Domain.Exceptions;
using MiniMarket.Domain.ValueObjects;

namespace MiniMarket.Domain.Tests.Entities;

public class SaleTests
{
    private static Sale CreateValidSale() => Sale.Create(
        number: "V-001",
        cashRegisterId: Guid.NewGuid(),
        userId: Guid.NewGuid());

    private static Product CreateValidProduct() => Product.Create(
        code: "PROD001",
        name: "Producto de prueba",
        categoryId: Guid.NewGuid(),
        purchasePrice: Money.Create(10m),
        salePrice: Money.Create(15m),
        unit: "Unidad",
        allowFractions: true);

    [Fact]
    public void Create_WithValidData_ShouldCreateSale()
    {
        var cashRegisterId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var sale = Sale.Create("V-001", cashRegisterId, userId);

        sale.Id.Should().NotBeEmpty();
        sale.Number.Should().Be("V-001");
        sale.CashRegisterId.Should().Be(cashRegisterId);
        sale.UserId.Should().Be(userId);
        sale.Status.Should().Be(SaleStatus.Pending);
        sale.Total.Amount.Should().Be(0);
    }

    [Fact]
    public void Create_WithEmptyNumber_ShouldThrowDomainException()
    {
        var act = () => Sale.Create("", Guid.NewGuid(), Guid.NewGuid());

        act.Should().Throw<DomainException>()
            .WithMessage("El número de venta es requerido");
    }

    [Fact]
    public void AddItem_ShouldAddItemAndRecalculateTotal()
    {
        var sale = CreateValidSale();
        var product = CreateValidProduct();

        sale.AddItem(product, Quantity.Create(2));

        sale.Items.Should().HaveCount(1);
        sale.Subtotal.Amount.Should().Be(30m);
        sale.Total.Amount.Should().Be(30m);
    }

    [Fact]
    public void AddItem_SameProduct_ShouldIncreaseQuantity()
    {
        var sale = CreateValidSale();
        var product = CreateValidProduct();

        sale.AddItem(product, Quantity.Create(2));
        sale.AddItem(product, Quantity.Create(3));

        sale.Items.Should().HaveCount(1);
        sale.Items.First().Quantity.Value.Should().Be(5);
        sale.Subtotal.Amount.Should().Be(75m);
    }

    [Fact]
    public void AddItem_WithZeroQuantity_ShouldThrowDomainException()
    {
        var sale = CreateValidSale();
        var product = CreateValidProduct();

        var act = () => sale.AddItem(product, Quantity.Zero());

        act.Should().Throw<DomainException>()
            .WithMessage("La cantidad debe ser mayor a cero");
    }

    [Fact]
    public void AddItem_FractionsNotAllowed_ShouldThrowDomainException()
    {
        var sale = CreateValidSale();
        var product = Product.Create(
            code: "PROD001",
            name: "Producto",
            categoryId: Guid.NewGuid(),
            purchasePrice: Money.Create(10m),
            salePrice: Money.Create(15m),
            unit: "Unidad",
            allowFractions: false);

        var act = () => sale.AddItem(product, Quantity.Create(1.5m));

        act.Should().Throw<DomainException>()
            .WithMessage($"El producto {product.Name} no permite fracciones");
    }

    [Fact]
    public void AddItem_ToCompletedSale_ShouldThrowDomainException()
    {
        var sale = CreateValidSale();
        var product = CreateValidProduct();
        sale.AddItem(product, Quantity.Create(1));
        sale.AddPayment(PaymentMethod.Cash, Money.Create(15m));
        sale.Complete();

        var act = () => sale.AddItem(product, Quantity.Create(1));

        act.Should().Throw<DomainException>()
            .WithMessage("No se pueden agregar items a una venta que no está pendiente");
    }

    [Fact]
    public void RemoveItem_ShouldRemoveAndRecalculateTotal()
    {
        var sale = CreateValidSale();
        var product = CreateValidProduct();
        sale.AddItem(product, Quantity.Create(2));

        sale.RemoveItem(product.Id);

        sale.Items.Should().BeEmpty();
        sale.Total.Amount.Should().Be(0);
    }

    [Fact]
    public void RemoveItem_NotFound_ShouldThrowEntityNotFoundException()
    {
        var sale = CreateValidSale();

        var act = () => sale.RemoveItem(Guid.NewGuid());

        act.Should().Throw<EntityNotFoundException>();
    }

    [Fact]
    public void UpdateItemQuantity_ShouldUpdateAndRecalculate()
    {
        var sale = CreateValidSale();
        var product = CreateValidProduct();
        sale.AddItem(product, Quantity.Create(2));

        sale.UpdateItemQuantity(product.Id, Quantity.Create(5));

        sale.Items.First().Quantity.Value.Should().Be(5);
        sale.Total.Amount.Should().Be(75m);
    }

    [Fact]
    public void UpdateItemQuantity_ToZero_ShouldRemoveItem()
    {
        var sale = CreateValidSale();
        var product = CreateValidProduct();
        sale.AddItem(product, Quantity.Create(2));

        sale.UpdateItemQuantity(product.Id, Quantity.Zero());

        sale.Items.Should().BeEmpty();
    }

    [Fact]
    public void ApplyDiscount_ShouldReduceTotal()
    {
        var sale = CreateValidSale();
        var product = CreateValidProduct();
        sale.AddItem(product, Quantity.Create(2));

        sale.ApplyDiscount(Money.Create(5m));

        sale.Discount.Amount.Should().Be(5m);
        sale.Total.Amount.Should().Be(25m);
    }

    [Fact]
    public void ApplyDiscount_GreaterThanSubtotal_ShouldThrowDomainException()
    {
        var sale = CreateValidSale();
        var product = CreateValidProduct();
        sale.AddItem(product, Quantity.Create(1));

        var act = () => sale.ApplyDiscount(Money.Create(20m));

        act.Should().Throw<DomainException>()
            .WithMessage("El descuento no puede ser mayor al subtotal");
    }

    [Fact]
    public void AddPayment_ShouldUpdatePaymentInfo()
    {
        var sale = CreateValidSale();
        var product = CreateValidProduct();
        sale.AddItem(product, Quantity.Create(1));

        sale.AddPayment(PaymentMethod.Cash, Money.Create(20m));

        sale.Payments.Should().HaveCount(1);
        sale.AmountPaid.Amount.Should().Be(20m);
        sale.Change.Amount.Should().Be(5m);
        sale.PaymentMethod.Should().Be(PaymentMethod.Cash);
    }

    [Fact]
    public void AddPayment_MultiplePayments_ShouldSetMethodToMixed()
    {
        var sale = CreateValidSale();
        var product = CreateValidProduct();
        sale.AddItem(product, Quantity.Create(1));

        sale.AddPayment(PaymentMethod.Cash, Money.Create(10m));
        sale.AddPayment(PaymentMethod.Card, Money.Create(5m));

        sale.PaymentMethod.Should().Be(PaymentMethod.Mixed);
    }

    [Fact]
    public void Complete_WithSufficientPayment_ShouldSetStatusCompleted()
    {
        var sale = CreateValidSale();
        var product = CreateValidProduct();
        sale.AddItem(product, Quantity.Create(1));
        sale.AddPayment(PaymentMethod.Cash, Money.Create(15m));

        sale.Complete();

        sale.Status.Should().Be(SaleStatus.Completed);
    }

    [Fact]
    public void Complete_WithoutItems_ShouldThrowDomainException()
    {
        var sale = CreateValidSale();
        sale.AddPayment(PaymentMethod.Cash, Money.Create(15m));

        var act = () => sale.Complete();

        act.Should().Throw<DomainException>()
            .WithMessage("No se puede completar una venta sin items");
    }

    [Fact]
    public void Complete_WithInsufficientPayment_ShouldThrowDomainException()
    {
        var sale = CreateValidSale();
        var product = CreateValidProduct();
        sale.AddItem(product, Quantity.Create(1));
        sale.AddPayment(PaymentMethod.Cash, Money.Create(10m));

        var act = () => sale.Complete();

        act.Should().Throw<DomainException>()
            .WithMessage("El monto pagado es insuficiente");
    }

    [Fact]
    public void Complete_CreditSaleWithoutPayment_ShouldComplete()
    {
        var sale = Sale.Create("V-001", Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), true);
        var product = CreateValidProduct();
        sale.AddItem(product, Quantity.Create(1));

        sale.Complete();

        sale.Status.Should().Be(SaleStatus.Completed);
    }

    [Fact]
    public void Cancel_ShouldSetStatusCancelled()
    {
        var sale = CreateValidSale();
        var product = CreateValidProduct();
        sale.AddItem(product, Quantity.Create(1));

        sale.Cancel("Cancelado por cliente");

        sale.Status.Should().Be(SaleStatus.Cancelled);
        sale.Notes.Should().Contain("CANCELADA");
    }

    [Fact]
    public void Cancel_AlreadyCancelled_ShouldThrowDomainException()
    {
        var sale = CreateValidSale();
        sale.Cancel();

        var act = () => sale.Cancel();

        act.Should().Throw<DomainException>()
            .WithMessage("La venta ya está cancelada");
    }
}
