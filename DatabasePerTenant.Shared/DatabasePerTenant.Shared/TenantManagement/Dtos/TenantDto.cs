namespace DatabasePerTenant.Shared.TenantManagement.Dtos
{
    public class TenantDto
    {
        public int? TenantId { get; set; }
        public string TenantName { get; set; }
        public string DatabaseName { get; set; }
    }
}
