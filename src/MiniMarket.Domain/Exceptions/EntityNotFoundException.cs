namespace MiniMarket.Domain.Exceptions;

public class EntityNotFoundException : DomainException
{
    public string EntityName { get; }
    public object? EntityId { get; }

    public EntityNotFoundException(string entityName, object? entityId = null)
        : base($"{entityName} no encontrado" + (entityId is not null ? $" con ID: {entityId}" : string.Empty))
    {
        EntityName = entityName;
        EntityId = entityId;
    }
}
