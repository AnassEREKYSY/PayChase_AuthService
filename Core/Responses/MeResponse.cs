namespace Core.Responses;
public record MeResponse(
    string Id, string OrgId, string Email, string Name,
    string[] Roles, string Status, DateTime CreatedAt
);