namespace Core.Entities;
using MongoDB.Bson.Serialization.Attributes;
public class QuietHours
{
    [BsonElement("start")] public string Start { get; set; } = "21:00";
    [BsonElement("end")] public string End { get; set; } = "08:00";
}