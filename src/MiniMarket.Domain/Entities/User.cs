using MiniMarket.Domain.Common;
using MiniMarket.Domain.Enums;
using MiniMarket.Domain.Exceptions;

namespace MiniMarket.Domain.Entities;

public class User : AggregateRoot<Guid>
{
    public string Username { get; private set; } = default!;
    public string PasswordHash { get; private set; } = default!;
    public string FullName { get; private set; } = default!;
    public UserRole Role { get; private set; }
    public bool IsActive { get; private set; }
    public DateTime? LastLoginAt { get; private set; }

    private User() : base()
    {
    }

    public static User Create(
        string username,
        string passwordHash,
        string fullName,
        UserRole role)
    {
        if (string.IsNullOrWhiteSpace(username))
        {
            throw new DomainException("El nombre de usuario es requerido");
        }

        if (string.IsNullOrWhiteSpace(passwordHash))
        {
            throw new DomainException("La contraseña es requerida");
        }

        if (string.IsNullOrWhiteSpace(fullName))
        {
            throw new DomainException("El nombre completo es requerido");
        }

        return new User
        {
            Id = Guid.NewGuid(),
            Username = username.Trim().ToLowerInvariant(),
            PasswordHash = passwordHash,
            FullName = fullName.Trim(),
            Role = role,
            IsActive = true
        };
    }

    public void UpdatePassword(string newPasswordHash)
    {
        if (string.IsNullOrWhiteSpace(newPasswordHash))
        {
            throw new DomainException("La nueva contraseña es requerida");
        }

        PasswordHash = newPasswordHash;
        SetUpdated();
    }

    public void UpdateProfile(string fullName)
    {
        if (string.IsNullOrWhiteSpace(fullName))
        {
            throw new DomainException("El nombre completo es requerido");
        }

        FullName = fullName.Trim();
        SetUpdated();
    }

    public void ChangeRole(UserRole newRole)
    {
        Role = newRole;
        SetUpdated();
    }

    public void RecordLogin()
    {
        LastLoginAt = DateTime.UtcNow;
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

    public bool IsAdmin() => Role == UserRole.Admin;

    public bool IsCashier() => Role == UserRole.Cashier;
}
