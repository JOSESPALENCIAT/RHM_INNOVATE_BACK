namespace RHM.Shared.MultiTenant;

public class TenantContext
{
    public Guid? TenantId { get; set; }
    public string? TenantSlug { get; set; }
    public bool IsSuperAdmin { get; set; }
}
