namespace MiniMarket.Application.DTOs;

public record SupplierDto(
    Guid Id,
    string Name,
    string? BusinessName,
    string? Ruc,
    string? Phone,
    string? Email,
    string? Address,
    string? ContactPerson,
    bool IsActive,
    string? Notes,
    DateTime CreatedAt,
    DateTime? UpdatedAt);

public record CreateSupplierDto(
    string Name,
    string? BusinessName,
    string? Ruc,
    string? Phone,
    string? Email,
    string? Street,
    string? District,
    string? City,
    string? ContactPerson,
    string? Notes);

public record UpdateSupplierDto(
    string Name,
    string? BusinessName,
    string? Ruc,
    string? Phone,
    string? Email,
    string? Street,
    string? District,
    string? City,
    string? ContactPerson,
    string? Notes);
