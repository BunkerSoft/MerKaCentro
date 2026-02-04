using MiniMarket.Domain.Common;
using MiniMarket.Domain.Enums;
using MiniMarket.Domain.Exceptions;
using MiniMarket.Domain.ValueObjects;

namespace MiniMarket.Domain.Entities;

public class Customer : AggregateRoot<Guid>
{
    public string Name { get; private set; } = default!;
    public DocumentNumber? DocumentNumber { get; private set; }
    public PhoneNumber? Phone { get; private set; }
    public Email? Email { get; private set; }
    public Address Address { get; private set; } = default!;
    public Money CreditLimit { get; private set; } = default!;
    public Money CurrentDebt { get; private set; } = default!;
    public CustomerStatus Status { get; private set; }
    public string? Notes { get; private set; }

    private readonly List<Sale> _sales = [];
    public IReadOnlyCollection<Sale> Sales => _sales.AsReadOnly();

    private readonly List<CreditPayment> _creditPayments = [];
    public IReadOnlyCollection<CreditPayment> CreditPayments => _creditPayments.AsReadOnly();

    private Customer() : base()
    {
    }

    public static Customer Create(
        string name,
        DocumentNumber? documentNumber = null,
        PhoneNumber? phone = null,
        Email? email = null,
        Address? address = null,
        Money? creditLimit = null,
        string? notes = null)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new DomainException("El nombre del cliente es requerido");
        }

        return new Customer
        {
            Id = Guid.NewGuid(),
            Name = name.Trim(),
            DocumentNumber = documentNumber,
            Phone = phone,
            Email = email,
            Address = address ?? Address.Empty(),
            CreditLimit = creditLimit ?? Money.Zero(),
            CurrentDebt = Money.Zero(),
            Status = CustomerStatus.Active,
            Notes = notes?.Trim()
        };
    }

    public void Update(
        string name,
        DocumentNumber? documentNumber,
        PhoneNumber? phone,
        Email? email,
        Address address,
        string? notes)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new DomainException("El nombre del cliente es requerido");
        }

        Name = name.Trim();
        DocumentNumber = documentNumber;
        Phone = phone;
        Email = email;
        Address = address;
        Notes = notes?.Trim();
        SetUpdated();
    }

    public void SetCreditLimit(Money creditLimit)
    {
        if (creditLimit.Amount < 0)
        {
            throw new DomainException("El límite de crédito no puede ser negativo");
        }

        CreditLimit = creditLimit;
        SetUpdated();
    }

    public Money GetAvailableCredit()
    {
        if (CreditLimit.Amount <= CurrentDebt.Amount)
        {
            return Money.Zero(CreditLimit.Currency);
        }

        return CreditLimit.Subtract(CurrentDebt);
    }

    public bool HasAvailableCredit(Money amount)
    {
        return GetAvailableCredit() >= amount;
    }

    public void AddDebt(Money amount)
    {
        if (amount.Amount <= 0)
        {
            throw new DomainException("El monto de la deuda debe ser mayor a cero");
        }

        Money newDebt = CurrentDebt.Add(amount);

        if (newDebt > CreditLimit && CreditLimit.Amount > 0)
        {
            throw new CreditLimitExceededException(Id, amount.Amount, GetAvailableCredit().Amount);
        }

        CurrentDebt = newDebt;
        SetUpdated();
    }

    public void ReduceDebt(Money amount)
    {
        if (amount.Amount <= 0)
        {
            throw new DomainException("El monto del pago debe ser mayor a cero");
        }

        if (amount > CurrentDebt)
        {
            throw new DomainException("El pago excede la deuda actual");
        }

        CurrentDebt = CurrentDebt.Subtract(amount);
        SetUpdated();
    }

    public void RegisterPayment(Money amount, PaymentMethod method, string? reference = null, string? notes = null)
    {
        if (amount.Amount <= 0)
        {
            throw new DomainException("El monto del pago debe ser mayor a cero");
        }

        if (amount > CurrentDebt)
        {
            throw new DomainException("El pago excede la deuda actual");
        }

        var payment = CreditPayment.Create(Id, amount, method, reference, notes);
        _creditPayments.Add(payment);

        ReduceDebt(amount);
    }

    public bool HasDebt() => CurrentDebt.Amount > 0;

    public void Activate()
    {
        Status = CustomerStatus.Active;
        SetUpdated();
    }

    public void Deactivate()
    {
        Status = CustomerStatus.Inactive;
        SetUpdated();
    }

    public void Block(string? reason = null)
    {
        Status = CustomerStatus.Blocked;
        if (!string.IsNullOrWhiteSpace(reason))
        {
            Notes = $"{Notes}\n[BLOQUEADO] {DateTime.Now:g}: {reason}".Trim();
        }
        SetUpdated();
    }
}
