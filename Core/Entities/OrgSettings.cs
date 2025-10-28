namespace Core.Entities;
using MongoDB.Bson.Serialization.Attributes;
public class OrgSettings
{
    [BsonElement("timezone")] public string Timezone { get; set; } = "UTC";
    [BsonElement("quietHours")] public QuietHours QuietHours { get; set; } = new();
    [BsonElement("defaultChannels")] public List<string> DefaultChannels { get; set; } = new() { "email" };
}