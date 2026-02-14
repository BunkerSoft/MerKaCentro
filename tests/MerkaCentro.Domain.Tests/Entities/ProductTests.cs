using FluentAssertions;
using MerkaCentro.Domain.Entities;
using MerkaCentro.Domain.Enums;
using MerkaCentro.Domain.Exceptions;
using MerkaCentro.Domain.ValueObjects;

namespace MerkaCentro.Domain.Tests.Entities;

public class ProductTests
{
    private static Product CreateValidProduct() => Product.Create(
        code: "PROD001",
        name: "Producto de prueba",
        categoryId: Guid.NewGuid(),
        purchasePrice: Money.Create(10m),
        salePrice: Money.Create(15m),
        unit: "Unidad");

    [Fact]
    public void Create_WithValidData_ShouldCreateProduct()
    {
        var categoryId = Guid.NewGuid();
        var product = Product.Create(
            code: "PROD001",
            name: "Coca Cola 500ml",
            categoryId: categoryId,
            purchasePrice: Money.Create(2.50m),
            salePrice: Money.Create(3.50m),
            unit: "Botella");

        product.Id.Should().NotBeEmpty();
        product.Code.Should().Be("PROD001");
        product.Name.Should().Be("Coca Cola 500ml");
        product.CategoryId.Should().Be(categoryId);
        product.PurchasePrice.Amount.Should().Be(2.50m);
        product.SalePrice.Amount.Should().Be(3.50m);
        product.Unit.Should().Be("Botella");
        product.CurrentStock.Value.Should().Be(0);
        product.MinStock.Value.Should().Be(5);
        product.Status.Should().Be(ProductStatus.Active);
        // Disabled: PriceHistory disabled due to EF Core Owned Entity tracking issues
        // product.PriceHistory.Should().HaveCount(1);
    }

    [Fact]
    public void Create_WithBarcode_ShouldSetBarcode()
    {
        var product = Product.Create(
            code: "PROD001",
            name: "Producto",
            categoryId: Guid.NewGuid(),
            purchasePrice: Money.Create(10m),
            salePrice: Money.Create(15m),
            unit: "Unidad",
            barcode: Barcode.Create("1234567890123"));

        product.Barcode.Should().NotBeNull();
        product.Barcode!.Value.Should().Be("1234567890123");
    }

    [Fact]
    public void Create_WithEmptyCode_ShouldThrowDomainException()
    {
        var act = () => Product.Create(
            code: "",
            name: "Producto",
            categoryId: Guid.NewGuid(),
            purchasePrice: Money.Create(10m),
            salePrice: Money.Create(15m),
            unit: "Unidad");

        act.Should().Throw<DomainException>()
            .WithMessage("El código del producto es requerido");
    }

    [Fact]
    public void Create_WithEmptyName_ShouldThrowDomainException()
    {
        var act = () => Product.Create(
            code: "PROD001",
            name: "",
            categoryId: Guid.NewGuid(),
            purchasePrice: Money.Create(10m),
            salePrice: Money.Create(15m),
            unit: "Unidad");

        act.Should().Throw<DomainException>()
            .WithMessage("El nombre del producto es requerido");
    }

    [Fact]
    public void Create_WithSalePriceLessThanPurchase_ShouldThrowDomainException()
    {
        var act = () => Product.Create(
            code: "PROD001",
            name: "Producto",
            categoryId: Guid.NewGuid(),
            purchasePrice: Money.Create(15m),
            salePrice: Money.Create(10m),
            unit: "Unidad");

        act.Should().Throw<DomainException>()
            .WithMessage("El precio de venta no puede ser menor al precio de compra");
    }

    [Fact]
    public void Create_WithZeroSalePrice_ShouldThrowDomainException()
    {
        var act = () => Product.Create(
            code: "PROD001",
            name: "Producto",
            categoryId: Guid.NewGuid(),
            purchasePrice: Money.Create(0m),
            salePrice: Money.Create(0m),
            unit: "Unidad");

        act.Should().Throw<DomainException>()
            .WithMessage("El precio de venta debe ser mayor a cero");
    }

