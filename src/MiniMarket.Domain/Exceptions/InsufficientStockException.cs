namespace MiniMarket.Domain.Exceptions;

public class InsufficientStockException : DomainException
{
    public Guid ProductId { get; }
    public decimal RequestedQuantity { get; }
    public decimal AvailableQuantity { get; }

    public InsufficientStockException(Guid productId, decimal requestedQuantity, decimal availableQuantity)
        : base($"Stock insuficiente. Solicitado: {requestedQuantity}, Disponible: {availableQuantity}")
    {
        ProductId = productId;
        RequestedQuantity = requestedQuantity;
        AvailableQuantity = availableQuantity;
    }
}
