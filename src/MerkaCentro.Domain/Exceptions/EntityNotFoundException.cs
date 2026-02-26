namespace MerkaCentro.Domain.Exceptions;

public class EntityNotFoundException : DomainException
{
    public string EntityName { get; }
    public object? EntityId { get; }

    public EntityNotFoundException() : base()
    {
        EntityName = string.Empty;
        EntityId = null;
    }

    public EntityNotFoundException(string message) : base(message)
    {
        EntityName = string.Empty;
        EntityId = null;
    }

    public EntityNotFoundException(string message, Exception innerException) : base(message, innerException)
    {
        EntityName = string.Empty;
        EntityId = null;
    }

    public EntityNotFoundException(string entityName, object? entityId = null)
        : base($"{entityName} no encontrado" + (entityId is not null ? $" con ID: {entityId}" : string.Empty))
    {
        EntityName = entityName;
        EntityId = entityId;
    }
}
