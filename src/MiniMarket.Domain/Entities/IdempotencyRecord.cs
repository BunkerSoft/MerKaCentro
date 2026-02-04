using MiniMarket.Domain.Common;

namespace MiniMarket.Domain.Entities;

public class IdempotencyRecord : AggregateRoot<Guid>
{
    public Guid IdempotencyKey { get; private set; }
    public string OperationType { get; private set; } = default!;
    public string? RequestHash { get; private set; }
    public string? ResponseData { get; private set; }
    public int StatusCode { get; private set; }
    public DateTime ProcessedAt { get; private set; }
    public DateTime ExpiresAt { get; private set; }

    private IdempotencyRecord() : base()
    {
    }

    public static IdempotencyRecord Create(
        Guid idempotencyKey,
        string operationType,
        string? requestHash,
        string? responseData,
        int statusCode,
        TimeSpan? expiration = null)
    {
        var expiresIn = expiration ?? TimeSpan.FromHours(24);

        return new IdempotencyRecord
        {
            Id = Guid.NewGuid(),
            IdempotencyKey = idempotencyKey,
            OperationType = operationType,
            RequestHash = requestHash,
            ResponseData = responseData,
            StatusCode = statusCode,
            ProcessedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.Add(expiresIn)
        };
    }

    public void UpdateResponse(string? responseData, int statusCode)
    {
        ResponseData = responseData;
        StatusCode = statusCode;
        SetUpdated();
    }

    public bool IsExpired() => DateTime.UtcNow > ExpiresAt;
}
