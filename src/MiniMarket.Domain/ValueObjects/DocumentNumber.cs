using System.Text.RegularExpressions;
using MiniMarket.Domain.Common;
using MiniMarket.Domain.Enums;
using MiniMarket.Domain.Exceptions;

namespace MiniMarket.Domain.ValueObjects;

public sealed partial class DocumentNumber : ValueObject
{
    public string Value { get; }
    public DocumentType Type { get; }

    private DocumentNumber(string value, DocumentType type)
    {
        Value = value;
        Type = type;
    }

    public static DocumentNumber Create(string value, DocumentType type)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new DomainException("El número de documento es requerido");
        }

        string cleanValue = value.Trim();

        bool isValid = type switch
        {
            DocumentType.DNI => DniRegex().IsMatch(cleanValue),
            DocumentType.RUC => RucRegex().IsMatch(cleanValue),
            DocumentType.CE => CeRegex().IsMatch(cleanValue),
            _ => false
        };

        if (!isValid)
        {
            throw new DomainException($"El número de documento no es válido para el tipo {type}");
        }

        return new DocumentNumber(cleanValue, type);
    }

    public static DocumentNumber? CreateOptional(string? value, DocumentType? type)
    {
        if (string.IsNullOrWhiteSpace(value) || type is null)
        {
            return null;
        }

        return Create(value, type.Value);
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
        yield return Type;
    }

    public override string ToString() => $"{Type}: {Value}";

    [GeneratedRegex(@"^\d{8}$")]
    private static partial Regex DniRegex();

    [GeneratedRegex(@"^(10|20)\d{9}$")]
    private static partial Regex RucRegex();

    [GeneratedRegex(@"^[A-Za-z0-9]{9,12}$")]
    private static partial Regex CeRegex();
}
