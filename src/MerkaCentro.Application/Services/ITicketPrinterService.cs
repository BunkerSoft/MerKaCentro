using MerkaCentro.Application.Common;
using MerkaCentro.Application.DTOs;

namespace MerkaCentro.Application.Services;

public interface ITicketPrinterService
{
    Task<Result> PrintSaleTicketAsync(SaleDto sale);
    Task<Result> PrintCashClosingAsync(CashRegisterDto cashRegister);
    Task<Result> PrintReportAsync(string title, IEnumerable<string> lines);
    Task<Result> OpenCashDrawerAsync();
    Task<Result<bool>> IsAvailableAsync();
}

public class TicketPrinterService : ITicketPrinterService
{
    public Task<Result> PrintSaleTicketAsync(SaleDto sale)
    {
        // TODO: Implement ESC/POS printing
        // For now, just return success (would print to console in development)
        Console.WriteLine("=================================");
        Console.WriteLine("        MINIMARKET POS          ");
        Console.WriteLine("=================================");
        Console.WriteLine($"Ticket: {sale.Number}");
        Console.WriteLine($"Fecha: {sale.CreatedAt:dd/MM/yyyy HH:mm}");
        Console.WriteLine("---------------------------------");

        foreach (var item in sale.Items)
        {
            Console.WriteLine($"{item.ProductName}");
            Console.WriteLine($"  {item.Quantity} x {item.UnitPrice:N2} = {item.Total:N2}");
        }

        Console.WriteLine("---------------------------------");
        Console.WriteLine($"Subtotal: $ {sale.Subtotal:N2}");
        if (sale.Discount > 0)
        {
            Console.WriteLine($"Descuento: $ {sale.Discount:N2}");
        }
        Console.WriteLine($"TOTAL: $ {sale.Total:N2}");
        Console.WriteLine("---------------------------------");
        Console.WriteLine($"Pagado: $ {sale.AmountPaid:N2}");
        Console.WriteLine($"Cambio: $ {sale.Change:N2}");
        Console.WriteLine("=================================");
        Console.WriteLine("     Gracias por su compra!     ");
        Console.WriteLine("=================================");

        return Task.FromResult(Result.Success());
    }

    public Task<Result> PrintCashClosingAsync(CashRegisterDto cashRegister)
    {
        Console.WriteLine("=================================");
        Console.WriteLine("       CIERRE DE CAJA           ");
        Console.WriteLine("=================================");
        Console.WriteLine($"Usuario: {cashRegister.UserName}");
        Console.WriteLine($"Apertura: {cashRegister.OpenedAt:dd/MM/yyyy HH:mm}");
        Console.WriteLine($"Cierre: {cashRegister.ClosedAt:dd/MM/yyyy HH:mm}");
        Console.WriteLine("---------------------------------");
        Console.WriteLine($"Inicial: $ {cashRegister.InitialCash:N2}");
        Console.WriteLine($"Ventas: $ {cashRegister.Summary.TotalSales:N2}");
        Console.WriteLine($"Gastos: $ {cashRegister.Summary.TotalExpenses:N2}");
        Console.WriteLine($"Retiros: $ {cashRegister.Summary.TotalWithdrawals:N2}");
        Console.WriteLine($"Depositos: $ {cashRegister.Summary.TotalDeposits:N2}");
        Console.WriteLine("---------------------------------");
        Console.WriteLine($"Esperado: $ {cashRegister.ExpectedCash:N2}");
        Console.WriteLine($"Contado: $ {cashRegister.FinalCash:N2}");
        Console.WriteLine($"Diferencia: $ {cashRegister.Difference:N2}");
        Console.WriteLine("=================================");

        return Task.FromResult(Result.Success());
    }

    public Task<Result> PrintReportAsync(string title, IEnumerable<string> lines)
    {
        Console.WriteLine("=================================");
        Console.WriteLine($"  {title.ToUpperInvariant()}");
        Console.WriteLine("=================================");

        foreach (var line in lines)
        {
            Console.WriteLine(line);
        }

        Console.WriteLine("=================================");

        return Task.FromResult(Result.Success());
    }

    public Task<Result> OpenCashDrawerAsync()
    {
        // TODO: Send ESC/POS command to open drawer
        // ESC p 0 50 200 (typical command)
        Console.WriteLine("[CAJA ABIERTA]");
        return Task.FromResult(Result.Success());
    }

    public Task<Result<bool>> IsAvailableAsync()
    {
        // TODO: Check if printer is connected and available
        return Task.FromResult(Result<bool>.Success(true));
    }
}
