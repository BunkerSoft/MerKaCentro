using MiniMarket.Application.Common;

namespace MiniMarket.Application.Services;

public class BarcodeService : IBarcodeService
{
    private static readonly Random _random = new();

    public Task<Result<byte[]>> GenerateBarcodeAsync(string code, BarcodeFormat format = BarcodeFormat.Code128)
    {
        try
        {
            // TODO: Implement using ZXing.Net or similar library
            // For now, return a placeholder
            // Example with ZXing:
            // var writer = new BarcodeWriterPixelData
            // {
            //     Format = MapFormat(format),
            //     Options = new EncodingOptions { Width = 300, Height = 100 }
            // };
            // var pixelData = writer.Write(code);
            // return Result<byte[]>.Success(pixelData.Pixels);

            // Placeholder - returns empty array
            return Task.FromResult(Result<byte[]>.Success(Array.Empty<byte>()));
        }
        catch (Exception ex)
        {
            return Task.FromResult(Result<byte[]>.Failure($"Error generando codigo de barras: {ex.Message}"));
        }
    }

    public Task<Result<byte[]>> GenerateQrCodeAsync(string content)
    {
        return GenerateBarcodeAsync(content, BarcodeFormat.QrCode);
    }

    public Task<Result<string>> GenerateUniqueCodeAsync(string prefix = "")
    {
        var timestamp = DateTime.UtcNow.ToString("yyMMddHHmmss");
        var random = _random.Next(1000, 9999);
        var code = string.IsNullOrEmpty(prefix)
            ? $"{timestamp}{random}"
            : $"{prefix}{timestamp}{random}";

        return Task.FromResult(Result<string>.Success(code));
    }

    public bool ValidateBarcode(string code, BarcodeFormat format)
    {
        if (string.IsNullOrWhiteSpace(code))
            return false;

        return format switch
        {
            BarcodeFormat.Ean13 => ValidateEan13(code),
            BarcodeFormat.Ean8 => ValidateEan8(code),
            BarcodeFormat.Upca => ValidateUpca(code),
            BarcodeFormat.Upce => ValidateUpce(code),
            BarcodeFormat.Code128 => code.Length <= 128,
            BarcodeFormat.Code39 => ValidateCode39(code),
            _ => true
        };
    }

    private static bool ValidateEan13(string code)
    {
        if (code.Length != 13 || !code.All(char.IsDigit))
            return false;

        return ValidateCheckDigit(code);
    }

    private static bool ValidateEan8(string code)
    {
        if (code.Length != 8 || !code.All(char.IsDigit))
            return false;

        return ValidateCheckDigit(code);
    }

    private static bool ValidateUpca(string code)
    {
        if (code.Length != 12 || !code.All(char.IsDigit))
            return false;

        return ValidateCheckDigit(code);
    }

    private static bool ValidateUpce(string code)
    {
        if (code.Length != 8 || !code.All(char.IsDigit))
            return false;

        return true; // Simplified validation
    }

    private static bool ValidateCode39(string code)
    {
        const string validChars = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ-. $/+%";
        return code.ToUpperInvariant().All(c => validChars.Contains(c));
    }

    private static bool ValidateCheckDigit(string code)
    {
        var sum = 0;
        var isOdd = true;

        for (var i = code.Length - 2; i >= 0; i--)
        {
            var digit = code[i] - '0';
            sum += isOdd ? digit * 3 : digit;
            isOdd = !isOdd;
        }

        var checkDigit = (10 - (sum % 10)) % 10;
        return checkDigit == (code[^1] - '0');
    }
}
