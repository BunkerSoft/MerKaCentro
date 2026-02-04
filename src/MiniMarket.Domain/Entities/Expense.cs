using MiniMarket.Domain.Common;
using MiniMarket.Domain.Exceptions;
using MiniMarket.Domain.ValueObjects;

namespace MiniMarket.Domain.Entities;

public class Expense : AggregateRoot<Guid>
{
    public Guid? CashRegisterId { get; private set; }
    public CashRegister? CashRegister { get; private set; }
    public Guid UserId { get; private set; }
    public User? User { get; private set; }
    public Guid CategoryId { get; private set; }
    public ExpenseCategory? Category { get; private set; }
    public string Description { get; private set; } = default!;
    public Money Amount { get; private set; } = default!;
    public string? Reference { get; private set; }
    public string? Notes { get; private set; }

    private Expense() : base()
    {
    }

    public static Expense Create(
        Guid userId,
        Guid categoryId,
        string description,
        Money amount,
        Guid? cashRegisterId = null,
        string? reference = null,
        string? notes = null)
    {
        if (string.IsNullOrWhiteSpace(description))
        {
            throw new DomainException("La descripción del gasto es requerida");
        }

        if (amount.Amount <= 0)
        {
            throw new DomainException("El monto del gasto debe ser mayor a cero");
        }

        return new Expense
        {
            Id = Guid.NewGuid(),
            CashRegisterId = cashRegisterId,
            UserId = userId,
            CategoryId = categoryId,
            Description = description.Trim(),
            Amount = amount,
            Reference = reference?.Trim(),
            Notes = notes?.Trim()
        };
    }

    public void Update(
        Guid categoryId,
        string description,
        Money amount,
        string? reference,
        string? notes)
    {
        if (string.IsNullOrWhiteSpace(description))
        {
            throw new DomainException("La descripción del gasto es requerida");
        }

        if (amount.Amount <= 0)
        {
            throw new DomainException("El monto del gasto debe ser mayor a cero");
        }

        CategoryId = categoryId;
        Description = description.Trim();
        Amount = amount;
        Reference = reference?.Trim();
        Notes = notes?.Trim();
        SetUpdated();
    }
}
