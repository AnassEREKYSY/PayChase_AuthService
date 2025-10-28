using Core.Entities;

namespace Infrastructure.IRepositories;
public interface IOrgRepository
{
    Task<Org?> GetByIdAsync(string id, CancellationToken ct);
}