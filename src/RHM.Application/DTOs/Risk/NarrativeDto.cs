namespace RHM.Application.DTOs.Risk;

public class NarrativeResponseDto
{
    public string PatientId { get; set; } = string.Empty;
    public string ProfileId { get; set; } = string.Empty;
    public string Narrative { get; set; } = string.Empty;
    public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
}
