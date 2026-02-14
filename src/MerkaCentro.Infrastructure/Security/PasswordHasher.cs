using System.Security.Cryptography;

namespace MerkaCentro.Infrastructure.Security;

/// <summary>
/// Servicio de hash de contraseñas usando PBKDF2 con SHA256.
/// Consolidado en un solo lugar para evitar inconsistencias entre DataSeeder y AuthService.
/// </summary>
public class PasswordHasher
{
    private const int SaltSize = 16;      // 128 bits
    private const int HashSize = 32;      // 256 bits
    private const int Iterations = 100000; // NIST 2023 recommendation: 120,000+

    /// <summary>
    /// Genera un hash seguro para una contraseña usando PBKDF2-SHA256.
    /// </summary>
    public static string Hash(string password)
    {
        if (string.IsNullOrWhiteSpace(password))
            throw new ArgumentException("La contraseña no puede estar vacía", nameof(password));

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

    /// <summary>
    /// Verifica si una contraseña coincide con su hash almacenado.
    /// Utiliza comparación de tiempo constante para prevenir ataques de temporización.
    /// </summary>
    public static bool Verify(string password, string storedHash)
    {
        if (string.IsNullOrWhiteSpace(password))
            return false;

        try
        {
            var hashBytes = Convert.FromBase64String(storedHash);
            if (hashBytes.Length != SaltSize + HashSize)
                return false;

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

            // Comparación de tiempo constante para prevenir timing attacks
            return CryptographicOperations.FixedTimeEquals(computedHash, storedHashPart);
        }
        catch
        {
            return false;
        }
    }
}
