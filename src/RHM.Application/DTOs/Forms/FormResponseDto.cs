namespace RHM.Application.DTOs.Forms;

public class FormResponseDto
{
    public string? Id { get; set; }
    public string FormId { get; set; } = string.Empty;
    public string TenantId { get; set; } = string.Empty;
    public Dictionary<string, string> Data { get; set; } = new();
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public DateTime SubmittedAt { get; set; }
}

public class SubmitFormDto
{
    public Dictionary<string, string> Data { get; set; } = new();
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
}
