namespace Infrastructure.ISecurity;
public interface ITokenBlacklist
{
    Task BlacklistAsync(string jti, TimeSpan ttl);
    Task<bool> IsBlacklistedAsync(string jti);
}