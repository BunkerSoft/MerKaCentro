using FluentAssertions;
using MiniMarket.Domain.Exceptions;
using MiniMarket.Domain.ValueObjects;

namespace MiniMarket.Domain.Tests.ValueObjects;

public class MoneyTests
{
    [Fact]
    public void Create_WithValidAmount_ShouldCreateMoney()
    {
        var money = Money.Create(100.50m, "PEN");

        money.Amount.Should().Be(100.50m);
        money.Currency.Should().Be("PEN");
    }

    [Fact]
    public void Create_WithNegativeAmount_ShouldThrowDomainException()
    {
        var act = () => Money.Create(-10m);

        act.Should().Throw<DomainException>()
            .WithMessage("El monto no puede ser negativo");
    }

    [Fact]
    public void Create_WithEmptyCurrency_ShouldThrowDomainException()
    {
        var act = () => Money.Create(100m, "");

        act.Should().Throw<DomainException>()
            .WithMessage("La moneda es requerida");
    }

    [Fact]
    public void Create_ShouldRoundToTwoDecimals()
    {
        var money = Money.Create(100.555m);

        money.Amount.Should().Be(100.56m);
    }

    [Fact]
    public void Add_WithSameCurrency_ShouldReturnSum()
    {
        var money1 = Money.Create(100m);
        var money2 = Money.Create(50m);

        var result = money1.Add(money2);

        result.Amount.Should().Be(150m);
    }

    [Fact]
    public void Add_WithDifferentCurrency_ShouldThrowDomainException()
    {
        var money1 = Money.Create(100m, "PEN");
        var money2 = Money.Create(50m, "USD");

        var act = () => money1.Add(money2);

        act.Should().Throw<DomainException>()
            .WithMessage("No se pueden operar monedas diferentes: PEN y USD");
    }

    [Fact]
    public void Subtract_WithSameCurrency_ShouldReturnDifference()
    {
        var money1 = Money.Create(100m);
        var money2 = Money.Create(30m);

        var result = money1.Subtract(money2);

        result.Amount.Should().Be(70m);
    }

    [Fact]
    public void Subtract_ResultingNegative_ShouldThrowDomainException()
    {
        var money1 = Money.Create(30m);
        var money2 = Money.Create(100m);

        var act = () => money1.Subtract(money2);

        act.Should().Throw<DomainException>()
            .WithMessage("El resultado no puede ser negativo");
    }

    [Fact]
    public void Multiply_WithPositiveFactor_ShouldReturnProduct()
    {
        var money = Money.Create(100m);

        var result = money.Multiply(2.5m);

        result.Amount.Should().Be(250m);
    }

    [Fact]
    public void Multiply_WithNegativeFactor_ShouldThrowDomainException()
    {
        var money = Money.Create(100m);

        var act = () => money.Multiply(-1m);

        act.Should().Throw<DomainException>()
            .WithMessage("El factor no puede ser negativo");
    }

    [Fact]
    public void Multiply_WithQuantity_ShouldReturnProduct()
    {
        var money = Money.Create(10m);
        var quantity = Quantity.Create(3);

        var result = money.Multiply(quantity);

        result.Amount.Should().Be(30m);
    }

    [Fact]
    public void Zero_ShouldReturnMoneyWithZeroAmount()
    {
        var money = Money.Zero();

        money.Amount.Should().Be(0);
        money.Currency.Should().Be("PEN");
    }

    [Fact]
    public void Operators_ShouldWorkCorrectly()
    {
        var money1 = Money.Create(100m);
        var money2 = Money.Create(50m);

        (money1 + money2).Amount.Should().Be(150m);
        (money1 - money2).Amount.Should().Be(50m);
        (money1 * 2m).Amount.Should().Be(200m);
        (money1 > money2).Should().BeTrue();
        (money2 < money1).Should().BeTrue();
        (money1 >= money2).Should().BeTrue();
        (money2 <= money1).Should().BeTrue();
    }

    [Fact]
    public void Equals_WithSameValues_ShouldReturnTrue()
    {
        var money1 = Money.Create(100m, "PEN");
        var money2 = Money.Create(100m, "PEN");

        money1.Should().Be(money2);
    }

    [Fact]
    public void Equals_WithDifferentValues_ShouldReturnFalse()
    {
        var money1 = Money.Create(100m, "PEN");
        var money2 = Money.Create(200m, "PEN");

        money1.Should().NotBe(money2);
    }

    [Fact]
    public void ToString_ShouldReturnFormattedString()
    {
        var money = Money.Create(1234.56m, "PEN");

        money.ToString().Should().Contain("PEN").And.Contain("1");
    }
}
