using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace RHM.Infrastructure.Documents;

public class FormResponse
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = string.Empty;

    public string FormId { get; set; } = string.Empty;
    public string TenantId { get; set; } = string.Empty;

    public Dictionary<string, string> Data { get; set; } = new();

    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public string? IpAddress { get; set; }
    public DateTime SubmittedAt { get; set; } = DateTime.UtcNow;
}
