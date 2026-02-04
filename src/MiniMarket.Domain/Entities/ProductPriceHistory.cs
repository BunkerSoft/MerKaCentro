using MiniMarket.Domain.Common;
using MiniMarket.Domain.ValueObjects;

namespace MiniMarket.Domain.Entities;

public class ProductPriceHistory : Entity<Guid>
{
    public Guid ProductId { get; private set; }
    public Money PurchasePrice { get; private set; } = default!;
    public Money SalePrice { get; private set; } = default!;
    public DateTime EffectiveDate { get; private set; }

    private ProductPriceHistory() : base()
    {
    }

    internal static ProductPriceHistory Create(Guid productId, Money purchasePrice, Money salePrice)
    {
        return new ProductPriceHistory
        {
            Id = Guid.NewGuid(),
            ProductId = productId,
            PurchasePrice = purchasePrice,
            SalePrice = salePrice,
            EffectiveDate = DateTime.UtcNow
        };
    }
}
