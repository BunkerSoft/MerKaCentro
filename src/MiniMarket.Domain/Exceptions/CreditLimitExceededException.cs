namespace MiniMarket.Domain.Exceptions;

public class CreditLimitExceededException : DomainException
{
    public Guid CustomerId { get; }
    public decimal RequestedAmount { get; }
    public decimal AvailableCredit { get; }

    public CreditLimitExceededException(Guid customerId, decimal requestedAmount, decimal availableCredit)
        : base($"Límite de crédito excedido. Solicitado: {requestedAmount:C}, Disponible: {availableCredit:C}")
    {
        CustomerId = customerId;
        RequestedAmount = requestedAmount;
        AvailableCredit = availableCredit;
    }
}
