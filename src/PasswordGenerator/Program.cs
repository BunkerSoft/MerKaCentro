using System;
using System.Security.Cryptography;

if (args.Length == 0)
{
    Console.WriteLine("Usage: dotnet run --project src/PasswordGenerator -- <password>");
    return 1;
}

string password = args[0];
try
{
    string hash = PasswordHasher.Hash(password);
    Console.WriteLine(hash);
    return 0;
}
catch (Exception ex)
{
    Console.Error.WriteLine("Error: " + ex.Message);
    return 2;
}

static class PasswordHasher
{
    private const int SaltSize = 16;
    private const int HashSize = 32;
    private const int Iterations = 100000;

    public static string Hash(string password)
    {
        if (string.IsNullOrWhiteSpace(password))
            throw new ArgumentException("Password cannot be empty", nameof(password));

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
}
