namespace DatabasePerTenant.Shared.TenantManagement.Dtos
{
    public class CloneTenantDto
    {
        public int TenantToCloneId { get; set; }
        public string CloneName { get; set; }
    }
}
