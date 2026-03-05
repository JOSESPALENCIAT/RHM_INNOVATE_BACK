namespace RHM.Application.DTOs.Forms;

public class FormSchemaDto
{
    public string? Id { get; set; }
    public string TenantId { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public List<FormFieldDto> Fields { get; set; } = new();
    public bool IsPublished { get; set; }
    public string? QrCodeBase64 { get; set; }
    public string? PublicUrl { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? PublishedAt { get; set; }
}

public class FormFieldDto
{
    public string Id { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
    public bool IsRequired { get; set; }
    public List<string> Options { get; set; } = new();
    public int Order { get; set; }
    public string? Placeholder { get; set; }

    // Validation
    public int? MinValue { get; set; }
    public int? MaxValue { get; set; }
    public int? MaxLength { get; set; }
    public string? Pattern { get; set; }
    public bool DisableFutureDates { get; set; }

    // Conditional display
    public ShowIfConditionDto? ShowIf { get; set; }

    // Likert scale
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

public class ShowIfConditionDto
{
    public string FieldId { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
}
