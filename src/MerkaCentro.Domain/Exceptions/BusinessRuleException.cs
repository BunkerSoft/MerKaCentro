namespace MerkaCentro.Domain.Exceptions;

public class BusinessRuleException : DomainException
{
    public string RuleName { get; }

    public BusinessRuleException() : base()
    {
        RuleName = string.Empty;
    }

    public BusinessRuleException(string message) : base(message)
    {
        RuleName = string.Empty;
    }

    public BusinessRuleException(string message, Exception innerException) : base(message, innerException)
    {
        RuleName = string.Empty;
    }

    public BusinessRuleException(string ruleName, string message)
        : base(message)
    {
        RuleName = ruleName;
    }
}
