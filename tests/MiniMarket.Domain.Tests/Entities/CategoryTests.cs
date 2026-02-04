using FluentAssertions;
using MiniMarket.Domain.Entities;
using MiniMarket.Domain.Exceptions;

namespace MiniMarket.Domain.Tests.Entities;

public class CategoryTests
{
    [Fact]
    public void Create_WithValidData_ShouldCreateCategory()
    {
        var category = Category.Create("Bebidas", "Bebidas y refrescos");

        category.Id.Should().NotBeEmpty();
        category.Name.Should().Be("Bebidas");
        category.Description.Should().Be("Bebidas y refrescos");
        category.IsActive.Should().BeTrue();
        category.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void Create_WithoutDescription_ShouldCreateCategory()
    {
        var category = Category.Create("Bebidas");

        category.Name.Should().Be("Bebidas");
        category.Description.Should().BeNull();
    }

    [Fact]
    public void Create_WithEmptyName_ShouldThrowDomainException()
    {
        var act = () => Category.Create("");

        act.Should().Throw<DomainException>()
            .WithMessage("El nombre de la categoría es requerido");
    }

    [Fact]
    public void Create_WithWhitespaceName_ShouldThrowDomainException()
    {
        var act = () => Category.Create("   ");

        act.Should().Throw<DomainException>()
            .WithMessage("El nombre de la categoría es requerido");
    }

    [Fact]
    public void Create_ShouldTrimNameAndDescription()
    {
        var category = Category.Create("  Bebidas  ", "  Descripción  ");

        category.Name.Should().Be("Bebidas");
        category.Description.Should().Be("Descripción");
    }

    [Fact]
    public void Update_WithValidData_ShouldUpdateCategory()
    {
        var category = Category.Create("Bebidas");

        category.Update("Lácteos", "Productos lácteos");

        category.Name.Should().Be("Lácteos");
        category.Description.Should().Be("Productos lácteos");
        category.UpdatedAt.Should().NotBeNull();
    }

    [Fact]
    public void Update_WithEmptyName_ShouldThrowDomainException()
    {
        var category = Category.Create("Bebidas");

        var act = () => category.Update("", null);

        act.Should().Throw<DomainException>()
            .WithMessage("El nombre de la categoría es requerido");
    }

    [Fact]
    public void Activate_ShouldSetIsActiveTrue()
    {
        var category = Category.Create("Bebidas");
        category.Deactivate();

        category.Activate();

        category.IsActive.Should().BeTrue();
    }

    [Fact]
    public void Deactivate_ShouldSetIsActiveFalse()
    {
        var category = Category.Create("Bebidas");

        category.Deactivate();

        category.IsActive.Should().BeFalse();
    }
}
