using RHM.Domain.Enums;

namespace RHM.Domain.Entities;

public class Subscription
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public decimal MonthlyAmount { get; set; }
    public SubscriptionStatus Status { get; set; } = SubscriptionStatus.Active;

    public Tenant Tenant { get; set; } = null!;
}
