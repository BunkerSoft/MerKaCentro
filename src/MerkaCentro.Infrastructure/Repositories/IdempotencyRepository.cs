using Microsoft.EntityFrameworkCore;
using MerkaCentro.Domain.Entities;
using MerkaCentro.Domain.Ports.Output;
using MerkaCentro.Infrastructure.Data;

namespace MerkaCentro.Infrastructure.Repositories;

public class IdempotencyRepository : RepositoryBase<IdempotencyRecord, Guid>, IIdempotencyRepository
{
    public IdempotencyRepository(MerkaCentroDbContext context) : base(context)
    {
    }

    public async Task<IdempotencyRecord?> GetByKeyAsync(Guid idempotencyKey, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .FirstOrDefaultAsync(x => x.IdempotencyKey == idempotencyKey, cancellationToken);
    }

    public async Task<bool> ExistsAsync(Guid idempotencyKey, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .AnyAsync(x => x.IdempotencyKey == idempotencyKey, cancellationToken);
    }

    public async Task<IReadOnlyList<IdempotencyRecord>> GetExpiredAsync(CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        return await DbSet
            .Where(x => x.ExpiresAt < now)
            .ToListAsync(cancellationToken);
    }

    public async Task DeleteExpiredAsync(CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        await DbSet
            .Where(x => x.ExpiresAt < now)
            .ExecuteDeleteAsync(cancellationToken);
    }
}
