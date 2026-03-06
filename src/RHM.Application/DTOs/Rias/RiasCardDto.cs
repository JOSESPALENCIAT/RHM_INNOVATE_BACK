using RHM.Application.DTOs.Forms;

namespace RHM.Application.DTOs.Rias;

public class TenantRiasConfigDto
{
    public List<RiasCardDto> Cards { get; set; } = new();
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedByUserId { get; set; }
}

public class RiasCardDto
{
    public string Id { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Subtitle { get; set; } = string.Empty;
    public string Color { get; set; } = string.Empty;
    public string Icon { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public List<RiasSectionDto> Sections { get; set; } = new();
}

public class RiasSectionDto
{
    public string Id { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public List<FormFieldDto> Fields { get; set; } = new();
}
