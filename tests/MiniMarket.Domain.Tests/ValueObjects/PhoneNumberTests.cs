using FluentAssertions;
using MiniMarket.Domain.Exceptions;
using MiniMarket.Domain.ValueObjects;

namespace MiniMarket.Domain.Tests.ValueObjects;

public class PhoneNumberTests
{
    [Theory]
    [InlineData("1234567", "1234567")]
    [InlineData("123-456-7890", "1234567890")]
    [InlineData("+51 987 654 321", "51987654321")]
    [InlineData("(01) 234-5678", "012345678")]
    public void Create_WithValidPhone_ShouldCreatePhoneNumber(string input, string expected)
    {
        var phone = PhoneNumber.Create(input);

        phone.Value.Should().Be(expected);
    }

    [Fact]
    public void Create_WithEmptyValue_ShouldThrowDomainException()
    {
        var act = () => PhoneNumber.Create("");

        act.Should().Throw<DomainException>()
            .WithMessage("El número de teléfono es requerido");
    }

    [Fact]
    public void Create_WithTooShortNumber_ShouldThrowDomainException()
    {
        var act = () => PhoneNumber.Create("123456");

        act.Should().Throw<DomainException>()
            .WithMessage("El número de teléfono debe tener entre 7 y 15 dígitos");
    }

    [Fact]
    public void Create_WithTooLongNumber_ShouldThrowDomainException()
    {
        var act = () => PhoneNumber.Create("1234567890123456");

        act.Should().Throw<DomainException>()
            .WithMessage("El número de teléfono debe tener entre 7 y 15 dígitos");
    }

    [Fact]
    public void CreateOptional_WithNull_ShouldReturnNull()
    {
        var phone = PhoneNumber.CreateOptional(null);

        phone.Should().BeNull();
    }

    [Fact]
    public void CreateOptional_WithValidValue_ShouldCreatePhoneNumber()
    {
        var phone = PhoneNumber.CreateOptional("1234567");

        phone.Should().NotBeNull();
        phone!.Value.Should().Be("1234567");
    }

    [Fact]
    public void Equals_WithSameValues_ShouldReturnTrue()
    {
        var phone1 = PhoneNumber.Create("1234567");
        var phone2 = PhoneNumber.Create("1234567");

        phone1.Should().Be(phone2);
    }
}
