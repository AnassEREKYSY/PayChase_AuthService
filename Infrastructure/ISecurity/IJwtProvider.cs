using Core.Entities;

namespace Infrastructure.ISecurity;
public interface IJwtProvider
{
    (string token, DateTime expiresAt) CreateAccessToken(User user);
    (string token, DateTime expiresAt) CreateRefreshToken(User user);
}