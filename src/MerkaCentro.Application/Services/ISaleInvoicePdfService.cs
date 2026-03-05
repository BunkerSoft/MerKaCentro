using MerkaCentro.Application.DTOs;

namespace MerkaCentro.Application.Services;

public interface ISaleInvoicePdfService
{
    Task<byte[]> GeneratePdfAsync(SaleDto sale);
}
