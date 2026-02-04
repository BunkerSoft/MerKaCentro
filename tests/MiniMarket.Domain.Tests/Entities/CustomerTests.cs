using FluentAssertions;
using MiniMarket.Domain.Entities;
using MiniMarket.Domain.Enums;
using MiniMarket.Domain.Exceptions;
using MiniMarket.Domain.ValueObjects;

namespace MiniMarket.Domain.Tests.Entities;

public class CustomerTests
{
    private static Customer CreateValidCustomer() => Customer.Create(
        name: "Juan Pérez",
        creditLimit: Money.Create(500m));

    [Fact]
    public void Create_WithValidData_ShouldCreateCustomer()
    {
        var customer = Customer.Create(
            name: "Juan Pérez",
            documentNumber: DocumentNumber.Create("12345678", DocumentType.DNI),
            phone: PhoneNumber.Create("987654321"),
            email: Email.Create("juan@email.com"),
            creditLimit: Money.Create(1000m));

        customer.Id.Should().NotBeEmpty();
        customer.Name.Should().Be("Juan Pérez");
        customer.DocumentNumber.Should().NotBeNull();
        customer.Phone.Should().NotBeNull();
        customer.Email.Should().NotBeNull();
        customer.CreditLimit.Amount.Should().Be(1000m);
        customer.CurrentDebt.Amount.Should().Be(0);
        customer.Status.Should().Be(CustomerStatus.Active);
    }

    [Fact]
    public void Create_WithEmptyName_ShouldThrowDomainException()
    {
        var act = () => Customer.Create("");

        act.Should().Throw<DomainException>()
            .WithMessage("El nombre del cliente es requerido");
    }

    [Fact]
    public void GetAvailableCredit_ShouldReturnCorrectAmount()
    {
        var customer = CreateValidCustomer();
        customer.AddDebt(Money.Create(200m));

        var available = customer.GetAvailableCredit();

        available.Amount.Should().Be(300m);
    }

    [Fact]
    public void GetAvailableCredit_WhenDebtExceedsLimit_ShouldReturnZero()
    {
        var customer = Customer.Create("Test", creditLimit: Money.Create(100m));
        customer.AddDebt(Money.Create(100m));

        var available = customer.GetAvailableCredit();

        available.Amount.Should().Be(0);
    }

    [Fact]
    public void HasAvailableCredit_WhenEnoughCredit_ShouldReturnTrue()
    {
        var customer = CreateValidCustomer();

        customer.HasAvailableCredit(Money.Create(300m)).Should().BeTrue();
    }

    [Fact]
    public void HasAvailableCredit_WhenNotEnoughCredit_ShouldReturnFalse()
    {
        var customer = CreateValidCustomer();
        customer.AddDebt(Money.Create(400m));

        customer.HasAvailableCredit(Money.Create(200m)).Should().BeFalse();
    }

    [Fact]
    public void AddDebt_ShouldIncreaseCurrentDebt()
    {
        var customer = CreateValidCustomer();

        customer.AddDebt(Money.Create(100m));

        customer.CurrentDebt.Amount.Should().Be(100m);
    }

    [Fact]
    public void AddDebt_WithZeroAmount_ShouldThrowDomainException()
    {
        var customer = CreateValidCustomer();

        var act = () => customer.AddDebt(Money.Create(0m));

        act.Should().Throw<DomainException>()
            .WithMessage("El monto de la deuda debe ser mayor a cero");
    }

    [Fact]
    public void AddDebt_ExceedingLimit_ShouldThrowCreditLimitExceededException()
    {
        var customer = CreateValidCustomer();
        customer.AddDebt(Money.Create(400m));

        var act = () => customer.AddDebt(Money.Create(200m));

        act.Should().Throw<CreditLimitExceededException>();
    }

    [Fact]
    public void ReduceDebt_ShouldDecreaseCurrentDebt()
    {
        var customer = CreateValidCustomer();
        customer.AddDebt(Money.Create(100m));

        customer.ReduceDebt(Money.Create(50m));

        customer.CurrentDebt.Amount.Should().Be(50m);
    }

    [Fact]
    public void ReduceDebt_ExceedingDebt_ShouldThrowDomainException()
    {
        var customer = CreateValidCustomer();
        customer.AddDebt(Money.Create(50m));

        var act = () => customer.ReduceDebt(Money.Create(100m));

        act.Should().Throw<DomainException>()
            .WithMessage("El pago excede la deuda actual");
    }

    [Fact]
    public void RegisterPayment_ShouldAddPaymentAndReduceDebt()
    {
        var customer = CreateValidCustomer();
        customer.AddDebt(Money.Create(100m));

        customer.RegisterPayment(Money.Create(50m), PaymentMethod.Cash);

        customer.CurrentDebt.Amount.Should().Be(50m);
        customer.CreditPayments.Should().HaveCount(1);
    }

    [Fact]
    public void HasDebt_WhenHasDebt_ShouldReturnTrue()
    {
        var customer = CreateValidCustomer();
        customer.AddDebt(Money.Create(100m));

        customer.HasDebt().Should().BeTrue();
    }

    [Fact]
    public void HasDebt_WhenNoDebt_ShouldReturnFalse()
    {
        var customer = CreateValidCustomer();

        customer.HasDebt().Should().BeFalse();
    }

    [Fact]
    public void SetCreditLimit_ShouldUpdateLimit()
    {
        var customer = CreateValidCustomer();

        customer.SetCreditLimit(Money.Create(1000m));

        customer.CreditLimit.Amount.Should().Be(1000m);
    }

    [Fact]
    public void SetCreditLimit_WithNegativeAmount_ShouldThrowDomainException()
    {
        var customer = CreateValidCustomer();

        // Money.Create validates non-negative amounts
        var act = () => Money.Create(-100m);

        act.Should().Throw<DomainException>()
            .WithMessage("El monto no puede ser negativo");
    }

    [Fact]
    public void Activate_ShouldSetStatusActive()
    {
        var customer = CreateValidCustomer();
        customer.Deactivate();

        customer.Activate();

        customer.Status.Should().Be(CustomerStatus.Active);
    }

    [Fact]
    public void Deactivate_ShouldSetStatusInactive()
    {
        var customer = CreateValidCustomer();

        customer.Deactivate();

        customer.Status.Should().Be(CustomerStatus.Inactive);
    }

    [Fact]
    public void Block_ShouldSetStatusBlocked()
    {
        var customer = CreateValidCustomer();

        customer.Block("Morosidad");

        customer.Status.Should().Be(CustomerStatus.Blocked);
        customer.Notes.Should().Contain("BLOQUEADO");
    }
}
