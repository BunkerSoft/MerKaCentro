using System.Text.RegularExpressions;
using MerkaCentro.Domain.Common;
using MerkaCentro.Domain.Exceptions;

namespace MerkaCentro.Domain.ValueObjects;

public sealed partial class Barcode : ValueObject
{
    public string Value { get; }

    private Barcode() { }

    private Barcode(string value)
    {
        Value = value;
    }

    public static Barcode Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new DomainException("El código de barras es requerido");
        }

        string cleanValue = value.Trim();

        if (!BarcodeRegex().IsMatch(cleanValue))
        {
            throw new DomainException("El código de barras debe contener solo números y tener entre 8 y 14 dígitos");
        }

        return new Barcode(cleanValue);
    }

    public static Barcode? CreateOptional(string? value)
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

    [GeneratedRegex(@"^\d{8,14}$")]
    private static partial Regex BarcodeRegex();
}
