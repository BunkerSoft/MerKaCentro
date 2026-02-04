using System.Text.RegularExpressions;
using MiniMarket.Domain.Common;
using MiniMarket.Domain.Exceptions;

namespace MiniMarket.Domain.ValueObjects;

public sealed partial class PhoneNumber : ValueObject
{
    public string Value { get; }

    private PhoneNumber(string value)
    {
        Value = value;
    }

    public static PhoneNumber Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new DomainException("El número de teléfono es requerido");
        }

        string cleanValue = PhoneCleanRegex().Replace(value, "");

        if (cleanValue.Length < 7 || cleanValue.Length > 15)
        {
            throw new DomainException("El número de teléfono debe tener entre 7 y 15 dígitos");
        }

        return new PhoneNumber(cleanValue);
    }

    public static PhoneNumber? CreateOptional(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        return Create(value);
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString() => Value;

    [GeneratedRegex(@"[^\d]")]
    private static partial Regex PhoneCleanRegex();
}
