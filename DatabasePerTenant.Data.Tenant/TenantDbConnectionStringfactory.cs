using DatabasePerTenant.Data.Catalog.Sharding;

namespace DatabasePerTenant.Data.Tenant
{
    public interface ITenantDbConnectionStringFactory
    {
        string GetConnectionSting();
    }

    public class TenantDbConnectionStringFactory : ITenantDbConnectionStringFactory
    {
        private readonly IShardingManager _shardingManager;
        private readonly IStorePerRequestTenantData _storePerRequestTenantData;

        public TenantDbConnectionStringFactory(IShardingManager shardingManager, IStorePerRequestTenantData storePerRequestTenantData)
        {
            _shardingManager = shardingManager;
            _storePerRequestTenantData = storePerRequestTenantData;
        }

        public string GetConnectionSting()
        {
            var tenantId = _storePerRequestTenantData.GetTenantId();
            // TODO: check if id belongs to logged in user
            var sqlConnectionString = _shardingManager.OpenConnectionForTenant(tenantId).Result;

            return sqlConnectionString;
        }
    }
}