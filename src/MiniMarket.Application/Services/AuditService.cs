using System.Text.Json;
using MiniMarket.Application.Common;
using MiniMarket.Domain.Entities;
using MiniMarket.Domain.Ports.Output;

namespace MiniMarket.Application.Services;

public class AuditService : IAuditService
{
    private readonly IAuditLogRepository _auditLogRepository;
    private readonly IUnitOfWork _unitOfWork;
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = false,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public AuditService(
        IAuditLogRepository auditLogRepository,
        IUnitOfWork unitOfWork)
    {
        _auditLogRepository = auditLogRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> LogAsync(
        string entityType,
        Guid entityId,
        string action,
        object? oldValues,
        object? newValues,
        Guid? userId,
        string? userName,
        string? ipAddress = null,
        string? userAgent = null)
    {
        var oldValuesJson = oldValues != null
            ? JsonSerializer.Serialize(oldValues, JsonOptions)
            : null;

        var newValuesJson = newValues != null
            ? JsonSerializer.Serialize(newValues, JsonOptions)
            : null;

        var auditLog = AuditLog.Create(
            entityType,
            entityId,
            action,
            oldValuesJson,
            newValuesJson,
            userId,
            userName,
            ipAddress,
            userAgent);

        await _auditLogRepository.AddAsync(auditLog);
        await _unitOfWork.SaveChangesAsync();

        return Result.Success();
    }

    public async Task<Result<IReadOnlyList<AuditLogDto>>> GetByEntityAsync(string entityType, Guid entityId)
    {
        var logs = await _auditLogRepository.GetByEntityAsync(entityType, entityId);
        var dtos = logs.Select(MapToDto).ToList();
        return Result<IReadOnlyList<AuditLogDto>>.Success(dtos);
    }

    public async Task<Result<IReadOnlyList<AuditLogDto>>> GetByUserAsync(Guid userId, int count = 50)
    {
        var logs = await _auditLogRepository.GetByUserAsync(userId, count);
        var dtos = logs.Select(MapToDto).ToList();
        return Result<IReadOnlyList<AuditLogDto>>.Success(dtos);
    }

    public async Task<Result<IReadOnlyList<AuditLogDto>>> GetRecentAsync(int count = 100)
    {
        var logs = await _auditLogRepository.GetRecentAsync(count);
        var dtos = logs.Select(MapToDto).ToList();
        return Result<IReadOnlyList<AuditLogDto>>.Success(dtos);
    }

    public async Task<Result<PagedResult<AuditLogDto>>> GetPagedAsync(
        int pageNumber,
        int pageSize,
        string? entityType = null)
    {
        var allLogs = await _auditLogRepository.GetRecentAsync(1000);

        var query = allLogs.AsEnumerable();
        if (!string.IsNullOrEmpty(entityType))
        {
            query = query.Where(l => l.EntityType == entityType);
        }

        var totalCount = query.Count();
        var items = query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(MapToDto)
            .ToList();

        var pagedResult = new PagedResult<AuditLogDto>(items, totalCount, pageNumber, pageSize);
        return Result<PagedResult<AuditLogDto>>.Success(pagedResult);
    }

    public async Task<Result> CleanupOldLogsAsync(int daysToKeep = 90)
    {
        var cutoffDate = DateTime.UtcNow.AddDays(-daysToKeep);
        await _auditLogRepository.DeleteOlderThanAsync(cutoffDate);
        await _unitOfWork.SaveChangesAsync();
        return Result.Success();
    }

    private static AuditLogDto MapToDto(AuditLog log) => new(
        log.Id,
        log.EntityType,
        log.EntityId,
        log.Action,
        log.OldValues,
        log.NewValues,
        log.UserId,
        log.UserName,
        log.IpAddress,
        log.CreatedAt);
}
