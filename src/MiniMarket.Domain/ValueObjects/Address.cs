using MiniMarket.Domain.Common;

namespace MiniMarket.Domain.ValueObjects;

public sealed class Address : ValueObject
{
    public string Street { get; }
    public string? District { get; }
    public string? City { get; }
    public string? Reference { get; }

    private Address(string street, string? district, string? city, string? reference)
    {
        Street = street;
        District = district;
        City = city;
        Reference = reference;
    }

    public static Address Create(string street, string? district = null, string? city = null, string? reference = null)
    {
        if (string.IsNullOrWhiteSpace(street))
        {
            street = string.Empty;
        }

        return new Address(street.Trim(), district?.Trim(), city?.Trim(), reference?.Trim());
    }

    public static Address Empty() => new(string.Empty, null, null, null);

    public bool IsEmpty() => string.IsNullOrWhiteSpace(Street);

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Street;
        yield return District;
        yield return City;
        yield return Reference;
    }

    public override string ToString()
    {
        var parts = new List<string>();

        if (!string.IsNullOrWhiteSpace(Street))
        {
            parts.Add(Street);
        }

        if (!string.IsNullOrWhiteSpace(District))
        {
            parts.Add(District);
        }

        if (!string.IsNullOrWhiteSpace(City))
        {
            parts.Add(City);
        }

        return string.Join(", ", parts);
    }
}
