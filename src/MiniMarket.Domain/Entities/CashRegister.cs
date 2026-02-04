using MiniMarket.Domain.Common;
using MiniMarket.Domain.Enums;
using MiniMarket.Domain.Exceptions;
using MiniMarket.Domain.ValueObjects;

namespace MiniMarket.Domain.Entities;

public class CashRegister : AggregateRoot<Guid>
{
    public Guid UserId { get; private set; }
    public User? User { get; private set; }
    public Money InitialCash { get; private set; } = default!;
    public Money CurrentCash { get; private set; } = default!;
    public Money ExpectedCash { get; private set; } = default!;
    public Money? FinalCash { get; private set; }
    public Money? Difference { get; private set; }
    public DateTime OpenedAt { get; private set; }
    public DateTime? ClosedAt { get; private set; }
    public CashRegisterStatus Status { get; private set; }
    public string? Notes { get; private set; }

    private readonly List<CashMovement> _movements = [];
    public IReadOnlyCollection<CashMovement> Movements => _movements.AsReadOnly();

    private readonly List<Sale> _sales = [];
    public IReadOnlyCollection<Sale> Sales => _sales.AsReadOnly();

    private CashRegister() : base()
    {
    }

    public static CashRegister Open(Guid userId, Money initialCash, string? notes = null)
    {
        if (initialCash.Amount < 0)
        {
            throw new DomainException("El monto inicial no puede ser negativo");
        }

        var register = new CashRegister
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            InitialCash = initialCash,
            CurrentCash = initialCash,
            ExpectedCash = initialCash,
            OpenedAt = DateTime.UtcNow,
            Status = CashRegisterStatus.Open,
            Notes = notes?.Trim()
        };

        register.AddMovement(CashMovementType.InitialCash, initialCash, "Apertura de caja");

        return register;
    }

    public void RegisterSale(Money amount, string reference)
    {
        EnsureOpen();

        if (amount.Amount <= 0)
        {
            throw new DomainException("El monto de la venta debe ser mayor a cero");
        }

        CurrentCash = CurrentCash.Add(amount);
        ExpectedCash = ExpectedCash.Add(amount);
        AddMovement(CashMovementType.Sale, amount, $"Venta: {reference}");
    }

    public void RegisterWithdrawal(Money amount, string reason)
    {
        EnsureOpen();

        if (amount.Amount <= 0)
        {
            throw new DomainException("El monto del retiro debe ser mayor a cero");
        }

        if (amount > CurrentCash)
        {
            throw new DomainException("No hay suficiente efectivo en caja");
        }

        CurrentCash = CurrentCash.Subtract(amount);
        ExpectedCash = ExpectedCash.Subtract(amount);
        AddMovement(CashMovementType.Withdrawal, amount.Negate(), $"Retiro: {reason}");
    }

    public void RegisterDeposit(Money amount, string reason)
    {
        EnsureOpen();

        if (amount.Amount <= 0)
        {
            throw new DomainException("El monto del depósito debe ser mayor a cero");
        }

        CurrentCash = CurrentCash.Add(amount);
        ExpectedCash = ExpectedCash.Add(amount);
        AddMovement(CashMovementType.Deposit, amount, $"Depósito: {reason}");
    }

    public void RegisterExpense(Money amount, string description)
    {
        EnsureOpen();

        if (amount.Amount <= 0)
        {
            throw new DomainException("El monto del gasto debe ser mayor a cero");
        }

        if (amount > CurrentCash)
        {
            throw new DomainException("No hay suficiente efectivo en caja para el gasto");
        }

        CurrentCash = CurrentCash.Subtract(amount);
        ExpectedCash = ExpectedCash.Subtract(amount);
        AddMovement(CashMovementType.Expense, amount.Negate(), $"Gasto: {description}");
    }

    public void RegisterCreditPayment(Money amount, string customerName)
    {
        EnsureOpen();

        if (amount.Amount <= 0)
        {
            throw new DomainException("El monto del pago debe ser mayor a cero");
        }

        CurrentCash = CurrentCash.Add(amount);
        ExpectedCash = ExpectedCash.Add(amount);
        AddMovement(CashMovementType.CreditPayment, amount, $"Pago de crédito: {customerName}");
    }

    public void Close(Money finalCash, string? notes = null)
    {
        EnsureOpen();

        FinalCash = finalCash;
        Difference = Money.CreateWithSign(finalCash.Amount - ExpectedCash.Amount, finalCash.Currency);
        ClosedAt = DateTime.UtcNow;
        Status = CashRegisterStatus.Closed;

        if (!string.IsNullOrWhiteSpace(notes))
        {
            Notes = $"{Notes}\n[CIERRE] {notes}".Trim();
        }

        SetUpdated();
    }

    public Money GetTotalSales()
    {
        return _movements
            .Where(m => m.Type == CashMovementType.Sale)
            .Aggregate(Money.Zero(), (acc, m) => acc.Add(m.Amount));
    }

    public Money GetTotalExpenses()
    {
        return _movements
            .Where(m => m.Type == CashMovementType.Expense)
            .Aggregate(Money.Zero(), (acc, m) => Money.CreateWithSign(acc.Amount + Math.Abs(m.Amount.Amount), acc.Currency));
    }

    public Money GetTotalWithdrawals()
    {
        return _movements
            .Where(m => m.Type == CashMovementType.Withdrawal)
            .Aggregate(Money.Zero(), (acc, m) => Money.CreateWithSign(acc.Amount + Math.Abs(m.Amount.Amount), acc.Currency));
    }

    public Money GetTotalDeposits()
    {
        return _movements
            .Where(m => m.Type == CashMovementType.Deposit)
            .Aggregate(Money.Zero(), (acc, m) => acc.Add(m.Amount));
    }

    private void EnsureOpen()
    {
        if (Status != CashRegisterStatus.Open)
        {
            throw new DomainException("La caja no está abierta");
        }
    }

    private void AddMovement(CashMovementType type, Money amount, string description)
    {
        var movement = CashMovement.Create(Id, type, amount, CurrentCash, description);
        _movements.Add(movement);
        SetUpdated();
    }
}
