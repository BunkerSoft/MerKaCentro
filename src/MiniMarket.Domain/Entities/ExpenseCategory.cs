using MiniMarket.Domain.Common;
using MiniMarket.Domain.Exceptions;

namespace MiniMarket.Domain.Entities;

public class ExpenseCategory : AggregateRoot<Guid>
{
    public string Name { get; private set; } = default!;
    public string? Description { get; private set; }
    public bool IsActive { get; private set; }

    private readonly List<Expense> _expenses = [];
    public IReadOnlyCollection<Expense> Expenses => _expenses.AsReadOnly();

    private ExpenseCategory() : base()
    {
    }

    public static ExpenseCategory Create(string name, string? description = null)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new DomainException("El nombre de la categoría es requerido");
        }

        return new ExpenseCategory
        {
            Id = Guid.NewGuid(),
            Name = name.Trim(),
            Description = description?.Trim(),
            IsActive = true
        };
    }

    public void Update(string name, string? description)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new DomainException("El nombre de la categoría es requerido");
        }

        Name = name.Trim();
        Description = description?.Trim();
        SetUpdated();
    }

    public void Activate()
    {
        IsActive = true;
        SetUpdated();
    }

    public void Deactivate()
    {
        IsActive = false;
        SetUpdated();
    }
}
