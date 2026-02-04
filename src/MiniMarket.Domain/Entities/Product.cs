using MiniMarket.Domain.Common;
using MiniMarket.Domain.Enums;
using MiniMarket.Domain.Exceptions;
using MiniMarket.Domain.ValueObjects;

namespace MiniMarket.Domain.Entities;

public class Product : AggregateRoot<Guid>
{
    public string Code { get; private set; } = default!;
    public Barcode? Barcode { get; private set; }
    public string Name { get; private set; } = default!;
    public string? Description { get; private set; }
    public Guid CategoryId { get; private set; }
    public Category? Category { get; private set; }
    public Money PurchasePrice { get; private set; } = default!;
    public Money SalePrice { get; private set; } = default!;
    public Quantity MinStock { get; private set; } = default!;
    public Quantity CurrentStock { get; private set; } = default!;
    public string Unit { get; private set; } = default!;
    public bool AllowFractions { get; private set; }
    public ProductStatus Status { get; private set; }

    private readonly List<ProductPriceHistory> _priceHistory = [];
    public IReadOnlyCollection<ProductPriceHistory> PriceHistory => _priceHistory.AsReadOnly();

    private readonly List<StockMovement> _stockMovements = [];
    public IReadOnlyCollection<StockMovement> StockMovements => _stockMovements.AsReadOnly();

    private Product() : base()
    {
    }

    public static Product Create(
        string code,
        string name,
        Guid categoryId,
        Money purchasePrice,
        Money salePrice,
        string unit,
        Barcode? barcode = null,
        string? description = null,
        Quantity? minStock = null,
        bool allowFractions = false)
    {
        ValidateBasicFields(code, name);
        ValidatePrices(purchasePrice, salePrice);

        var product = new Product
        {
            Id = Guid.NewGuid(),
            Code = code.Trim().ToUpperInvariant(),
            Barcode = barcode,
            Name = name.Trim(),
            Description = description?.Trim(),
            CategoryId = categoryId,
            PurchasePrice = purchasePrice,
            SalePrice = salePrice,
            MinStock = minStock ?? Quantity.Create(5),
            CurrentStock = Quantity.Zero(),
            Unit = unit.Trim(),
            AllowFractions = allowFractions,
            Status = ProductStatus.Active
        };

        product.AddPriceHistory(purchasePrice, salePrice);

        return product;
    }

    public void Update(
        string name,
        Guid categoryId,
        string unit,
        Barcode? barcode,
        string? description,
        Quantity minStock,
        bool allowFractions)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new DomainException("El nombre del producto es requerido");
        }

        Name = name.Trim();
        CategoryId = categoryId;
        Unit = unit.Trim();
        Barcode = barcode;
        Description = description?.Trim();
        MinStock = minStock;
        AllowFractions = allowFractions;
        SetUpdated();
    }

    public void UpdatePrices(Money purchasePrice, Money salePrice)
    {
        ValidatePrices(purchasePrice, salePrice);

        bool pricesChanged = PurchasePrice != purchasePrice || SalePrice != salePrice;

        PurchasePrice = purchasePrice;
        SalePrice = salePrice;

        if (pricesChanged)
        {
            AddPriceHistory(purchasePrice, salePrice);
        }

        SetUpdated();
    }

    public void AddStock(Quantity quantity, MovementType type, string? reference = null, string? notes = null)
    {
        if (quantity <= Quantity.Zero())
        {
            throw new DomainException("La cantidad a agregar debe ser mayor a cero");
        }

        CurrentStock = CurrentStock.Add(quantity);

        var movement = StockMovement.Create(
            Id,
            type,
            quantity,
            CurrentStock,
            reference,
            notes);

        _stockMovements.Add(movement);
        SetUpdated();
    }

    public void RemoveStock(Quantity quantity, MovementType type, string? reference = null, string? notes = null)
    {
        if (quantity <= Quantity.Zero())
        {
            throw new DomainException("La cantidad a remover debe ser mayor a cero");
        }

        if (CurrentStock < quantity)
        {
            throw new InsufficientStockException(Id, quantity.Value, CurrentStock.Value);
        }

        CurrentStock = CurrentStock.Subtract(quantity);

        var movement = StockMovement.Create(
            Id,
            type,
            quantity.Negate(),
            CurrentStock,
            reference,
            notes);

        _stockMovements.Add(movement);
        SetUpdated();
    }

    public void AdjustStock(Quantity newStock, string? notes = null)
    {
        decimal difference = newStock.Value - CurrentStock.Value;

        CurrentStock = newStock;

        var movement = StockMovement.Create(
            Id,
            MovementType.Adjustment,
            Quantity.Create(Math.Abs(difference)),
            CurrentStock,
            null,
            notes ?? $"Ajuste de inventario. Diferencia: {difference:+#;-#;0}");

        _stockMovements.Add(movement);
        SetUpdated();
    }

    public bool IsLowStock() => CurrentStock <= MinStock;

    public void Activate()
    {
        Status = ProductStatus.Active;
        SetUpdated();
    }

    public void Deactivate()
    {
        Status = ProductStatus.Inactive;
        SetUpdated();
    }

    public void Discontinue()
    {
        Status = ProductStatus.Discontinued;
        SetUpdated();
    }

    public Percentage GetProfitMargin()
    {
        if (PurchasePrice.Amount == 0)
        {
            return Percentage.Zero();
        }

        decimal margin = ((SalePrice.Amount - PurchasePrice.Amount) / PurchasePrice.Amount) * 100;
        return Percentage.Create(Math.Max(0, Math.Min(100, margin)));
    }

    private void AddPriceHistory(Money purchasePrice, Money salePrice)
    {
        var history = ProductPriceHistory.Create(Id, purchasePrice, salePrice);
        _priceHistory.Add(history);
    }

    private static void ValidateBasicFields(string code, string name)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            throw new DomainException("El código del producto es requerido");
        }

        if (string.IsNullOrWhiteSpace(name))
        {
            throw new DomainException("El nombre del producto es requerido");
        }
    }

    private static void ValidatePrices(Money purchasePrice, Money salePrice)
    {
        if (purchasePrice.Amount < 0)
        {
            throw new DomainException("El precio de compra no puede ser negativo");
        }

        if (salePrice.Amount <= 0)
        {
            throw new DomainException("El precio de venta debe ser mayor a cero");
        }

        if (salePrice < purchasePrice)
        {
            throw new DomainException("El precio de venta no puede ser menor al precio de compra");
        }
    }
}
