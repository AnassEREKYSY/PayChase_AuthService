using Infrastructure.IRepositories;
using Core.Entities;
using MongoDB.Driver;

namespace Infrastructure.Repositories;
public class RefreshTokenRepository : IRefreshTokenRepository
{
    private readonly IMongoCollection<RefreshToken> _col;
    public RefreshTokenRepository(IMongoDatabase db) => _col = db.GetCollection<RefreshToken>("refreshTokens");

    public Task StoreAsync(RefreshToken token, CancellationToken ct) => _col.InsertOneAsync(token, cancellationToken: ct);

    public async Task<RefreshToken?> GetAsync(string token, CancellationToken ct) =>
        await _col.Find(x => x.Token == token && !x.Revoked).FirstOrDefaultAsync(ct);

    public Task RevokeAsync(string token, CancellationToken ct) =>
        _col.UpdateOneAsync(x => x.Token == token, Builders<RefreshToken>.Update.Set(x => x.Revoked, true), cancellationToken: ct);
}
