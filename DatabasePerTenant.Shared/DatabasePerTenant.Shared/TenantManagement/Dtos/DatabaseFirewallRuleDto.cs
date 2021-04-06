namespace DatabasePerTenant.Shared.TenantManagement.Dtos
{
    public class DatabaseFirewallRuleDto
    {
        public string RuleName { get; set; }

        public string StartIp { get; set; }

        public string EndIp { get; set; }
    }
}
