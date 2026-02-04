using MiniMarket.Domain.Enums;

namespace MiniMarket.Application.DTOs;

public record CustomerDto(
    Guid Id,
    string Name,
    string? DocumentNumber,
    DocumentType? DocumentType,
    string? Phone,
    string? Email,
    string? Address,
    decimal CreditLimit,
    decimal CurrentDebt,
    decimal AvailableCredit,
    CustomerStatus Status,
    string? Notes,
    DateTime CreatedAt,
    DateTime? UpdatedAt);

public record CreateCustomerDto(
    string Name,
    string? DocumentNumber,
    DocumentType? DocumentType,
    string? Phone,
    string? Email,
    string? Street,
    string? District,
    string? City,
    string? Reference,
    decimal CreditLimit = 0,
    string? Notes = null);

public record UpdateCustomerDto(
    string Name,
    string? DocumentNumber,
    DocumentType? DocumentType,
    string? Phone,
    string? Email,
    string? Street,
    string? District,
    string? City,
    string? Reference,
    string? Notes);

public record CustomerDebtDto(
    Guid Id,
    string Name,
    string? DocumentNumber,
    decimal CurrentDebt,
    decimal CreditLimit,
    DateTime? LastPurchaseDate);
