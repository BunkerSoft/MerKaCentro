using MiniMarket.Domain.Entities;
using MiniMarket.Domain.Enums;

namespace MiniMarket.Domain.Ports.Output;

public interface IUserRepository : IRepository<User, Guid>
{
    Task<User?> GetByUsernameAsync(string username, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<User>> GetByRoleAsync(UserRole role, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<User>> GetActiveAsync(CancellationToken cancellationToken = default);
    Task<bool> UsernameExistsAsync(string username, Guid? excludeId = null, CancellationToken cancellationToken = default);
}
