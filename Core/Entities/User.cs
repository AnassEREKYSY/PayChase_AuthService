using MongoDB.Bson.Serialization.Attributes;

namespace Core.Entities;

public enum UserStatus { active, disabled }

public class User
{
    [BsonId] public string Id { get; set; } = default!;

    [BsonElement("orgId")] public string OrgId { get; set; } = default!;
    [BsonElement("email")] public string Email { get; set; } = default!;
    [BsonElement("name")] public string Name { get; set; } = default!;
    [BsonElement("passwordHash")] public string PasswordHash { get; set; } = default!;
    [BsonElement("roles")] public List<string> Roles { get; set; } = new() { "user" };
    [BsonElement("status")] public UserStatus Status { get; set; } = UserStatus.active;
    [BsonElement("createdAt")] public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
