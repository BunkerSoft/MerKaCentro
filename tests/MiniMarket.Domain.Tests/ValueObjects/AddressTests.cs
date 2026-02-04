using FluentAssertions;
using MiniMarket.Domain.ValueObjects;

namespace MiniMarket.Domain.Tests.ValueObjects;

public class AddressTests
{
    [Fact]
    public void Create_WithAllValues_ShouldCreateAddress()
    {
        var address = Address.Create("Jr. Lima 123", "Miraflores", "Lima", "Cerca al parque");

        address.Street.Should().Be("Jr. Lima 123");
        address.District.Should().Be("Miraflores");
        address.City.Should().Be("Lima");
        address.Reference.Should().Be("Cerca al parque");
    }

    [Fact]
    public void Create_WithOnlyStreet_ShouldCreateAddress()
    {
        var address = Address.Create("Jr. Lima 123");

        address.Street.Should().Be("Jr. Lima 123");
        address.District.Should().BeNull();
        address.City.Should().BeNull();
        address.Reference.Should().BeNull();
    }

    [Fact]
    public void Create_WithEmptyStreet_ShouldCreateAddressWithEmptyStreet()
    {
        var address = Address.Create("");

        address.Street.Should().BeEmpty();
    }

    [Fact]
    public void Empty_ShouldReturnAddressWithEmptyStreet()
    {
        var address = Address.Empty();

        address.Street.Should().BeEmpty();
        address.IsEmpty().Should().BeTrue();
    }

    [Fact]
    public void IsEmpty_WhenStreetIsEmpty_ShouldReturnTrue()
    {
        var address = Address.Create("");

        address.IsEmpty().Should().BeTrue();
    }

    [Fact]
    public void IsEmpty_WhenStreetHasValue_ShouldReturnFalse()
    {
        var address = Address.Create("Jr. Lima 123");

        address.IsEmpty().Should().BeFalse();
    }

    [Fact]
    public void ToString_ShouldReturnFormattedString()
    {
        var address = Address.Create("Jr. Lima 123", "Miraflores", "Lima");

        var result = address.ToString();

        result.Should().Contain("Jr. Lima 123");
        result.Should().Contain("Miraflores");
        result.Should().Contain("Lima");
    }

    [Fact]
    public void ToString_WithOnlyStreet_ShouldReturnOnlyStreet()
    {
        var address = Address.Create("Jr. Lima 123");

        address.ToString().Should().Be("Jr. Lima 123");
    }

    [Fact]
    public void Equals_WithSameValues_ShouldReturnTrue()
    {
        var address1 = Address.Create("Jr. Lima 123", "Miraflores", "Lima");
        var address2 = Address.Create("Jr. Lima 123", "Miraflores", "Lima");

        address1.Should().Be(address2);
    }

    [Fact]
    public void Equals_WithDifferentValues_ShouldReturnFalse()
    {
        var address1 = Address.Create("Jr. Lima 123", "Miraflores", "Lima");
        var address2 = Address.Create("Av. Larco 456", "Miraflores", "Lima");

        address1.Should().NotBe(address2);
    }
}
