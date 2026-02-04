using System.Text.RegularExpressions;
using MiniMarket.Domain.Common;
using MiniMarket.Domain.Exceptions;

namespace MiniMarket.Domain.ValueObjects;

public sealed partial class Email : ValueObject
{
    public string Value { get; }

    private Email(string value)
    {
        Value = value;
    }

    public static Email Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new DomainException("El correo electrónico es requerido");
        }

        string cleanValue = value.Trim().ToLowerInvariant();

        if (!EmailRegex().IsMatch(cleanValue))
        {
            throw new DomainException("El correo electrónico no tiene un formato válido");
        }

        return new Email(cleanValue);
    }

    public static Email? CreateOptional(string? value)
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

    [GeneratedRegex(@"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$")]
    private static partial Regex EmailRegex();
}
