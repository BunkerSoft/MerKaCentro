using MiniMarket.Application.Common;
using MiniMarket.Domain.Enums;

namespace MiniMarket.Application.Services;

public interface IAuthService
{
    Task<Result<AuthResult>> LoginAsync(string username, string password);
    Task<Result<UserDto>> GetUserByIdAsync(Guid userId);
    Task<Result<UserDto>> CreateUserAsync(CreateUserRequest request);
    Task<Result> ChangePasswordAsync(Guid userId, string currentPassword, string newPassword);
    Task<Result> UpdateUserAsync(Guid userId, UpdateUserRequest request);
    Task<Result> DeactivateUserAsync(Guid userId);
    Task<Result> ActivateUserAsync(Guid userId);
    Task<Result<IReadOnlyList<UserDto>>> GetAllUsersAsync();
    string HashPassword(string password);
    bool VerifyPassword(string password, string hash);
}

public record AuthResult(
    Guid UserId,
    string Username,
    string FullName,
    UserRole Role);

public record UserDto(
    Guid Id,
    string Username,
    string FullName,
    UserRole Role,
    bool IsActive,
    DateTime? LastLoginAt,
    DateTime CreatedAt);

public record CreateUserRequest(
    string Username,
    string Password,
    string FullName,
    UserRole Role);

public record UpdateUserRequest(
    string FullName,
    UserRole? Role);
