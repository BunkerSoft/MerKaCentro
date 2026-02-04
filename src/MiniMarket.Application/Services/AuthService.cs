using System.Security.Cryptography;
using MiniMarket.Application.Common;
using MiniMarket.Domain.Entities;
using MiniMarket.Domain.Enums;
using MiniMarket.Domain.Ports.Output;

namespace MiniMarket.Application.Services;

public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly IUnitOfWork _unitOfWork;
    private const int SaltSize = 16;
    private const int HashSize = 32;
    private const int Iterations = 100000;

    public AuthService(IUserRepository userRepository, IUnitOfWork unitOfWork)
    {
        _userRepository = userRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<AuthResult>> LoginAsync(string username, string password)
    {
        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
        {
            return Result<AuthResult>.Failure("Usuario y contraseña son requeridos");
        }

        var user = await _userRepository.GetByUsernameAsync(username.Trim().ToLowerInvariant());

        if (user == null)
        {
            return Result<AuthResult>.Failure("Usuario o contraseña incorrectos");
        }

        if (!user.IsActive)
        {
            return Result<AuthResult>.Failure("Usuario desactivado");
        }

        if (!VerifyPassword(password, user.PasswordHash))
        {
            return Result<AuthResult>.Failure("Usuario o contraseña incorrectos");
        }

        user.RecordLogin();
        await _unitOfWork.SaveChangesAsync();

        var result = new AuthResult(
            user.Id,
            user.Username,
            user.FullName,
            user.Role);

        return Result<AuthResult>.Success(result);
    }

    public async Task<Result<UserDto>> GetUserByIdAsync(Guid userId)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null)
        {
            return Result<UserDto>.Failure("Usuario no encontrado");
        }

        return Result<UserDto>.Success(MapToDto(user));
    }

    public async Task<Result<UserDto>> CreateUserAsync(CreateUserRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Username))
        {
            return Result<UserDto>.Failure("El nombre de usuario es requerido");
        }

        if (string.IsNullOrWhiteSpace(request.Password) || request.Password.Length < 6)
        {
            return Result<UserDto>.Failure("La contraseña debe tener al menos 6 caracteres");
        }

        var existingUser = await _userRepository.GetByUsernameAsync(request.Username.Trim().ToLowerInvariant());
        if (existingUser != null)
        {
            return Result<UserDto>.Failure("El nombre de usuario ya existe");
        }

        var passwordHash = HashPassword(request.Password);
        var user = User.Create(
            request.Username,
            passwordHash,
            request.FullName,
            request.Role);

        await _userRepository.AddAsync(user);
        await _unitOfWork.SaveChangesAsync();

        return Result<UserDto>.Success(MapToDto(user));
    }

    public async Task<Result> ChangePasswordAsync(Guid userId, string currentPassword, string newPassword)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null)
        {
            return Result.Failure("Usuario no encontrado");
        }

        if (!VerifyPassword(currentPassword, user.PasswordHash))
        {
            return Result.Failure("Contraseña actual incorrecta");
        }

        if (string.IsNullOrWhiteSpace(newPassword) || newPassword.Length < 6)
        {
            return Result.Failure("La nueva contraseña debe tener al menos 6 caracteres");
        }

        var newHash = HashPassword(newPassword);
        user.UpdatePassword(newHash);
        await _unitOfWork.SaveChangesAsync();

        return Result.Success();
    }

    public async Task<Result> UpdateUserAsync(Guid userId, UpdateUserRequest request)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null)
        {
            return Result.Failure("Usuario no encontrado");
        }

        if (!string.IsNullOrWhiteSpace(request.FullName))
        {
            user.UpdateProfile(request.FullName);
        }

        if (request.Role.HasValue)
        {
            user.ChangeRole(request.Role.Value);
        }

        await _unitOfWork.SaveChangesAsync();
        return Result.Success();
    }

    public async Task<Result> DeactivateUserAsync(Guid userId)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null)
        {
            return Result.Failure("Usuario no encontrado");
        }

        user.Deactivate();
        await _unitOfWork.SaveChangesAsync();
        return Result.Success();
    }

    public async Task<Result> ActivateUserAsync(Guid userId)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null)
        {
            return Result.Failure("Usuario no encontrado");
        }

        user.Activate();
        await _unitOfWork.SaveChangesAsync();
        return Result.Success();
    }

    public async Task<Result<IReadOnlyList<UserDto>>> GetAllUsersAsync()
    {
        var users = await _userRepository.GetAllAsync();
        var dtos = users.Select(MapToDto).ToList();
        return Result<IReadOnlyList<UserDto>>.Success(dtos);
    }

    public string HashPassword(string password)
    {
        var salt = RandomNumberGenerator.GetBytes(SaltSize);
        var hash = Rfc2898DeriveBytes.Pbkdf2(
            password,
            salt,
            Iterations,
            HashAlgorithmName.SHA256,
            HashSize);

        var result = new byte[SaltSize + HashSize];
        Buffer.BlockCopy(salt, 0, result, 0, SaltSize);
        Buffer.BlockCopy(hash, 0, result, SaltSize, HashSize);

        return Convert.ToBase64String(result);
    }

    public bool VerifyPassword(string password, string storedHash)
    {
        try
        {
            var hashBytes = Convert.FromBase64String(storedHash);
            if (hashBytes.Length != SaltSize + HashSize)
            {
                return false;
            }

            var salt = new byte[SaltSize];
            Buffer.BlockCopy(hashBytes, 0, salt, 0, SaltSize);

            var storedHashPart = new byte[HashSize];
            Buffer.BlockCopy(hashBytes, SaltSize, storedHashPart, 0, HashSize);

            var computedHash = Rfc2898DeriveBytes.Pbkdf2(
                password,
                salt,
                Iterations,
                HashAlgorithmName.SHA256,
                HashSize);

            return CryptographicOperations.FixedTimeEquals(computedHash, storedHashPart);
        }
        catch
        {
            return false;
        }
    }

    private static UserDto MapToDto(User user) => new(
        user.Id,
        user.Username,
        user.FullName,
        user.Role,
        user.IsActive,
        user.LastLoginAt,
        user.CreatedAt);
}
