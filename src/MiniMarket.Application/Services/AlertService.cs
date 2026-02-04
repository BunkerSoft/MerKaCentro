using MiniMarket.Application.Common;
using MiniMarket.Domain.Entities;
using MiniMarket.Domain.Enums;
using MiniMarket.Domain.Ports.Output;

namespace MiniMarket.Application.Services;

public class AlertService : IAlertService
{
    private readonly IAlertRepository _alertRepository;
    private readonly IProductRepository _productRepository;
    private readonly ICustomerRepository _customerRepository;
    private readonly IUnitOfWork _unitOfWork;

    public AlertService(
        IAlertRepository alertRepository,
        IProductRepository productRepository,
        ICustomerRepository customerRepository,
        IUnitOfWork unitOfWork)
    {
        _alertRepository = alertRepository;
        _productRepository = productRepository;
        _customerRepository = customerRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<IReadOnlyList<AlertDto>>> GetActiveAsync()
    {
        var alerts = await _alertRepository.GetActiveAsync();
        var dtos = alerts.Select(MapToDto).ToList();
        return Result<IReadOnlyList<AlertDto>>.Success(dtos);
    }

    public async Task<Result<IReadOnlyList<AlertDto>>> GetByTypeAsync(AlertType type)
    {
        var alerts = await _alertRepository.GetByTypeAsync(type);
        var dtos = alerts.Select(MapToDto).ToList();
        return Result<IReadOnlyList<AlertDto>>.Success(dtos);
    }

    public async Task<Result<int>> GetActiveCountAsync()
    {
        var count = await _alertRepository.GetActiveCountAsync();
        return Result<int>.Success(count);
    }

    public async Task<Result> AcknowledgeAsync(Guid alertId, Guid userId)
    {
        var alert = await _alertRepository.GetByIdAsync(alertId);
        if (alert == null)
            return Result.Failure("Alerta no encontrada");

        alert.Acknowledge(userId);
        await _unitOfWork.SaveChangesAsync();
        return Result.Success();
    }

    public async Task<Result> DismissAsync(Guid alertId, Guid userId)
    {
        var alert = await _alertRepository.GetByIdAsync(alertId);
        if (alert == null)
            return Result.Failure("Alerta no encontrada");

        alert.Dismiss(userId);
        await _unitOfWork.SaveChangesAsync();
        return Result.Success();
    }

    public async Task<Result> CheckLowStockAsync()
    {
        var products = await _productRepository.GetLowStockAsync();

        foreach (var product in products)
        {
            var existingAlert = await _alertRepository.GetByEntityAsync(
                nameof(Product), product.Id, AlertType.LowStock);

            if (existingAlert == null)
            {
                var severity = product.CurrentStock <= 0
                    ? AlertSeverity.Critical
                    : AlertSeverity.Warning;

                var alert = Alert.Create(
                    AlertType.LowStock,
                    severity,
                    $"Stock bajo: {product.Name}",
                    $"El producto '{product.Name}' tiene stock bajo ({product.CurrentStock} {product.Unit.Abbreviation}). Stock minimo: {product.MinStock}",
                    nameof(Product),
                    product.Id);

                await _alertRepository.AddAsync(alert);
            }
        }

        await _unitOfWork.SaveChangesAsync();
        return Result.Success();
    }

    public async Task<Result> CheckExpiringProductsAsync(int daysAhead = 30)
    {
        var expirationDate = DateTime.Today.AddDays(daysAhead);
        var products = await _productRepository.GetExpiringBeforeAsync(expirationDate);

        foreach (var product in products)
        {
            var existingAlert = await _alertRepository.GetByEntityAsync(
                nameof(Product), product.Id, AlertType.ExpiringProduct);

            if (existingAlert == null && product.ExpirationDate.HasValue)
            {
                var daysUntilExpiration = (product.ExpirationDate.Value - DateTime.Today).Days;
                var severity = daysUntilExpiration <= 7
                    ? AlertSeverity.Critical
                    : (daysUntilExpiration <= 15 ? AlertSeverity.Warning : AlertSeverity.Info);

                var alert = Alert.Create(
                    AlertType.ExpiringProduct,
                    severity,
                    $"Producto por vencer: {product.Name}",
                    $"El producto '{product.Name}' vence el {product.ExpirationDate.Value:dd/MM/yyyy} ({daysUntilExpiration} dias)",
                    nameof(Product),
                    product.Id);

                await _alertRepository.AddAsync(alert);
            }
        }

        await _unitOfWork.SaveChangesAsync();
        return Result.Success();
    }

    public async Task<Result> CheckCustomerDebtsAsync(decimal threshold = 100)
    {
        var customers = await _customerRepository.GetWithDebtAsync();

        foreach (var customer in customers.Where(c => c.CreditBalance >= threshold))
        {
            var existingAlert = await _alertRepository.GetByEntityAsync(
                nameof(Customer), customer.Id, AlertType.CustomerDebt);

            if (existingAlert == null)
            {
                var severity = customer.CreditBalance >= customer.CreditLimit
                    ? AlertSeverity.Critical
                    : (customer.CreditBalance >= customer.CreditLimit * 0.8m ? AlertSeverity.Warning : AlertSeverity.Info);

                var alert = Alert.Create(
                    AlertType.CustomerDebt,
                    severity,
                    $"Deuda de cliente: {customer.Name}",
                    $"El cliente '{customer.Name}' tiene una deuda de S/ {customer.CreditBalance:N2}. Limite: S/ {customer.CreditLimit:N2}",
                    nameof(Customer),
                    customer.Id);

                await _alertRepository.AddAsync(alert);
            }
        }

        await _unitOfWork.SaveChangesAsync();
        return Result.Success();
    }

    public async Task<Result> RunAllChecksAsync()
    {
        await CheckLowStockAsync();
        await CheckExpiringProductsAsync();
        await CheckCustomerDebtsAsync();
        return Result.Success();
    }

    private static AlertDto MapToDto(Alert alert) => new(
        alert.Id,
        alert.Type,
        alert.Severity,
        alert.Status,
        alert.Title,
        alert.Message,
        alert.EntityType,
        alert.EntityId,
        alert.CreatedAt,
        alert.AcknowledgedAt,
        alert.AcknowledgedByUser?.Name);
}
