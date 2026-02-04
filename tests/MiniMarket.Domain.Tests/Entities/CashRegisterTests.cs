using FluentAssertions;
using MiniMarket.Domain.Entities;
using MiniMarket.Domain.Enums;
using MiniMarket.Domain.Exceptions;
using MiniMarket.Domain.ValueObjects;

namespace MiniMarket.Domain.Tests.Entities;

public class CashRegisterTests
{
    private static CashRegister CreateOpenRegister() =>
        CashRegister.Open(Guid.NewGuid(), Money.Create(100m));

    [Fact]
    public void Open_WithValidData_ShouldCreateCashRegister()
    {
        var userId = Guid.NewGuid();
        var register = CashRegister.Open(userId, Money.Create(100m), "Apertura normal");

        register.Id.Should().NotBeEmpty();
        register.UserId.Should().Be(userId);
        register.InitialCash.Amount.Should().Be(100m);
        register.CurrentCash.Amount.Should().Be(100m);
        register.ExpectedCash.Amount.Should().Be(100m);
        register.Status.Should().Be(CashRegisterStatus.Open);
        register.Movements.Should().HaveCount(1);
    }

    [Fact]
    public void Open_WithNegativeInitialCash_ShouldThrowDomainException()
    {
        // Money.Create validates non-negative amounts
        var act = () => Money.Create(-100m);

        act.Should().Throw<DomainException>()
            .WithMessage("El monto no puede ser negativo");
    }

    [Fact]
    public void RegisterSale_ShouldIncreaseCash()
    {
        var register = CreateOpenRegister();

        register.RegisterSale(Money.Create(50m), "V-001");

        register.CurrentCash.Amount.Should().Be(150m);
        register.ExpectedCash.Amount.Should().Be(150m);
        register.Movements.Should().HaveCount(2);
    }

    [Fact]
    public void RegisterSale_WithZeroAmount_ShouldThrowDomainException()
    {
        var register = CreateOpenRegister();

        var act = () => register.RegisterSale(Money.Create(0m), "V-001");

        act.Should().Throw<DomainException>()
            .WithMessage("El monto de la venta debe ser mayor a cero");
    }

    [Fact]
    public void RegisterWithdrawal_ShouldDecreaseCash()
    {
        var register = CreateOpenRegister();

        register.RegisterWithdrawal(Money.Create(30m), "Retiro para banco");

        register.CurrentCash.Amount.Should().Be(70m);
        register.ExpectedCash.Amount.Should().Be(70m);
        register.Movements.Last().Amount.Amount.Should().Be(-30m);
    }

    [Fact]
    public void RegisterWithdrawal_ExceedingCash_ShouldThrowDomainException()
    {
        var register = CreateOpenRegister();

        var act = () => register.RegisterWithdrawal(Money.Create(150m), "Retiro");

        act.Should().Throw<DomainException>()
            .WithMessage("No hay suficiente efectivo en caja");
    }

    [Fact]
    public void RegisterDeposit_ShouldIncreaseCash()
    {
        var register = CreateOpenRegister();

        register.RegisterDeposit(Money.Create(50m), "Depósito adicional");

        register.CurrentCash.Amount.Should().Be(150m);
        register.ExpectedCash.Amount.Should().Be(150m);
    }

    [Fact]
    public void RegisterExpense_ShouldDecreaseCash()
    {
        var register = CreateOpenRegister();

        register.RegisterExpense(Money.Create(20m), "Compra de bolsas");

        register.CurrentCash.Amount.Should().Be(80m);
        register.ExpectedCash.Amount.Should().Be(80m);
        register.Movements.Last().Amount.Amount.Should().Be(-20m);
    }

    [Fact]
    public void RegisterExpense_ExceedingCash_ShouldThrowDomainException()
    {
        var register = CreateOpenRegister();

        var act = () => register.RegisterExpense(Money.Create(150m), "Gasto");

        act.Should().Throw<DomainException>()
            .WithMessage("No hay suficiente efectivo en caja para el gasto");
    }

    [Fact]
    public void RegisterCreditPayment_ShouldIncreaseCash()
    {
        var register = CreateOpenRegister();

        register.RegisterCreditPayment(Money.Create(50m), "Juan Pérez");

        register.CurrentCash.Amount.Should().Be(150m);
        register.ExpectedCash.Amount.Should().Be(150m);
    }

    [Fact]
    public void Close_ShouldSetStatusClosedAndCalculateDifference()
    {
        var register = CreateOpenRegister();
        register.RegisterSale(Money.Create(100m), "V-001");

        register.Close(Money.Create(195m), "Faltante de 5 soles");

        register.Status.Should().Be(CashRegisterStatus.Closed);
        register.FinalCash!.Amount.Should().Be(195m);
        register.Difference!.Amount.Should().Be(-5m);
        register.ClosedAt.Should().NotBeNull();
    }

    [Fact]
    public void Close_WithExactCash_ShouldHaveZeroDifference()
    {
        var register = CreateOpenRegister();
        register.RegisterSale(Money.Create(100m), "V-001");

        register.Close(Money.Create(200m));

        register.Difference!.Amount.Should().Be(0m);
    }

    [Fact]
    public void Operation_OnClosedRegister_ShouldThrowDomainException()
    {
        var register = CreateOpenRegister();
        register.Close(Money.Create(100m));

        var act = () => register.RegisterSale(Money.Create(50m), "V-001");

        act.Should().Throw<DomainException>()
            .WithMessage("La caja no está abierta");
    }

    [Fact]
    public void GetTotalSales_ShouldReturnSumOfSales()
    {
        var register = CreateOpenRegister();
        register.RegisterSale(Money.Create(50m), "V-001");
        register.RegisterSale(Money.Create(30m), "V-002");

        var total = register.GetTotalSales();

        total.Amount.Should().Be(80m);
    }

    [Fact]
    public void GetTotalExpenses_ShouldReturnSumOfExpenses()
    {
        var register = CreateOpenRegister();
        register.RegisterExpense(Money.Create(20m), "Gasto 1");
        register.RegisterExpense(Money.Create(15m), "Gasto 2");

        var total = register.GetTotalExpenses();

        total.Amount.Should().Be(35m);
    }

    [Fact]
    public void GetTotalWithdrawals_ShouldReturnSumOfWithdrawals()
    {
        var register = CreateOpenRegister();
        register.RegisterWithdrawal(Money.Create(30m), "Retiro 1");
        register.RegisterWithdrawal(Money.Create(20m), "Retiro 2");

        var total = register.GetTotalWithdrawals();

        total.Amount.Should().Be(50m);
    }

    [Fact]
    public void GetTotalDeposits_ShouldReturnSumOfDeposits()
    {
        var register = CreateOpenRegister();
        register.RegisterDeposit(Money.Create(50m), "Depósito 1");
        register.RegisterDeposit(Money.Create(25m), "Depósito 2");

        var total = register.GetTotalDeposits();

        total.Amount.Should().Be(75m);
    }
}
