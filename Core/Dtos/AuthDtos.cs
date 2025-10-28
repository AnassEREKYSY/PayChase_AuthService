namespace Core.Dtos;

public record RegisterRequest(string Email, string Password, string? FullName);
public record LoginRequest(string Email, string Password);
public record RefreshRequest(string RefreshToken);
public record TokenResponse(string AccessToken, DateTime ExpiresAt, string RefreshToken, DateTime RefreshExpiresAt);
public record MeResponse(string Id, string Email, string? FullName, DateTime CreatedAt);
