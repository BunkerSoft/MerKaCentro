using MiniMarket.Application.Common;

namespace MiniMarket.Application.Services;

public interface IBarcodeService
{
    Task<Result<byte[]>> GenerateBarcodeAsync(string code, BarcodeFormat format = BarcodeFormat.Code128);
    Task<Result<byte[]>> GenerateQrCodeAsync(string content);
    Task<Result<string>> GenerateUniqueCodeAsync(string prefix = "");
    bool ValidateBarcode(string code, BarcodeFormat format);
}

public enum BarcodeFormat
{
    Code128,
    Code39,
    Ean13,
    Ean8,
    Upca,
    Upce,
    QrCode
}
