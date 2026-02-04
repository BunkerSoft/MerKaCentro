using System.Text.Json;
using MiniMarket.Domain.Entities;
using MiniMarket.Domain.Ports.Output;

namespace MiniMarket.Application.Services;

public class IdempotencyService : IIdempotencyService
{
    private readonly IIdempotencyRepository _idempotencyRepository;
    private readonly IUnitOfWork _unitOfWork;

    public IdempotencyService(
        IIdempotencyRepository idempotencyRepository,
        IUnitOfWork unitOfWork)
    {
        _idempotencyRepository = idempotencyRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<IdempotencyResult<T>> ExecuteAsync<T>(
        Guid idempotencyKey,
        string operationType,
        Func<Task<T>> operation)
    {
        // Check if operation was already processed
        var existingRecord = await _idempotencyRepository.GetByKeyAsync(idempotencyKey);

        if (existingRecord != null && !existingRecord.IsExpired())
        {
            // Return cached result
            if (existingRecord.StatusCode >= 200 && existingRecord.StatusCode < 300)
            {
                var cachedValue = DeserializeResponse<T>(existingRecord.ResponseData);
                return IdempotencyResult<T>.CachedSuccess(cachedValue!);
            }
            else
            {
                return IdempotencyResult<T>.CachedFailure(existingRecord.ResponseData ?? "Error en operacion previa");
            }
        }

        // Execute the operation
        try
        {
            var result = await operation();
            var responseData = SerializeResponse(result);

            var record = IdempotencyRecord.Create(
                idempotencyKey,
                operationType,
                null,
                responseData,
                200);

            if (existingRecord != null)
            {
                existingRecord.UpdateResponse(responseData, 200);
                _idempotencyRepository.Update(existingRecord);
            }
            else
            {
                await _idempotencyRepository.AddAsync(record);
            }

            await _unitOfWork.SaveChangesAsync();

            return IdempotencyResult<T>.NewSuccess(result);
        }
        catch (Exception ex)
        {
            var errorMessage = ex.Message;

            var record = IdempotencyRecord.Create(
                idempotencyKey,
                operationType,
                null,
                errorMessage,
                500);

            if (existingRecord != null)
            {
                existingRecord.UpdateResponse(errorMessage, 500);
                _idempotencyRepository.Update(existingRecord);
            }
            else
            {
                await _idempotencyRepository.AddAsync(record);
            }

            await _unitOfWork.SaveChangesAsync();

            return IdempotencyResult<T>.NewFailure(errorMessage);
        }
    }

    public async Task<bool> ExistsAsync(Guid idempotencyKey)
    {
        return await _idempotencyRepository.ExistsAsync(idempotencyKey);
    }

    public async Task<T?> GetStoredResultAsync<T>(Guid idempotencyKey) where T : class
    {
        var record = await _idempotencyRepository.GetByKeyAsync(idempotencyKey);
        if (record == null || record.IsExpired())
        {
            return null;
        }

        return DeserializeResponse<T>(record.ResponseData);
    }

    public async Task CleanExpiredAsync()
    {
        await _idempotencyRepository.DeleteExpiredAsync();
    }

    private static string? SerializeResponse<T>(T response)
    {
        if (response == null) return null;
        return JsonSerializer.Serialize(response);
    }

    private static T? DeserializeResponse<T>(string? responseData)
    {
        if (string.IsNullOrEmpty(responseData)) return default;
        return JsonSerializer.Deserialize<T>(responseData);
    }
}
