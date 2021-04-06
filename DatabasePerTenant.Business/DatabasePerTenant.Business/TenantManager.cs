using DatabasePerTenant.Data.Catalog;
using DatabasePerTenant.Data.Catalog.SQLHelpers;
using DatabasePerTenant.Data.Tenant;
using DatabasePerTenant.Shared.TenantManagement.Dtos;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DatabasePerTenant.Business
{
    public interface ITenantManager
    {
        Task<int> CloneTenant(CloneTenantDto dto);
        Task<DatabaseFirewallRuleDto> CreateNewDatabaseFirewallRulesAsync(DatabaseFirewallRuleDto dto);
        Task<NewTenantDatabaseDto> CreateNewTenant(TenantDto dto);
        Task<List<DatabaseFirewallRuleDto>> GetDatabaseFirewallRulesAsync();
        Task RemoveDatabaseFirewallRulesAsync(string databaseFirewallRuleName);
        Task RemoveTenant(int tenantId);
    }

    public class TenantManager : ITenantManager
    {
        private readonly ITenantDbManager TenantDbManager;
        private IFirewallRulesSQLHelper FirewallRulesSQLHelper;
        private readonly ICatalogRepository CatalogRepository;
        private readonly IStorePerRequestTenantData StorePerRequestTenantData;

        public TenantManager(ITenantDbManager tenantDbManager,
            IFirewallRulesSQLHelper firewallRulesSQLHelper,
            ICatalogRepository catalogRepository,
            IStorePerRequestTenantData storePerRequestTenantData)
        {
            TenantDbManager = tenantDbManager;
            FirewallRulesSQLHelper = firewallRulesSQLHelper;
            CatalogRepository = catalogRepository;
            StorePerRequestTenantData = storePerRequestTenantData;
        }

        public async Task<NewTenantDatabaseDto> CreateNewTenant(TenantDto dto)
        {
            var newTenantDatabase = await TenantDbManager.RegisterNewTenantAsync(dto.TenantName);

            return newTenantDatabase;
        }

        public async Task<int> CloneTenant(CloneTenantDto dto)
        {
            var tenantId = await TenantDbManager.CloneTenantAsync(dto.TenantToCloneId, dto.CloneName);

            return tenantId;
        }

        public async Task RemoveTenant(int tenantId)
        {
            await TenantDbManager.RemoveTenantAsync(tenantId);
        }

        public async Task<List<DatabaseFirewallRuleDto>> GetDatabaseFirewallRulesAsync()
        {
            var tenantId = StorePerRequestTenantData.GetTenantId();

            var tenant = await CatalogRepository.GetTenant(tenantId);

            var firewallRules = await FirewallRulesSQLHelper.GetDatabaseFirewallRules(tenant.ElasticPool.Server.ServerName, tenant.DatabaseName);

            return firewallRules;
        }

        public async Task<DatabaseFirewallRuleDto> CreateNewDatabaseFirewallRulesAsync(DatabaseFirewallRuleDto dto)
        {
            var tenantId = StorePerRequestTenantData.GetTenantId();

            var tenant = await CatalogRepository.GetTenant(tenantId);

            await FirewallRulesSQLHelper.AddDatabaseFirewallRules(tenant.ElasticPool.Server.ServerName, tenant.DatabaseName, dto);

            return dto;
        }

        public async Task RemoveDatabaseFirewallRulesAsync(string databaseFirewallRuleName)
        {
            var tenantId = StorePerRequestTenantData.GetTenantId();

            var tenant = await CatalogRepository.GetTenant(tenantId);

            await FirewallRulesSQLHelper.RemoveDatabaseFirewallRule(tenant.ElasticPool.Server.ServerName, tenant.DatabaseName, databaseFirewallRuleName);
        }
    }
}