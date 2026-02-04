using FluentAssertions;
using MiniMarket.Domain.Exceptions;
using MiniMarket.Domain.ValueObjects;

namespace MiniMarket.Domain.Tests.ValueObjects;

public class PercentageTests
{
    [Theory]
    [InlineData(0)]
    [InlineData(50)]
    [InlineData(100)]
    public void Create_WithValidValue_ShouldCreatePercentage(decimal value)
    {
        var percentage = Percentage.Create(value);

        percentage.Value.Should().Be(value);
    }

    [Fact]
    public void Create_WithNegativeValue_ShouldThrowDomainException()
    {
        var act = () => Percentage.Create(-10);

        act.Should().Throw<DomainException>()
            .WithMessage("El porcentaje debe estar entre 0 y 100");
    }

    [Fact]
    public void Create_WithValueOver100_ShouldThrowDomainException()
    {
        var act = () => Percentage.Create(101);

        act.Should().Throw<DomainException>()
            .WithMessage("El porcentaje debe estar entre 0 y 100");
    }

    [Fact]
    public void Zero_ShouldReturnPercentageWithZeroValue()
    {
        var percentage = Percentage.Zero();

        percentage.Value.Should().Be(0);
    }

    [Fact]
    public void ApplyTo_ShouldReturnCorrectAmount()
    {
        var percentage = Percentage.Create(10);
        var money = Money.Create(100m);

        var result = percentage.ApplyTo(money);

        result.Amount.Should().Be(10m);
    }

    [Fact]
    public void AsDecimal_ShouldReturnDecimalRepresentation()
    {
        var percentage = Percentage.Create(25);

        percentage.AsDecimal().Should().Be(0.25m);
    }

    [Fact]
    public void ToString_ShouldReturnFormattedString()
    {
        var percentage = Percentage.Create(50);

        percentage.ToString().Should().Contain("50");
    }

    [Fact]
    public void Equals_WithSameValues_ShouldReturnTrue()
    {
        var pct1 = Percentage.Create(50);
        var pct2 = Percentage.Create(50);

        pct1.Should().Be(pct2);
    }
}
