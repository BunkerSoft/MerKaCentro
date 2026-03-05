using MerkaCentro.Domain.Common;
using MerkaCentro.Domain.Enums;

namespace MerkaCentro.Domain.Entities;

public class Alert : AggregateRoot<Guid>
{
    public AlertType Type { get; private set; }
    public AlertSeverity Severity { get; private set; }
    public AlertStatus Status { get; private set; }
    public string Title { get; private set; } = string.Empty;
    public string Message { get; private set; } = string.Empty;
    public string? EntityType { get; private set; }
    public Guid? EntityId { get; private set; }
    public DateTime? AcknowledgedAt { get; private set; }
    public Guid? AcknowledgedByUserId { get; private set; }
    public User? AcknowledgedByUser { get; private set; }
    public DateTime? ResolvedAt { get; private set; }

    private Alert() : base() { }

    public static Alert Create(
        AlertType type,
        AlertSeverity severity,
        string title,
        string message,
        string? entityType = null,
        Guid? entityId = null)
    {
        var alert = new Alert
        {
            Id = Guid.NewGuid(),
            Type = type,
            Severity = severity,
            Status = AlertStatus.Active,
            Title = title,
            Message = message,
            EntityType = entityType,
            EntityId = entityId
        };
        return alert;
    }

    public void Acknowledge(Guid userId)
    {
        if (Status != AlertStatus.Active)
        {
            return;
        }

        Status = AlertStatus.Acknowledged;
        AcknowledgedAt = DateTime.UtcNow;
        AcknowledgedByUserId = userId;
        SetUpdated();
    }

    public void Resolve()
    {
        if (Status == AlertStatus.Resolved || Status == AlertStatus.Dismissed)
        {
            return;
        }

        Status = AlertStatus.Resolved;
        ResolvedAt = DateTime.UtcNow;
        SetUpdated();
    }

    public void Dismiss(Guid userId)
    {
        if (Status == AlertStatus.Resolved || Status == AlertStatus.Dismissed)
        {
            return;
        }

        Status = AlertStatus.Dismissed;
        AcknowledgedAt = DateTime.UtcNow;
        AcknowledgedByUserId = userId;
        SetUpdated();
    }
}
