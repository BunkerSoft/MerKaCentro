using FluentAssertions;
using MiniMarket.Domain.Exceptions;
using MiniMarket.Domain.ValueObjects;

namespace MiniMarket.Domain.Tests.ValueObjects;

public class BarcodeTests
{
    [Theory]
    [InlineData("12345678")]
    [InlineData("123456789012")]
    [InlineData("12345678901234")]
    public void Create_WithValidBarcode_ShouldCreateBarcode(string value)
    {
        var barcode = Barcode.Create(value);

        barcode.Value.Should().Be(value);
    }

    [Fact]
    public void Create_WithEmptyValue_ShouldThrowDomainException()
    {
        var act = () => Barcode.Create("");

        act.Should().Throw<DomainException>()
            .WithMessage("El código de barras es requerido");
    }

    [Fact]
    public void Create_WithWhitespace_ShouldThrowDomainException()
    {
        var act = () => Barcode.Create("   ");

        act.Should().Throw<DomainException>()
            .WithMessage("El código de barras es requerido");
    }

    [Theory]
    [InlineData("1234567")]
    [InlineData("123456789012345")]
    [InlineData("12345678abc")]
    [InlineData("abcdefgh")]
    public void Create_WithInvalidBarcode_ShouldThrowDomainException(string value)
    {
        var act = () => Barcode.Create(value);

        act.Should().Throw<DomainException>()
            .WithMessage("El código de barras debe contener solo números y tener entre 8 y 14 dígitos");
    }

    [Fact]
    public void CreateOptional_WithNull_ShouldReturnNull()
    {
        var barcode = Barcode.CreateOptional(null);

        barcode.Should().BeNull();
    }

    [Fact]
    public void CreateOptional_WithEmpty_ShouldReturnNull()
    {
        var barcode = Barcode.CreateOptional("");

        barcode.Should().BeNull();
    }

    [Fact]
    public void CreateOptional_WithValidValue_ShouldCreateBarcode()
    {
        var barcode = Barcode.CreateOptional("12345678");

        barcode.Should().NotBeNull();
        barcode!.Value.Should().Be("12345678");
    }

    [Fact]
    public void ToString_ShouldReturnValue()
    {
        var barcode = Barcode.Create("12345678");

        barcode.ToString().Should().Be("12345678");
    }

    [Fact]
    public void Equals_WithSameValues_ShouldReturnTrue()
    {
        var barcode1 = Barcode.Create("12345678");
        var barcode2 = Barcode.Create("12345678");

        barcode1.Should().Be(barcode2);
    }
}
