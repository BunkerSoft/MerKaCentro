using Microsoft.EntityFrameworkCore;
using MiniMarket.Domain.Entities;
using MiniMarket.Domain.Enums;
using MiniMarket.Domain.Ports.Output;
using MiniMarket.Infrastructure.Data;

namespace MiniMarket.Infrastructure.Repositories;

public class UserRepository : RepositoryBase<User, Guid>, IUserRepository
{
    public UserRepository(MiniMarketDbContext context) : base(context)
    {
    }

    public async Task<User?> GetByUsernameAsync(string username, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .FirstOrDefaultAsync(u => u.Username.ToLower() == username.ToLower(), cancellationToken);
    }

    public async Task<IReadOnlyList<User>> GetByRoleAsync(UserRole role, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Where(u => u.Role == role)
            .OrderBy(u => u.FullName)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<User>> GetActiveAsync(CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Where(u => u.IsActive)
            .OrderBy(u => u.FullName)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> UsernameExistsAsync(string username, Guid? excludeId = null, CancellationToken cancellationToken = default)
    {
        var query = DbSet.Where(u => u.Username.ToLower() == username.ToLower());

        if (excludeId.HasValue)
            query = query.Where(u => u.Id != excludeId.Value);

        return await query.AnyAsync(cancellationToken);
    }
}
