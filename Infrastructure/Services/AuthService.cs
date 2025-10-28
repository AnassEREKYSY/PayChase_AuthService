using Core.Dtos;
using Core.Entities;
using Core.Responses;
using Infrastructure.IRepositories;
using Infrastructure.ISecurity;

namespace Infrastructure.Services;

public class AuthService(IUserRepository _users, IRefreshTokenRepository _refresh, IJwtProvider _jwt)
{

    public async Task<MeResponse> RegisterAsync(RegisterRequest req, CancellationToken ct)
    {
        var email = req.Email.Trim().ToLowerInvariant();
        var existing = await _users.GetByEmailAsync(req.OrgId, email, ct);
        if (existing is not null) throw new InvalidOperationException("Email already registered for this org.");

        var user = new User
        {
            Id = Guid.NewGuid().ToString("N"),
            OrgId = req.OrgId,
            Email = email,
            Name = req.Name?.Trim() ?? email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(req.Password),
            Roles = new() { "user" },
            Status = UserStatus.active,
            CreatedAt = DateTime.UtcNow
        };

        await _users.CreateAsync(user, ct);

        return new MeResponse(
            user.Id, user.OrgId, user.Email, user.Name,
            user.Roles.ToArray(), user.Status.ToString(), user.CreatedAt);
    }

    public async Task<TokenResponse> LoginAsync(LoginRequest req, CancellationToken ct)
    {
        var email = req.Email.Trim().ToLowerInvariant();
        var user = await _users.GetByEmailAsync(req.OrgId, email, ct)
                   ?? throw new UnauthorizedAccessException("Invalid credentials.");
        if (!BCrypt.Net.BCrypt.Verify(req.Password, user.PasswordHash))
            throw new UnauthorizedAccessException("Invalid credentials.");
        if (user.Status == UserStatus.disabled)
            throw new UnauthorizedAccessException("Account disabled.");

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
