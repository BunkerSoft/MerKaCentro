namespace MerkaCentro.Domain.Exceptions;

public class InsufficientStockException : DomainException
{
    public Guid ProductId { get; }
    public decimal RequestedQuantity { get; }
    public decimal AvailableQuantity { get; }

    public InsufficientStockException() : base()
    {
    }

    public InsufficientStockException(string message) : base(message)
    {
    }

    public InsufficientStockException(string message, Exception innerException) : base(message, innerException)
    {
    }

    public InsufficientStockException(Guid productId, decimal requestedQuantity, decimal availableQuantity)
        : base($"Stock insuficiente. Solicitado: {requestedQuantity}, Disponible: {availableQuantity}")
    {
        ProductId = productId;
        RequestedQuantity = requestedQuantity;
        AvailableQuantity = availableQuantity;
    }
}