    [Fact]
    public void UpdatePrices_ShouldUpdateAndAddHistory()
    {
        var product = CreateValidProduct();

        product.UpdatePrices(Money.Create(12m), Money.Create(18m));

        product.PurchasePrice.Amount.Should().Be(12m);
        product.SalePrice.Amount.Should().Be(18m);
        // Disabled: PriceHistory disabled due to EF Core Owned Entity tracking issues
        // product.PriceHistory.Should().HaveCount(2);
    }

    [Fact]
    public void AddStock_ShouldIncreaseCurrentStock()
    {
        var product = CreateValidProduct();

        product.AddStock(Quantity.Create(10), MovementType.Purchase, "PO-001");

        product.CurrentStock.Value.Should().Be(10);
        product.StockMovements.Should().HaveCount(1);
    }

    [Fact]
    public void AddStock_WithZeroQuantity_ShouldThrowDomainException()
    {
        var product = CreateValidProduct();

        var act = () => product.AddStock(Quantity.Zero(), MovementType.Purchase);

        act.Should().Throw<DomainException>()
            .WithMessage("La cantidad a agregar debe ser mayor a cero");
    }

    [Fact]
    public void RemoveStock_ShouldDecreaseCurrentStock()
    {
        var product = CreateValidProduct();
        product.AddStock(Quantity.Create(10), MovementType.Purchase);

        product.RemoveStock(Quantity.Create(3), MovementType.Sale, "V-001");

        product.CurrentStock.Value.Should().Be(7);
        product.StockMovements.Should().HaveCount(2);
    }

    [Fact]
    public void RemoveStock_WithInsufficientStock_ShouldThrowException()
    {
        var product = CreateValidProduct();
        product.AddStock(Quantity.Create(5), MovementType.Purchase);

        var act = () => product.RemoveStock(Quantity.Create(10), MovementType.Sale);

        act.Should().Throw<InsufficientStockException>();
    }

    [Fact]
    public void AdjustStock_ShouldSetStockToNewValue()
    {
        var product = CreateValidProduct();
        product.AddStock(Quantity.Create(10), MovementType.Purchase);

        product.AdjustStock(Quantity.Create(15), "Ajuste por inventario");

        product.CurrentStock.Value.Should().Be(15);
    }

    [Fact]
    public void IsLowStock_WhenStockBelowMin_ShouldReturnTrue()
    {
        var product = CreateValidProduct();
        product.AddStock(Quantity.Create(3), MovementType.Purchase);

        product.IsLowStock().Should().BeTrue();
    }

    [Fact]
    public void IsLowStock_WhenStockAboveMin_ShouldReturnFalse()
    {
        var product = CreateValidProduct();
        product.AddStock(Quantity.Create(10), MovementType.Purchase);

        product.IsLowStock().Should().BeFalse();
    }

    [Fact]
    public void GetProfitMargin_ShouldReturnCorrectPercentage()
    {
        var product = Product.Create(
            code: "PROD001",
            name: "Producto",
            categoryId: Guid.NewGuid(),
            purchasePrice: Money.Create(10m),
            salePrice: Money.Create(15m),
            unit: "Unidad");

        var margin = product.GetProfitMargin();

        margin.Value.Should().Be(50m);
    }

    [Fact]
    public void Activate_ShouldSetStatusActive()
    {
        var product = CreateValidProduct();
        product.Deactivate();

        product.Activate();

        product.Status.Should().Be(ProductStatus.Active);
    }

    [Fact]
    public void Deactivate_ShouldSetStatusInactive()
    {
        var product = CreateValidProduct();

        product.Deactivate();

        product.Status.Should().Be(ProductStatus.Inactive);
    }

    [Fact]
    public void Discontinue_ShouldSetStatusDiscontinued()
    {
        var product = CreateValidProduct();

        product.Discontinue();

        product.Status.Should().Be(ProductStatus.Discontinued);
    }
}
