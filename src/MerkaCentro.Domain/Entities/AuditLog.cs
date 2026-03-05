using MerkaCentro.Domain.Common;

namespace MerkaCentro.Domain.Entities;

public class AuditLog : AggregateRoot<Guid>
{
    public string EntityType { get; private set; } = string.Empty;
    public Guid EntityId { get; private set; }
    public string Action { get; private set; } = string.Empty;
    public string? OldValues { get; private set; }
    public string? NewValues { get; private set; }
    public Guid? UserId { get; private set; }
    public User? User { get; private set; }
    public string? UserName { get; private set; }
    public string? IpAddress { get; private set; }
    public string? UserAgent { get; private set; }

    private AuditLog() : base() { }

    public static AuditLog Create(
        string entityType,
        Guid entityId,
        string action,
        string? oldValues,
        string? newValues,
        Guid? userId,
        string? userName,
        string? ipAddress = null,
        string? userAgent = null)
    {
        return new AuditLog
        {
            Id = Guid.NewGuid(),
            EntityType = entityType,
            EntityId = entityId,
            Action = action,
            OldValues = oldValues,
            NewValues = newValues,
            UserId = userId,
            UserName = userName,
            IpAddress = ipAddress,
            UserAgent = userAgent
        };
    }
}
