using MiniMarket.Domain.Common;
using MiniMarket.Domain.Exceptions;

namespace MiniMarket.Domain.Entities;

public class Category : AggregateRoot<Guid>
{
    public string Name { get; private set; } = default!;
    public string? Description { get; private set; }
    public bool IsActive { get; private set; }

    private readonly List<Product> _products = [];
    public IReadOnlyCollection<Product> Products => _products.AsReadOnly();

    private Category() : base()
    {
    }

    public static Category Create(string name, string? description = null)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new DomainException("El nombre de la categoría es requerido");
        }

        return new Category
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
