using MiniMarket.Domain.Enums;

namespace MiniMarket.Application.DTOs;

public record UserDto(
    Guid Id,
    string Username,
    string FullName,
    UserRole Role,
    bool IsActive,
    DateTime? LastLoginAt,
    DateTime CreatedAt,
    DateTime? UpdatedAt);

public record CreateUserDto(
    string Username,
    string Password,
    string FullName,
    UserRole Role);

public record UpdateUserDto(
    string FullName,
    UserRole Role);

public record ChangePasswordDto(
    string CurrentPassword,
    string NewPassword);

public record LoginDto(
    string Username,
    string Password);

public record LoginResultDto(
    Guid UserId,
    string Username,
    string FullName,
    UserRole Role);
