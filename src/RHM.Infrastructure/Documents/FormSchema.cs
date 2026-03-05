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
    /// <summary>
    /// text | email | number | date | textarea | select | radio | checkbox |
    /// geolocation | autocomplete | likert | section | cascading
    /// </summary>
    public string Type { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
    public bool IsRequired { get; set; } = false;
    public List<string> Options { get; set; } = new();
    public int Order { get; set; }
    public string? Placeholder { get; set; }

    // Validation
    public int? MinValue { get; set; }
    public int? MaxValue { get; set; }
    public int? MaxLength { get; set; }
    public string? Pattern { get; set; }
    public bool DisableFutureDates { get; set; } = false;

    // Conditional display
    public ShowIfCondition? ShowIf { get; set; }

    // Likert scale (default 1-5)
    public int ScaleMin { get; set; } = 1;
    public int ScaleMax { get; set; } = 5;
    public string? ScaleMinLabel { get; set; }
    public string? ScaleMaxLabel { get; set; }

    // Cascading dropdown
    public string? ParentFieldId { get; set; }
    public Dictionary<string, List<string>>? CascadeOptions { get; set; }

    // Section separator
    public string? SectionDescription { get; set; }
}

public class ShowIfCondition
{
    public string FieldId { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
}
