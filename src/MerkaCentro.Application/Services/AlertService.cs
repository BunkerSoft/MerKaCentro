using MerkaCentro.Application.Common;
using MerkaCentro.Domain.Entities;
using MerkaCentro.Domain.Enums;
using MerkaCentro.Domain.Ports.Output;

namespace MerkaCentro.Application.Services;

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
                    $"El producto '{product.Name}' tiene stock bajo ({product.CurrentStock} {product.Unit}). Stock minimo: {product.MinStock}",
                    nameof(Product),
                    product.Id);

                await _alertRepository.AddAsync(alert);
            }
        }

        await _unitOfWork.SaveChangesAsync();
        return Result.Success();
    }

    public Task<Result> CheckExpiringProductsAsync(int daysAhead = 30)
    {
        // Note: GetExpiringBeforeAsync not implemented in repository
        // This feature would require adding the method to IProductRepository
        return Task.FromResult(Result.Success());
    }

    public async Task<Result> CheckCustomerDebtsAsync(decimal threshold = 100)
    {
        var customers = await _customerRepository.GetWithDebtAsync();

        foreach (var customer in customers.Where(c => c.CurrentDebt.Amount >= threshold))
        {
            var existingAlert = await _alertRepository.GetByEntityAsync(
                nameof(Customer), customer.Id, AlertType.CustomerDebt);

            if (existingAlert == null)
            {
                var debtAmount = customer.CurrentDebt.Amount;
                var limitAmount = customer.CreditLimit.Amount;
                var severity = debtAmount >= limitAmount
                    ? AlertSeverity.Critical
                    : (debtAmount >= limitAmount * 0.8m ? AlertSeverity.Warning : AlertSeverity.Info);

                var alert = Alert.Create(
                    AlertType.CustomerDebt,
                    severity,
                    $"Deuda de cliente: {customer.Name}",
                    $"El cliente '{customer.Name}' tiene una deuda de $ {debtAmount:N2}. Limite: $ {limitAmount:N2}",
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
        alert.AcknowledgedByUser?.FullName);
}
