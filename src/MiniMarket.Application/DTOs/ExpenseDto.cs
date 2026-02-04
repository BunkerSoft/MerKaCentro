namespace MiniMarket.Application.DTOs;

public record ExpenseDto(
    Guid Id,
    Guid? CashRegisterId,
    Guid UserId,
    string UserName,
    Guid CategoryId,
    string CategoryName,
    string Description,
    decimal Amount,
    string? Reference,
    string? Notes,
    DateTime CreatedAt,
    DateTime? UpdatedAt);

public record CreateExpenseDto(
    Guid CategoryId,
    string Description,
    decimal Amount,
    Guid? CashRegisterId,
    string? Reference,
    string? Notes);

public record UpdateExpenseDto(
    Guid CategoryId,
    string Description,
    decimal Amount,
    string? Reference,
    string? Notes);

public record ExpenseCategoryDto(
    Guid Id,
    string Name,
    string? Description,
    bool IsActive,
    int ExpenseCount,
    DateTime CreatedAt,
    DateTime? UpdatedAt);

public record CreateExpenseCategoryDto(
    string Name,
    string? Description);

public record UpdateExpenseCategoryDto(
    string Name,
    string? Description);
