using MerkaCentro.Domain.Common;
using MerkaCentro.Domain.Exceptions;

namespace MerkaCentro.Domain.Entities;

public class UnitOfMeasure : AggregateRoot<Guid>
{
    public string Code { get; private set; } = default!;
    public string Name { get; private set; } = default!;
    public string Group { get; private set; } = default!;
    public bool AllowsFractions { get; private set; }
    public bool IsActive { get; private set; }
    public int DisplayOrder { get; private set; }

    private UnitOfMeasure() : base() { }

    public static UnitOfMeasure Create(
        string code,
        string name,
        string group,
        bool allowsFractions = false,
        int displayOrder = 0)
    {
        if (string.IsNullOrWhiteSpace(code))
            throw new DomainException("El codigo de la unidad de medida es requerido");

        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("El nombre de la unidad de medida es requerido");

        if (string.IsNullOrWhiteSpace(group))
            throw new DomainException("El grupo de la unidad de medida es requerido");

        return new UnitOfMeasure
        {
            Id = Guid.NewGuid(),
            Code = code.Trim().ToUpperInvariant(),
            Name = name.Trim(),
            Group = group.Trim(),
            AllowsFractions = allowsFractions,
            IsActive = true,
            DisplayOrder = displayOrder
        };
    }

    public void Update(string name, string group, bool allowsFractions, int displayOrder)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("El nombre de la unidad de medida es requerido");

        Name = name.Trim();
        Group = group.Trim();
        AllowsFractions = allowsFractions;
        DisplayOrder = displayOrder;
        SetUpdated();
    }

    public void Activate() { IsActive = true; SetUpdated(); }
    public void Deactivate() { IsActive = false; SetUpdated(); }
}
