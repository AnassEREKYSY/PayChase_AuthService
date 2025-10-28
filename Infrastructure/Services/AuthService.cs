using Core.Dtos;
using Core.Entities;
using Infrastructure.IRepositories;
using Infrastructure.ISecurity;

namespace Infrastructure.Services;

public class AuthService
{
    private readonly IUserRepository _users;
    private readonly IRefreshTokenRepository _refresh;
    private readonly IJwtProvider _jwt;

    public AuthService(IUserRepository users, IRefreshTokenRepository refresh, IJwtProvider jwt)
    {
        _users = users; _refresh = refresh; _jwt = jwt;
    }

    public async Task<TokenResponse> RegisterAsync(RegisterRequest req, CancellationToken ct)
    {
        var existing = await _users.GetByEmailAsync(req.Email.ToLowerInvariant(), ct);
        if (existing is not null) throw new InvalidOperationException("Email already registered.");

        var user = new User
        {
            Id = Guid.NewGuid().ToString("N"),
            Email = req.Email.ToLowerInvariant(),
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(req.Password),
            FullName = req.FullName
        };

        await _users.CreateAsync(user, ct);
        return await IssueTokensAsync(user, ct);
    }

    public async Task<TokenResponse> LoginAsync(LoginRequest req, CancellationToken ct)
    {
        var user = await _users.GetByEmailAsync(req.Email.ToLowerInvariant(), ct)
                   ?? throw new UnauthorizedAccessException("Invalid credentials.");
        if (!BCrypt.Net.BCrypt.Verify(req.Password, user.PasswordHash))
            throw new UnauthorizedAccessException("Invalid credentials.");

        return await IssueTokensAsync(user, ct);
    }

    public async Task<TokenResponse> RefreshAsync(RefreshRequest req, CancellationToken ct)
    {
        var rt = await _refresh.GetAsync(req.RefreshToken, ct) ?? throw new UnauthorizedAccessException("Invalid refresh token.");
        if (rt.ExpiresAt <= DateTime.UtcNow || rt.Revoked) throw new UnauthorizedAccessException("Refresh token expired.");

        var user = await _users.GetByIdAsync(rt.UserId, ct) ?? throw new UnauthorizedAccessException("User not found.");
        await _refresh.RevokeAsync(rt.Token, ct); // rotate
        return await IssueTokensAsync(user, ct);
    }

    private async Task<TokenResponse> IssueTokensAsync(User user, CancellationToken ct)
    {
        var (access, accessExp) = _jwt.CreateAccessToken(user);
        var (refresh, refreshExp) = _jwt.CreateRefreshToken(user);

        await _refresh.StoreAsync(new RefreshToken
        {
            Id = Guid.NewGuid().ToString("N"),
            UserId = user.Id,
            Token = refresh,
            ExpiresAt = refreshExp
        }, ct);

        return new TokenResponse(access, accessExp, refresh, refreshExp);
    }
}
