using System.IdentityModel.Tokens.Jwt; 
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Configuration;
using Infrastructure.ISecurity;
using Core.Entities;

namespace Infrastructure.Security;

public class JwtProvider : IJwtProvider
{
    private readonly string _issuer, _audience, _key;
    private readonly int _accessMinutes, _refreshDays;

    public JwtProvider(IConfiguration cfg)
    {
        _issuer = cfg["JWT_ISSUER"] ?? cfg["Jwt:Issuer"]!;
        _audience = cfg["JWT_AUDIENCE"] ?? cfg["Jwt:Audience"]!;
        _key = cfg["JWT_KEY"]!;
        _accessMinutes = int.Parse(cfg["JWT_EXPIRES_MIN"] ?? cfg["Jwt:ExpiresMinutes"] ?? "15");
        _refreshDays = int.Parse(cfg["REFRESH_EXPIRES_DAYS"] ?? cfg["Jwt:RefreshDays"] ?? "7");
    }

    public (string token, DateTime expiresAt) CreateAccessToken(User user)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_key));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var expires = DateTime.UtcNow.AddMinutes(_accessMinutes);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id),
            new("orgId", user.OrgId),
            new(JwtRegisteredClaimNames.Email, user.Email),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };
        foreach (var r in user.Roles) claims.Add(new Claim(ClaimTypes.Role, r));

        var jwt = new JwtSecurityToken(
            issuer: _issuer,
            audience: _audience,
            claims: claims,
            notBefore: DateTime.UtcNow,
            expires: expires,
            signingCredentials: creds
        );
        return (new JwtSecurityTokenHandler().WriteToken(jwt), expires);
    }

    public (string token, DateTime expiresAt) CreateRefreshToken(User user)
    {
        var token = Convert.ToBase64String(Guid.NewGuid().ToByteArray());
        return (token, DateTime.UtcNow.AddDays(_refreshDays));
    }
}