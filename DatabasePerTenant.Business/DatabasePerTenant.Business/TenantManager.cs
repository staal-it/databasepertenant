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
        private readonly ITenantDbManager _tenantDbManager;
        private IFirewallRulesSqlHelper _firewallRulesSqlHelper;
        private readonly ICatalogRepository _catalogRepository;
        private readonly IStorePerRequestTenantData _storePerRequestTenantData;

        public TenantManager(ITenantDbManager tenantDbManager,
            IFirewallRulesSqlHelper firewallRulesSqlHelper,
            ICatalogRepository catalogRepository,
            IStorePerRequestTenantData storePerRequestTenantData)
        {
            _tenantDbManager = tenantDbManager;
            _firewallRulesSqlHelper = firewallRulesSqlHelper;
            _catalogRepository = catalogRepository;
            _storePerRequestTenantData = storePerRequestTenantData;
        }

        public async Task<NewTenantDatabaseDto> CreateNewTenant(TenantDto dto)
        {
            var newTenantDatabase = await _tenantDbManager.RegisterNewTenantAsync(dto.TenantName);

            return newTenantDatabase;
        }

        public async Task<int> CloneTenant(CloneTenantDto dto)
        {
            var tenantId = await _tenantDbManager.CloneTenantAsync(dto.TenantToCloneId, dto.CloneName);

            return tenantId;
        }

        public async Task RemoveTenant(int tenantId)
        {
            await _tenantDbManager.RemoveTenantAsync(tenantId);
        }

        public async Task<List<DatabaseFirewallRuleDto>> GetDatabaseFirewallRulesAsync()
        {
            var tenantId = _storePerRequestTenantData.GetTenantId();

            var tenant = await _catalogRepository.GetTenant(tenantId);

            var firewallRules = await _firewallRulesSqlHelper.GetDatabaseFirewallRules(tenant.ElasticPool.Server.ServerName, tenant.DatabaseName);

            return firewallRules;
        }

        public async Task<DatabaseFirewallRuleDto> CreateNewDatabaseFirewallRulesAsync(DatabaseFirewallRuleDto dto)
        {
            var tenantId = _storePerRequestTenantData.GetTenantId();

            var tenant = await _catalogRepository.GetTenant(tenantId);

            await _firewallRulesSqlHelper.AddDatabaseFirewallRules(tenant.ElasticPool.Server.ServerName, tenant.DatabaseName, dto);

            return dto;
        }

        public async Task RemoveDatabaseFirewallRulesAsync(string databaseFirewallRuleName)
        {
            var tenantId = _storePerRequestTenantData.GetTenantId();

            var tenant = await _catalogRepository.GetTenant(tenantId);

            await _firewallRulesSqlHelper.RemoveDatabaseFirewallRule(tenant.ElasticPool.Server.ServerName, tenant.DatabaseName, databaseFirewallRuleName);
        }
    }
}