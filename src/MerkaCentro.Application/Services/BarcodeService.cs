using MerkaCentro.Application.Common;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using ZXing;
using ZXing.Common;
using ZXing.QrCode;
using ZXing.Rendering;

namespace MerkaCentro.Application.Services;

public class BarcodeService : IBarcodeService
{
    private static readonly Random _random = new();

    public Task<Result<byte[]>> GenerateBarcodeAsync(string code, BarcodeFormat format = BarcodeFormat.Code128)
    {
        try
        {
            var writer = new BarcodeWriterPixelData
            {
                Format = MapFormat(format),
                Options = new EncodingOptions { Width = 300, Height = 100, Margin = 2 }
            };
            var pixelData = writer.Write(code);
            var bmp = PixelDataToBmp(pixelData.Pixels, pixelData.Width, pixelData.Height);
            return Task.FromResult(Result<byte[]>.Success(bmp));
        }
        catch (Exception ex)
        {
            return Task.FromResult(Result<byte[]>.Failure($"Error generando codigo de barras: {ex.Message}"));
        }
    }

    public Task<Result<byte[]>> GenerateQrCodeAsync(string content)
    {
        try
        {
            var writer = new BarcodeWriterPixelData
            {
                Format = ZXing.BarcodeFormat.QR_CODE,
                Options = new QrCodeEncodingOptions
                {
                    Width = 200,
                    Height = 200,
                    Margin = 1,
                    ErrorCorrection = ZXing.QrCode.Internal.ErrorCorrectionLevel.M
                }
            };
            var pixelData = writer.Write(content);
            var bmp = PixelDataToBmp(pixelData.Pixels, pixelData.Width, pixelData.Height);
            return Task.FromResult(Result<byte[]>.Success(bmp));
        }
        catch (Exception ex)
        {
            return Task.FromResult(Result<byte[]>.Failure($"Error generando QR: {ex.Message}"));
        }
    }

    /// <summary>
    /// Genera la BitMatrix cruda del QR (para uso en impresoras ESC/POS).
    /// </summary>
    public static BitMatrix GenerateQrMatrix(string content, int size = 200)
    {
        var writer = new QRCodeWriter();
        var hints = new Dictionary<EncodeHintType, object>
        {
            { EncodeHintType.ERROR_CORRECTION, ZXing.QrCode.Internal.ErrorCorrectionLevel.M },
            { EncodeHintType.MARGIN, 1 }
        };
        return writer.encode(content, ZXing.BarcodeFormat.QR_CODE, size, size, hints);
    }

    private static ZXing.BarcodeFormat MapFormat(BarcodeFormat format) => format switch
    {
        BarcodeFormat.Code128 => ZXing.BarcodeFormat.CODE_128,
        BarcodeFormat.Code39 => ZXing.BarcodeFormat.CODE_39,
        BarcodeFormat.Ean13 => ZXing.BarcodeFormat.EAN_13,
        BarcodeFormat.Ean8 => ZXing.BarcodeFormat.EAN_8,
        BarcodeFormat.Upca => ZXing.BarcodeFormat.UPC_A,
        BarcodeFormat.Upce => ZXing.BarcodeFormat.UPC_E,
        BarcodeFormat.QrCode => ZXing.BarcodeFormat.QR_CODE,
        _ => ZXing.BarcodeFormat.CODE_128
    };

    /// <summary>
    /// Convierte RGBA pixel data a BMP (compatible con QuestPDF y navegadores).
    /// </summary>
    private static byte[] PixelDataToBmp(byte[] rgbaPixels, int width, int height)
    {
        int rowSize = (width * 3 + 3) & ~3; // rows padded to 4-byte boundary
        int pixelDataSize = rowSize * height;
        int fileSize = 54 + pixelDataSize;
        var bmp = new byte[fileSize];

        // BMP file header (14 bytes)
        bmp[0] = 0x42; bmp[1] = 0x4D; // "BM"
        BitConverter.GetBytes(fileSize).CopyTo(bmp, 2);
        BitConverter.GetBytes(54).CopyTo(bmp, 10); // pixel data offset

        // DIB header (40 bytes)
        BitConverter.GetBytes(40).CopyTo(bmp, 14); // header size
        BitConverter.GetBytes(width).CopyTo(bmp, 18);
        BitConverter.GetBytes(height).CopyTo(bmp, 22);
        BitConverter.GetBytes((short)1).CopyTo(bmp, 26); // planes
        BitConverter.GetBytes((short)24).CopyTo(bmp, 28); // bits per pixel
        BitConverter.GetBytes(pixelDataSize).CopyTo(bmp, 34);

        // Pixel data (BMP is bottom-up, BGR)
        for (int y = 0; y < height; y++)
        {
            int srcRow = (height - 1 - y) * width * 4;
            int dstRow = 54 + y * rowSize;
            for (int x = 0; x < width; x++)
            {
                int srcIdx = srcRow + x * 4;
                int dstIdx = dstRow + x * 3;
                bmp[dstIdx] = rgbaPixels[srcIdx + 2];     // B
                bmp[dstIdx + 1] = rgbaPixels[srcIdx + 1]; // G
                bmp[dstIdx + 2] = rgbaPixels[srcIdx];     // R
            }
        }

        return bmp;
    }

    public Task<Result<string>> DecodeBarcodeAsync(byte[] imageBytes)
    {
        try
        {
            using var image = Image.Load<Rgb24>(imageBytes);
            int width = image.Width;
            int height = image.Height;

            var rgb = new byte[width * height * 3];
            image.ProcessPixelRows(accessor =>
            {
                for (int y = 0; y < height; y++)
                {
                    var row = accessor.GetRowSpan(y);
                    for (int x = 0; x < width; x++)
                    {
                        int idx = (y * width + x) * 3;
                        rgb[idx] = row[x].R;
                        rgb[idx + 1] = row[x].G;
                        rgb[idx + 2] = row[x].B;
                    }
                }
            });

            var luminanceSource = new RGBLuminanceSource(rgb, width, height);

            var reader = new BarcodeReaderGeneric();
            reader.Options.TryHarder = true;
            reader.Options.PossibleFormats = new[]
            {
                ZXing.BarcodeFormat.EAN_13,
                ZXing.BarcodeFormat.EAN_8,
                ZXing.BarcodeFormat.UPC_A,
                ZXing.BarcodeFormat.UPC_E,
                ZXing.BarcodeFormat.CODE_128,
                ZXing.BarcodeFormat.CODE_39,
                ZXing.BarcodeFormat.QR_CODE
            };

            var result = reader.Decode(luminanceSource);

            if (result == null)
                return Task.FromResult(Result<string>.Failure("No se pudo decodificar ningun codigo de barras en la imagen."));

            return Task.FromResult(Result<string>.Success(result.Text));
        }
        catch (Exception ex)
        {
            return Task.FromResult(Result<string>.Failure($"Error decodificando imagen: {ex.Message}"));
        }
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
