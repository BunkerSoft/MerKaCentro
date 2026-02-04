using FluentAssertions;
using MiniMarket.Domain.Entities;
using MiniMarket.Domain.Enums;
using MiniMarket.Domain.Exceptions;

namespace MiniMarket.Domain.Tests.Entities;

public class UserTests
{
    [Fact]
    public void Create_WithValidData_ShouldCreateUser()
    {
        var user = User.Create("admin", "hashedPassword123", "Administrador", UserRole.Admin);

        user.Id.Should().NotBeEmpty();
        user.Username.Should().Be("admin");
        user.PasswordHash.Should().Be("hashedPassword123");
        user.FullName.Should().Be("Administrador");
        user.Role.Should().Be(UserRole.Admin);
        user.IsActive.Should().BeTrue();
    }

    [Fact]
    public void Create_WithEmptyUsername_ShouldThrowDomainException()
    {
        var act = () => User.Create("", "password", "Nombre", UserRole.Cashier);

        act.Should().Throw<DomainException>()
            .WithMessage("El nombre de usuario es requerido");
    }

    [Fact]
    public void Create_WithEmptyPassword_ShouldThrowDomainException()
    {
        var act = () => User.Create("admin", "", "Nombre", UserRole.Cashier);

        act.Should().Throw<DomainException>()
            .WithMessage("La contraseña es requerida");
    }

    [Fact]
    public void Create_WithEmptyFullName_ShouldThrowDomainException()
    {
        var act = () => User.Create("admin", "password", "", UserRole.Cashier);

        act.Should().Throw<DomainException>()
            .WithMessage("El nombre completo es requerido");
    }

    [Fact]
    public void Create_ShouldNormalizeUsername()
    {
        var user = User.Create("  ADMIN  ", "password", "Nombre", UserRole.Admin);

        user.Username.Should().Be("admin");
    }

    [Fact]
    public void UpdatePassword_ShouldChangePasswordHash()
    {
        var user = User.Create("admin", "oldPassword", "Nombre", UserRole.Admin);

        user.UpdatePassword("newPassword");

        user.PasswordHash.Should().Be("newPassword");
    }

    [Fact]
    public void UpdatePassword_WithEmptyPassword_ShouldThrowDomainException()
    {
        var user = User.Create("admin", "oldPassword", "Nombre", UserRole.Admin);

        var act = () => user.UpdatePassword("");

        act.Should().Throw<DomainException>()
            .WithMessage("La nueva contraseña es requerida");
    }

    [Fact]
    public void UpdateProfile_ShouldChangeFullName()
    {
        var user = User.Create("admin", "password", "Nombre Antiguo", UserRole.Admin);

        user.UpdateProfile("Nombre Nuevo");

        user.FullName.Should().Be("Nombre Nuevo");
    }

    [Fact]
    public void ChangeRole_ShouldChangeUserRole()
    {
        var user = User.Create("admin", "password", "Nombre", UserRole.Cashier);

        user.ChangeRole(UserRole.Admin);

        user.Role.Should().Be(UserRole.Admin);
    }

    [Fact]
    public void RecordLogin_ShouldUpdateLastLoginAt()
    {
        var user = User.Create("admin", "password", "Nombre", UserRole.Admin);

        user.RecordLogin();

        user.LastLoginAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void Activate_ShouldSetIsActiveTrue()
    {
        var user = User.Create("admin", "password", "Nombre", UserRole.Admin);
        user.Deactivate();

        user.Activate();

        user.IsActive.Should().BeTrue();
    }

    [Fact]
    public void Deactivate_ShouldSetIsActiveFalse()
    {
        var user = User.Create("admin", "password", "Nombre", UserRole.Admin);

        user.Deactivate();

        user.IsActive.Should().BeFalse();
    }

    [Fact]
    public void IsAdmin_WhenAdmin_ShouldReturnTrue()
    {
        var user = User.Create("admin", "password", "Nombre", UserRole.Admin);

        user.IsAdmin().Should().BeTrue();
        user.IsCashier().Should().BeFalse();
    }

    [Fact]
    public void IsCashier_WhenCashier_ShouldReturnTrue()
    {
        var user = User.Create("cajero", "password", "Nombre", UserRole.Cashier);

        user.IsCashier().Should().BeTrue();
        user.IsAdmin().Should().BeFalse();
    }
}
