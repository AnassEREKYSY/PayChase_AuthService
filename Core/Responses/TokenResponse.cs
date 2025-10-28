namespace Core.Responses;
public record TokenResponse(
    string AccessToken, DateTime ExpiresAt,
    string RefreshToken, DateTime RefreshExpiresAt
);