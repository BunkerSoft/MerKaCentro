using FluentAssertions;
using MiniMarket.Domain.Exceptions;
using MiniMarket.Domain.ValueObjects;

namespace MiniMarket.Domain.Tests.ValueObjects;

public class EmailTests
{
    [Theory]
    [InlineData("test@example.com")]
    [InlineData("user.name@domain.org")]
    [InlineData("user+tag@subdomain.domain.com")]
    public void Create_WithValidEmail_ShouldCreateEmail(string value)
    {
        var email = Email.Create(value);

        email.Value.Should().Be(value.ToLowerInvariant());
    }

    [Fact]
    public void Create_WithEmptyValue_ShouldThrowDomainException()
    {
        var act = () => Email.Create("");

        act.Should().Throw<DomainException>()
            .WithMessage("El correo electrónico es requerido");
    }

    [Theory]
    [InlineData("invalid")]
    [InlineData("invalid@")]
    [InlineData("@domain.com")]
    [InlineData("invalid@domain")]
    public void Create_WithInvalidEmail_ShouldThrowDomainException(string value)
    {
        var act = () => Email.Create(value);

        act.Should().Throw<DomainException>()
            .WithMessage("El correo electrónico no tiene un formato válido");
    }

    [Fact]
    public void Create_ShouldConvertToLowerCase()
    {
        var email = Email.Create("Test@Example.COM");

        email.Value.Should().Be("test@example.com");
    }

    [Fact]
    public void CreateOptional_WithNull_ShouldReturnNull()
    {
        var email = Email.CreateOptional(null);

        email.Should().BeNull();
    }

    [Fact]
    public void CreateOptional_WithValidValue_ShouldCreateEmail()
    {
        var email = Email.CreateOptional("test@example.com");

        email.Should().NotBeNull();
        email!.Value.Should().Be("test@example.com");
    }

    [Fact]
    public void Equals_WithSameValues_ShouldReturnTrue()
    {
        var email1 = Email.Create("test@example.com");
        var email2 = Email.Create("TEST@EXAMPLE.COM");

        email1.Should().Be(email2);
    }
}
