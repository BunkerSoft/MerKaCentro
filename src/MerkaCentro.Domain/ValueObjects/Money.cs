using MerkaCentro.Domain.Common;
using MerkaCentro.Domain.Exceptions;

namespace MerkaCentro.Domain.ValueObjects;

public sealed class Money : ValueObject
{
    public decimal Amount { get; }
    public string Currency { get; }

    // EF Core necesita un constructor parameterless
    #pragma warning disable CS8618
    protected Money()
    {
    }
    #pragma warning restore CS8618

    private Money(decimal amount, string currency)
    {
        Amount = amount;
        Currency = currency;
    }

    public static Money Create(decimal amount, string currency = "PEN")
    {
        if (amount < 0)
        {
            throw new DomainException("El monto no puede ser negativo");
        }

        if (string.IsNullOrWhiteSpace(currency))
        {
            throw new DomainException("La moneda es requerida");
        }

        return new Money(Math.Round(amount, 2), currency.ToUpperInvariant());
    }

    public static Money Zero(string currency = "PEN") => new(0, currency);

    internal static Money CreateWithSign(decimal amount, string currency = "PEN")
    {
        if (string.IsNullOrWhiteSpace(currency))
        {
            throw new DomainException("La moneda es requerida");
        }

        return new Money(Math.Round(amount, 2), currency.ToUpperInvariant());
    }

    public Money Negate()
    {
        return new Money(-Amount, Currency);
    }

    public Money Add(Money other)
    {
        ValidateSameCurrency(other);
        return new Money(Amount + other.Amount, Currency);
    }

    public Money Subtract(Money other)
    {
        ValidateSameCurrency(other);
        decimal result = Amount - other.Amount;
        if (result < 0)
        {
            throw new DomainException("El resultado no puede ser negativo");
        }
        return new Money(result, Currency);
    }

    public Money Multiply(decimal factor)
    {
        if (factor < 0)
        {
            throw new DomainException("El factor no puede ser negativo");
        }
        return new Money(Math.Round(Amount * factor, 2), Currency);
    }

    public Money Multiply(Quantity quantity)
    {
        return Multiply(quantity.Value);
    }

    private void ValidateSameCurrency(Money other)
    {
        if (Currency != other.Currency)
        {
            throw new DomainException($"No se pueden operar monedas diferentes: {Currency} y {other.Currency}");
        }
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Amount;
        yield return Currency;
    }

    public override string ToString() => $"{Currency} {Amount:N2}";

    public static Money operator +(Money left, Money right) => left.Add(right);
    public static Money operator -(Money left, Money right) => left.Subtract(right);
    public static Money operator *(Money money, decimal factor) => money.Multiply(factor);
    public static Money operator *(Money money, Quantity quantity) => money.Multiply(quantity);
    public static bool operator >(Money left, Money right)
    {
        left.ValidateSameCurrency(right);
        return left.Amount > right.Amount;
    }
    public static bool operator <(Money left, Money right)
    {
        left.ValidateSameCurrency(right);
        return left.Amount < right.Amount;
    }
    public static bool operator >=(Money left, Money right)
    {
        left.ValidateSameCurrency(right);
        return left.Amount >= right.Amount;
    }
    public static bool operator <=(Money left, Money right)
    {
        left.ValidateSameCurrency(right);
        return left.Amount <= right.Amount;
    }
}
