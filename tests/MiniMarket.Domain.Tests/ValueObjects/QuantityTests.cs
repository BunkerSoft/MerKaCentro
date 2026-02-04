using FluentAssertions;
using MiniMarket.Domain.Exceptions;
using MiniMarket.Domain.ValueObjects;

namespace MiniMarket.Domain.Tests.ValueObjects;

public class QuantityTests
{
    [Fact]
    public void Create_WithValidValue_ShouldCreateQuantity()
    {
        var quantity = Quantity.Create(10.5m);

        quantity.Value.Should().Be(10.5m);
    }

    [Fact]
    public void Create_WithNegativeValue_ShouldThrowDomainException()
    {
        var act = () => Quantity.Create(-5m);

        act.Should().Throw<DomainException>()
            .WithMessage("La cantidad no puede ser negativa");
    }

    [Fact]
    public void Create_ShouldRoundToThreeDecimals()
    {
        var quantity = Quantity.Create(10.5555m);

        quantity.Value.Should().Be(10.556m);
    }

    [Fact]
    public void Zero_ShouldReturnQuantityWithZeroValue()
    {
        var quantity = Quantity.Zero();

        quantity.Value.Should().Be(0);
    }

    [Fact]
    public void One_ShouldReturnQuantityWithOneValue()
    {
        var quantity = Quantity.One();

        quantity.Value.Should().Be(1);
    }

    [Fact]
    public void Add_ShouldReturnSum()
    {
        var qty1 = Quantity.Create(10);
        var qty2 = Quantity.Create(5);

        var result = qty1.Add(qty2);

        result.Value.Should().Be(15);
    }

    [Fact]
    public void Subtract_ShouldReturnDifference()
    {
        var qty1 = Quantity.Create(10);
        var qty2 = Quantity.Create(3);

        var result = qty1.Subtract(qty2);

        result.Value.Should().Be(7);
    }

    [Fact]
    public void Subtract_ResultingNegative_ShouldThrowDomainException()
    {
        var qty1 = Quantity.Create(3);
        var qty2 = Quantity.Create(10);

        var act = () => qty1.Subtract(qty2);

        act.Should().Throw<DomainException>()
            .WithMessage("El resultado no puede ser negativo");
    }

    [Fact]
    public void IsZero_WhenZero_ShouldReturnTrue()
    {
        var quantity = Quantity.Zero();

        quantity.IsZero().Should().BeTrue();
    }

    [Fact]
    public void IsZero_WhenNotZero_ShouldReturnFalse()
    {
        var quantity = Quantity.Create(5);

        quantity.IsZero().Should().BeFalse();
    }

    [Fact]
    public void IsPositive_WhenPositive_ShouldReturnTrue()
    {
        var quantity = Quantity.Create(5);

        quantity.IsPositive().Should().BeTrue();
    }

    [Fact]
    public void IsPositive_WhenZero_ShouldReturnFalse()
    {
        var quantity = Quantity.Zero();

        quantity.IsPositive().Should().BeFalse();
    }

    [Fact]
    public void Operators_ShouldWorkCorrectly()
    {
        var qty1 = Quantity.Create(10);
        var qty2 = Quantity.Create(5);

        (qty1 + qty2).Value.Should().Be(15);
        (qty1 - qty2).Value.Should().Be(5);
        (qty1 > qty2).Should().BeTrue();
        (qty2 < qty1).Should().BeTrue();
        (qty1 >= qty2).Should().BeTrue();
        (qty2 <= qty1).Should().BeTrue();
    }

    [Fact]
    public void ImplicitConversionToDecimal_ShouldReturnValue()
    {
        var quantity = Quantity.Create(10.5m);

        decimal value = quantity;

        value.Should().Be(10.5m);
    }

    [Fact]
    public void Equals_WithSameValues_ShouldReturnTrue()
    {
        var qty1 = Quantity.Create(10);
        var qty2 = Quantity.Create(10);

        qty1.Should().Be(qty2);
    }
}
