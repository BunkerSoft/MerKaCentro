using MiniMarket.Application.Common;

namespace MiniMarket.Application.Services;

public interface IIdempotencyService
{
    Task<IdempotencyResult<T>> ExecuteAsync<T>(
        Guid idempotencyKey,
        string operationType,
        Func<Task<T>> operation);

    Task<bool> ExistsAsync(Guid idempotencyKey);
    Task<T?> GetStoredResultAsync<T>(Guid idempotencyKey) where T : class;
    Task CleanExpiredAsync();
}

public class IdempotencyResult<T>
{
    public bool IsNew { get; init; }
    public bool IsSuccess { get; init; }
    public T? Value { get; init; }
    public string? Error { get; init; }

    public static IdempotencyResult<T> NewSuccess(T value) => new()
    {
        IsNew = true,
        IsSuccess = true,
        Value = value
    };

    public static IdempotencyResult<T> CachedSuccess(T value) => new()
    {
        IsNew = false,
        IsSuccess = true,
        Value = value
    };

    public static IdempotencyResult<T> NewFailure(string error) => new()
    {
        IsNew = true,
        IsSuccess = false,
        Error = error
    };

    public static IdempotencyResult<T> CachedFailure(string error) => new()
    {
        IsNew = false,
        IsSuccess = false,
        Error = error
    };
}
