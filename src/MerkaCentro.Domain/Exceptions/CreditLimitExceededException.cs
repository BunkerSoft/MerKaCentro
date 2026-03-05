namespace MerkaCentro.Domain.Exceptions;

public class CreditLimitExceededException : DomainException
{
    public Guid CustomerId { get; }
    public decimal RequestedAmount { get; }
    public decimal AvailableCredit { get; }

    public CreditLimitExceededException() : base()
    {
    }

    public CreditLimitExceededException(string message) : base(message)
    {
    }

    public CreditLimitExceededException(string message, Exception innerException) : base(message, innerException)
    {
    }

    public CreditLimitExceededException(Guid customerId, decimal requestedAmount, decimal availableCredit)
        : base($"Límite de crédito excedido. Solicitado: {requestedAmount:C}, Disponible: {availableCredit:C}")
    {
        CustomerId = customerId;
        RequestedAmount = requestedAmount;
        AvailableCredit = availableCredit;
    }
}
