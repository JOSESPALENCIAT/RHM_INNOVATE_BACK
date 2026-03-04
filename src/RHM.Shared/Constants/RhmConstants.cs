namespace RHM.Shared.Constants;

public static class RhmConstants
{
    public const int MaxUsersPerTenant = 3;

    public static class Roles
    {
        public const string SuperAdmin = "SuperAdmin";
        public const string AccountAdmin = "AccountAdmin";
        public const string Operator = "Operator";
    }

    public static class Claims
    {
        public const string TenantId = "tenant_id";
        public const string Role = "role";
    }
}
