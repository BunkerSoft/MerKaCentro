using System.Security.Cryptography;

namespace MerkaCentro.Domain.Security;

/// <summary>
/// Servicio de hash de contraseñas usando PBKDF2 con SHA256.
/// Consolidado en un solo lugar para evitar inconsistencias entre DataSeeder y AuthService.
/// </summary>
public static class PasswordHasher
{
    private const int _saltSize = 16;      // 128 bits
    private const int _hashSize = 32;      // 256 bits
    private const int _iterations = 100000; // NIST 2023 recommendation: 120,000+

    /// <summary>
    /// Genera un hash seguro para una contraseña usando PBKDF2-SHA256.
    /// </summary>
    public static string Hash(string password)
    {
        if (string.IsNullOrWhiteSpace(password))
        {
            throw new ArgumentException("La contraseña no puede estar vacía", nameof(password));
        }

        var salt = RandomNumberGenerator.GetBytes(_saltSize);
        var hash = Rfc2898DeriveBytes.Pbkdf2(
            password,
            salt,
            _iterations,
            HashAlgorithmName.SHA256,
            _hashSize);

        var result = new byte[_saltSize + _hashSize];
        Buffer.BlockCopy(salt, 0, result, 0, _saltSize);
        Buffer.BlockCopy(hash, 0, result, _saltSize, _hashSize);

        return Convert.ToBase64String(result);
    }

    /// <summary>
    /// Verifica si una contraseña coincide con su hash almacenado.
    /// Utiliza comparación de tiempo constante para prevenir ataques de temporización.
    /// </summary>
    public static bool Verify(string password, string storedHash)
    {
        if (string.IsNullOrWhiteSpace(password))
        {
            return false;
        }

        try
        {
            var hashBytes = Convert.FromBase64String(storedHash);
            if (hashBytes.Length != _saltSize + _hashSize)
            {
                return false;
            }

            var salt = new byte[_saltSize];
            Buffer.BlockCopy(hashBytes, 0, salt, 0, _saltSize);

            var storedHashPart = new byte[_hashSize];
            Buffer.BlockCopy(hashBytes, _saltSize, storedHashPart, 0, _hashSize);

            var computedHash = Rfc2898DeriveBytes.Pbkdf2(
                password,
                salt,
                _iterations,
                HashAlgorithmName.SHA256,
                _hashSize);

            return CryptographicOperations.FixedTimeEquals(computedHash, storedHashPart);
        }
        catch
        {
            return false;
        }
    }
}
