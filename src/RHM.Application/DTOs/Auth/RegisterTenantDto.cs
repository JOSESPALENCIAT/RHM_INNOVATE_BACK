namespace RHM.Application.DTOs.Auth;

public class RegisterTenantDto
{
    public string TenantName { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string ContactEmail { get; set; } = string.Empty;
    public string AdminFirstName { get; set; } = string.Empty;
    public string AdminLastName { get; set; } = string.Empty;
    public string AdminPassword { get; set; } = string.Empty;
}
