using MiniMarket.Domain.Common;
using MiniMarket.Domain.Exceptions;

namespace MiniMarket.Domain.ValueObjects;

public sealed class Quantity : ValueObject
{
    public decimal Value { get; }

    private Quantity(decimal value)
    {
        Value = value;
    }

    public static Quantity Create(decimal value)
    {
        if (value < 0)
        {
            throw new DomainException("La cantidad no puede ser negativa");
        }

        return new Quantity(Math.Round(value, 3));
    }

    public static Quantity Zero() => new(0);

    public static Quantity One() => new(1);

    internal static Quantity CreateWithSign(decimal value)
    {
        return new Quantity(Math.Round(value, 3));
    }

    public Quantity Negate()
    {
        return new Quantity(-Value);
    }

    public Quantity Add(Quantity other)
    {
        return new Quantity(Value + other.Value);
    }

    public Quantity Subtract(Quantity other)
    {
        decimal result = Value - other.Value;
        if (result < 0)
        {
            throw new DomainException("El resultado no puede ser negativo");
        }
        return new Quantity(result);
    }

    public bool IsZero() => Value == 0;

    public bool IsPositive() => Value > 0;

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString() => Value.ToString("N3");

    public static Quantity operator +(Quantity left, Quantity right) => left.Add(right);
    public static Quantity operator -(Quantity left, Quantity right) => left.Subtract(right);
    public static bool operator >(Quantity left, Quantity right) => left.Value > right.Value;
    public static bool operator <(Quantity left, Quantity right) => left.Value < right.Value;
    public static bool operator >=(Quantity left, Quantity right) => left.Value >= right.Value;
    public static bool operator <=(Quantity left, Quantity right) => left.Value <= right.Value;

    public static implicit operator decimal(Quantity quantity) => quantity.Value;
}
