using StackExchange.Redis;
using Infrastructure.ISecurity;

namespace PayChase.Auth.Infrastructure.Security;

public class RedisTokenBlacklist : ITokenBlacklist
{
    private readonly IDatabase _db;
    public RedisTokenBlacklist(IConnectionMultiplexer mux) => _db = mux.GetDatabase();

    public Task BlacklistAsync(string jti, TimeSpan ttl) => _db.StringSetAsync($"bl:{jti}", "1", ttl);
    public async Task<bool> IsBlacklistedAsync(string jti) => await _db.StringGetAsync($"bl:{jti}") == "1";
}
