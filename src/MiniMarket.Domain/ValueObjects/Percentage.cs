using MiniMarket.Domain.Common;
using MiniMarket.Domain.Exceptions;

namespace MiniMarket.Domain.ValueObjects;

public sealed class Percentage : ValueObject
{
    public decimal Value { get; }

    private Percentage(decimal value)
    {
        Value = value;
    }

    public static Percentage Create(decimal value)
    {
        if (value < 0 || value > 100)
        {
            throw new DomainException("El porcentaje debe estar entre 0 y 100");
        }

        return new Percentage(Math.Round(value, 2));
    }

    public static Percentage Zero() => new(0);

    public Money ApplyTo(Money amount)
    {
        return amount.Multiply(Value / 100);
    }

    public decimal AsDecimal() => Value / 100;

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString() => $"{Value:N2}%";
}
