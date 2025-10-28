using Core.Entities;

namespace Infrastructure.IRepositories;
public interface IRefreshTokenRepository
{
    Task StoreAsync(RefreshToken token, CancellationToken ct);
    Task<RefreshToken?> GetAsync(string token, CancellationToken ct);
    Task RevokeAsync(string token, CancellationToken ct);
}