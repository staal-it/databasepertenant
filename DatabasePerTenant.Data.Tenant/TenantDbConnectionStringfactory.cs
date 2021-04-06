using DatabasePerTenant.Data.Catalog.Sharding;

namespace DatabasePerTenant.Data.Tenant
{
    public interface ITenantDbConnectionStringfactory
    {
        string GetConnectionSting();
    }

    public class TenantDbConnectionStringfactory : ITenantDbConnectionStringfactory
    {
        private readonly IShardingManager ShardingManager;
        private IStorePerRequestTenantData StorePerRequestTenantData;

        public TenantDbConnectionStringfactory(IShardingManager shardingManager, IStorePerRequestTenantData storePerRequestTenantData)
        {
            ShardingManager = shardingManager;
            StorePerRequestTenantData = storePerRequestTenantData;
        }

        public string GetConnectionSting()
        {
            // TODO: check if id belongs to logged in user
            var tenantId = StorePerRequestTenantData.GetTenantId();

            var sqlConnectionsting = ShardingManager.OpenConnectionForTenant(tenantId).Result;

            return sqlConnectionsting;
        }
    }
}