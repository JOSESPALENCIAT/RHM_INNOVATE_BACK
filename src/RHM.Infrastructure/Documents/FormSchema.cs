using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace RHM.Infrastructure.Documents;

public class FormSchema
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = string.Empty;

    public string TenantId { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public List<FormField> Fields { get; set; } = new();
    public bool IsPublished { get; set; } = false;
    public string? QrCodeBase64 { get; set; }
    public string? PublicUrl { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? PublishedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class FormField
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Type { get; set; } = string.Empty; // text, select, checkbox, date, number, textarea
    public string Label { get; set; } = string.Empty;
    public bool IsRequired { get; set; } = false;
    public List<string> Options { get; set; } = new();
    public int Order { get; set; }
    public string? Placeholder { get; set; }
}
