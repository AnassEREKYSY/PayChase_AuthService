namespace Core.Responses;
public record MeResponse(string Id, string Email, string? FullName, DateTime CreatedAt);
