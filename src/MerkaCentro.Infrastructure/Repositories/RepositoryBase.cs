using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using MerkaCentro.Domain.Common;
using MerkaCentro.Domain.Ports.Output;
using MerkaCentro.Infrastructure.Data;

namespace MerkaCentro.Infrastructure.Repositories;

public abstract class RepositoryBase<T, TId> : IRepository<T, TId>
    where T : AggregateRoot<TId>
    where TId : notnull
{
    protected readonly MerkaCentroDbContext Context;
    protected readonly DbSet<T> DbSet;

    protected RepositoryBase(MerkaCentroDbContext context)
    {
        Context = context;
        DbSet = context.Set<T>();
    }

    public virtual async Task<T?> GetByIdAsync(TId id, CancellationToken cancellationToken = default)
    {
        return await DbSet.FindAsync(new object[] { id }, cancellationToken);
    }

    public virtual async Task<IReadOnlyList<T>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await DbSet.ToListAsync(cancellationToken);
    }

    public virtual async Task<IReadOnlyList<T>> FindAsync(
        Expression<Func<T, bool>> predicate,
        CancellationToken cancellationToken = default)
    {
        return await DbSet.Where(predicate).ToListAsync(cancellationToken);
    }

    public virtual async Task<T?> FirstOrDefaultAsync(
        Expression<Func<T, bool>> predicate,
        CancellationToken cancellationToken = default)
    {
        return await DbSet.FirstOrDefaultAsync(predicate, cancellationToken);
    }

    public virtual async Task<bool> ExistsAsync(
        Expression<Func<T, bool>> predicate,
        CancellationToken cancellationToken = default)
    {
        return await DbSet.AnyAsync(predicate, cancellationToken);
    }

    public virtual async Task<int> CountAsync(
        Expression<Func<T, bool>>? predicate = null,
        CancellationToken cancellationToken = default)
    {
        return predicate == null
            ? await DbSet.CountAsync(cancellationToken)
            : await DbSet.CountAsync(predicate, cancellationToken);
    }

    public virtual async Task AddAsync(T entity, CancellationToken cancellationToken = default)
    {
        await DbSet.AddAsync(entity, cancellationToken);
    }

    public virtual async Task AddRangeAsync(IEnumerable<T> entities, CancellationToken cancellationToken = default)
    {
        await DbSet.AddRangeAsync(entities, cancellationToken);
    }

    public virtual void Update(T entity)
    {
        // Capture which entities are already tracked before Update traverses the graph
        var previouslyTracked = new HashSet<object>(
            Context.ChangeTracker.Entries().Select(e => e.Entity));

        DbSet.Update(entity);

        // Fix: DbSet.Update() marks ALL entities in the graph as Modified,
        // including newly created children (StockMovement, CashMovement, etc.)
        // that don't exist in the DB yet. Reset those to Added.
        foreach (var entry in Context.ChangeTracker.Entries())
        {
            if (entry.State == EntityState.Modified
                && !previouslyTracked.Contains(entry.Entity))
            {
                entry.State = EntityState.Added;
            }
        }
    }

    public virtual void Remove(T entity)
    {
        DbSet.Remove(entity);
    }

    public virtual void RemoveRange(IEnumerable<T> entities)
    {
        DbSet.RemoveRange(entities);
    }
}
