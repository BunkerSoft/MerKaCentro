using Microsoft.EntityFrameworkCore;
using MiniMarket.Domain.Entities;
using MiniMarket.Domain.Ports.Output;
using MiniMarket.Infrastructure.Data;

namespace MiniMarket.Infrastructure.Repositories;

public class IdempotencyRepository : IIdempotencyRepository
{
    private readonly MiniMarketDbContext _context;

    public IdempotencyRepository(MiniMarketDbContext context)
    {
        _context = context;
    }

    public async Task<IdempotencyRecord?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.IdempotencyRecords
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<IdempotencyRecord>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.IdempotencyRecords
            .OrderByDescending(x => x.ProcessedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(IdempotencyRecord entity, CancellationToken cancellationToken = default)
    {
        await _context.IdempotencyRecords.AddAsync(entity, cancellationToken);
    }

    public void Update(IdempotencyRecord entity)
    {
        _context.IdempotencyRecords.Update(entity);
    }

    public void Remove(IdempotencyRecord entity)
    {
        _context.IdempotencyRecords.Remove(entity);
    }

    public async Task<IdempotencyRecord?> GetByKeyAsync(Guid idempotencyKey, CancellationToken cancellationToken = default)
    {
        return await _context.IdempotencyRecords
            .FirstOrDefaultAsync(x => x.IdempotencyKey == idempotencyKey, cancellationToken);
    }

    public async Task<bool> ExistsAsync(Guid idempotencyKey, CancellationToken cancellationToken = default)
    {
        return await _context.IdempotencyRecords
            .AnyAsync(x => x.IdempotencyKey == idempotencyKey, cancellationToken);
    }

    public async Task<IReadOnlyList<IdempotencyRecord>> GetExpiredAsync(CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        return await _context.IdempotencyRecords
            .Where(x => x.ExpiresAt < now)
            .ToListAsync(cancellationToken);
    }

    public async Task DeleteExpiredAsync(CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        await _context.IdempotencyRecords
            .Where(x => x.ExpiresAt < now)
            .ExecuteDeleteAsync(cancellationToken);
    }
}
