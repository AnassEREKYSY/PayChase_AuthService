namespace Core.Entities;
using MongoDB.Bson.Serialization.Attributes;
public class Org
{
    [BsonId] public string Id { get; set; } = default!;
    [BsonElement("name")] public string Name { get; set; } = default!;
    [BsonElement("settings")] public OrgSettings Settings { get; set; } = new();
    [BsonElement("createdAt")] public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}