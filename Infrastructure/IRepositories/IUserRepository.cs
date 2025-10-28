using Core.Entities;

namespace Infrastructure.IRepositories;
public interface IUserRepository
{
    Task<User?> GetByEmailAsync(string orgId, string email, CancellationToken ct);
    Task<User?> GetByIdAsync(string id, CancellationToken ct);
    Task<string> CreateAsync(User user, CancellationToken ct);
}


