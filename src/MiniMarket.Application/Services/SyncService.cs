using System.Text.Json;
using MiniMarket.Application.Common;
using MiniMarket.Domain.Entities;
using MiniMarket.Domain.Enums;
using MiniMarket.Domain.Ports.Output;

namespace MiniMarket.Application.Services;

public class SyncService : ISyncService
{
    private readonly ISyncQueueRepository _syncQueueRepository;
    private readonly IUnitOfWork _unitOfWork;
    private bool _isOnline = true;
    private DateTime? _lastSyncAt;

    public SyncService(
        ISyncQueueRepository syncQueueRepository,
        IUnitOfWork unitOfWork)
    {
        _syncQueueRepository = syncQueueRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> EnqueueAsync<T>(T entity, string operation) where T : class
    {
        try
        {
            var entityType = typeof(T).Name;
            var entityId = GetEntityId(entity);
            var syncOperation = ParseOperation(operation);
            var payload = JsonSerializer.Serialize(entity);

            var queueItem = SyncQueueItem.Create(entityType, entityId, syncOperation, payload);

            await _syncQueueRepository.AddAsync(queueItem);
            await _unitOfWork.SaveChangesAsync();

            return Result.Success();
        }
        catch (Exception ex)
        {
            return Result.Failure($"Error al encolar: {ex.Message}");
        }
    }

    public async Task<Result<SyncStatusDto>> GetStatusAsync()
    {
        var pendingCount = await _syncQueueRepository.GetPendingCountAsync();
        var failed = await _syncQueueRepository.GetFailedAsync();

        var status = new SyncStatusDto(
            pendingCount,
            failed.Count,
            _isOnline,
            _lastSyncAt);

        return Result<SyncStatusDto>.Success(status);
    }

    public async Task<Result<int>> ProcessPendingAsync(int batchSize = 100)
    {
        if (!_isOnline)
        {
            return Result<int>.Failure("Sin conexion al servidor");
        }

        try
        {
            var pending = await _syncQueueRepository.GetPendingAsync(batchSize);
            var processedCount = 0;

            foreach (var item in pending)
            {
                try
                {
                    // Here you would call the actual sync API
                    // For now, we'll just mark as synced
                    await SyncItemToServerAsync(item);
                    item.MarkAsSynced();
                    processedCount++;
                }
                catch (Exception ex)
                {
                    item.MarkAsFailed(ex.Message);
                }

                _syncQueueRepository.Update(item);
            }

            await _unitOfWork.SaveChangesAsync();
            _lastSyncAt = DateTime.UtcNow;

            return Result<int>.Success(processedCount);
        }
        catch (Exception ex)
        {
            _isOnline = false;
            return Result<int>.Failure($"Error de sincronizacion: {ex.Message}");
        }
    }

    public async Task<Result> RetryFailedAsync()
    {
        try
        {
            var failed = await _syncQueueRepository.GetFailedAsync();

            foreach (var item in failed)
            {
                if (item.CanRetry())
                {
                    item.ResetForRetry();
                    _syncQueueRepository.Update(item);
                }
            }

            await _unitOfWork.SaveChangesAsync();

            return Result.Success();
        }
        catch (Exception ex)
        {
            return Result.Failure($"Error al reintentar: {ex.Message}");
        }
    }

    public async Task<Result> CleanupSyncedAsync(int daysOld = 7)
    {
        try
        {
            var cutoffDate = DateTime.UtcNow.AddDays(-daysOld);
            await _syncQueueRepository.DeleteSyncedAsync(cutoffDate);
            return Result.Success();
        }
        catch (Exception ex)
        {
            return Result.Failure($"Error al limpiar: {ex.Message}");
        }
    }

    private static Guid GetEntityId<T>(T entity) where T : class
    {
        var idProperty = typeof(T).GetProperty("Id");
        if (idProperty == null)
        {
            throw new InvalidOperationException($"Entity {typeof(T).Name} does not have an Id property");
        }

        var value = idProperty.GetValue(entity);
        return value is Guid guid ? guid : Guid.NewGuid();
    }

    private static SyncOperation ParseOperation(string operation)
    {
        return operation.ToLowerInvariant() switch
        {
            "insert" or "create" or "add" => SyncOperation.Insert,
            "update" or "edit" => SyncOperation.Update,
            "delete" or "remove" => SyncOperation.Delete,
            _ => SyncOperation.Insert
        };
    }

    private Task SyncItemToServerAsync(SyncQueueItem item)
    {
        // TODO: Implement actual HTTP call to sync server
        // This is a placeholder for the actual sync logic
        // Example:
        // var response = await _httpClient.PostAsync($"/api/sync/{item.EntityType}",
        //     new StringContent(item.Payload, Encoding.UTF8, "application/json"));
        // response.EnsureSuccessStatusCode();

        return Task.CompletedTask;
    }

    public void SetOnlineStatus(bool isOnline)
    {
        _isOnline = isOnline;
    }
}
