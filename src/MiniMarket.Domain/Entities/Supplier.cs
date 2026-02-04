using MiniMarket.Domain.Common;
using MiniMarket.Domain.Exceptions;
using MiniMarket.Domain.ValueObjects;

namespace MiniMarket.Domain.Entities;

public class Supplier : AggregateRoot<Guid>
{
    public string Name { get; private set; } = default!;
    public string? BusinessName { get; private set; }
    public DocumentNumber? Ruc { get; private set; }
    public PhoneNumber? Phone { get; private set; }
    public Email? Email { get; private set; }
    public Address Address { get; private set; } = default!;
    public string? ContactPerson { get; private set; }
    public bool IsActive { get; private set; }
    public string? Notes { get; private set; }

    private readonly List<PurchaseOrder> _purchaseOrders = [];
    public IReadOnlyCollection<PurchaseOrder> PurchaseOrders => _purchaseOrders.AsReadOnly();

    private Supplier() : base()
    {
    }

    public static Supplier Create(
        string name,
        string? businessName = null,
        DocumentNumber? ruc = null,
        PhoneNumber? phone = null,
        Email? email = null,
        Address? address = null,
        string? contactPerson = null,
        string? notes = null)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new DomainException("El nombre del proveedor es requerido");
        }

        return new Supplier
        {
            Id = Guid.NewGuid(),
            Name = name.Trim(),
            BusinessName = businessName?.Trim(),
            Ruc = ruc,
            Phone = phone,
            Email = email,
            Address = address ?? Address.Empty(),
            ContactPerson = contactPerson?.Trim(),
            IsActive = true,
            Notes = notes?.Trim()
        };
    }

    public void Update(
        string name,
        string? businessName,
        DocumentNumber? ruc,
        PhoneNumber? phone,
        Email? email,
        Address address,
        string? contactPerson,
        string? notes)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new DomainException("El nombre del proveedor es requerido");
        }

        Name = name.Trim();
        BusinessName = businessName?.Trim();
        Ruc = ruc;
        Phone = phone;
        Email = email;
        Address = address;
        ContactPerson = contactPerson?.Trim();
        Notes = notes?.Trim();
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
