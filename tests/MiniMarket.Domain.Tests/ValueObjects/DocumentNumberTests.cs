using FluentAssertions;
using MiniMarket.Domain.Enums;
using MiniMarket.Domain.Exceptions;
using MiniMarket.Domain.ValueObjects;

namespace MiniMarket.Domain.Tests.ValueObjects;

public class DocumentNumberTests
{
    [Theory]
    [InlineData("12345678", DocumentType.DNI)]
    [InlineData("10123456789", DocumentType.RUC)]
    [InlineData("20123456789", DocumentType.RUC)]
    [InlineData("AB1234567", DocumentType.CE)]
    public void Create_WithValidDocument_ShouldCreateDocumentNumber(string value, DocumentType type)
    {
        var document = DocumentNumber.Create(value, type);

        document.Value.Should().Be(value);
        document.Type.Should().Be(type);
    }

    [Fact]
    public void Create_WithEmptyValue_ShouldThrowDomainException()
    {
        var act = () => DocumentNumber.Create("", DocumentType.DNI);

        act.Should().Throw<DomainException>()
            .WithMessage("El número de documento es requerido");
    }

    [Theory]
    [InlineData("1234567", DocumentType.DNI)]
    [InlineData("123456789", DocumentType.DNI)]
    public void Create_WithInvalidDNI_ShouldThrowDomainException(string value, DocumentType type)
    {
        var act = () => DocumentNumber.Create(value, type);

        act.Should().Throw<DomainException>()
            .WithMessage($"El número de documento no es válido para el tipo {type}");
    }

    [Theory]
    [InlineData("12345678901", DocumentType.RUC)]
    [InlineData("30123456789", DocumentType.RUC)]
    public void Create_WithInvalidRUC_ShouldThrowDomainException(string value, DocumentType type)
    {
        var act = () => DocumentNumber.Create(value, type);

        act.Should().Throw<DomainException>()
            .WithMessage($"El número de documento no es válido para el tipo {type}");
    }

    [Theory]
    [InlineData("12345678", DocumentType.CE)]
    [InlineData("1234567890123", DocumentType.CE)]
    public void Create_WithInvalidCE_ShouldThrowDomainException(string value, DocumentType type)
    {
        var act = () => DocumentNumber.Create(value, type);

        act.Should().Throw<DomainException>()
            .WithMessage($"El número de documento no es válido para el tipo {type}");
    }

    [Fact]
    public void CreateOptional_WithNull_ShouldReturnNull()
    {
        var document = DocumentNumber.CreateOptional(null, DocumentType.DNI);

        document.Should().BeNull();
    }

    [Fact]
    public void CreateOptional_WithNullType_ShouldReturnNull()
    {
        var document = DocumentNumber.CreateOptional("12345678", null);

        document.Should().BeNull();
    }

    [Fact]
    public void CreateOptional_WithValidValues_ShouldCreateDocumentNumber()
    {
        var document = DocumentNumber.CreateOptional("12345678", DocumentType.DNI);

        document.Should().NotBeNull();
        document!.Value.Should().Be("12345678");
        document.Type.Should().Be(DocumentType.DNI);
    }

    [Fact]
    public void ToString_ShouldReturnFormattedString()
    {
        var document = DocumentNumber.Create("12345678", DocumentType.DNI);

        document.ToString().Should().Contain("DNI").And.Contain("12345678");
    }

    [Fact]
    public void Equals_WithSameValues_ShouldReturnTrue()
    {
        var doc1 = DocumentNumber.Create("12345678", DocumentType.DNI);
        var doc2 = DocumentNumber.Create("12345678", DocumentType.DNI);

        doc1.Should().Be(doc2);
    }

    [Fact]
    public void Equals_WithDifferentType_ShouldReturnFalse()
    {
        var doc1 = DocumentNumber.Create("AB1234567", DocumentType.CE);
        var doc2 = DocumentNumber.Create("12345678", DocumentType.DNI);

        doc1.Should().NotBe(doc2);
    }
}
