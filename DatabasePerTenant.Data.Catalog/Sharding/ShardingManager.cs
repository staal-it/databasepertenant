using Microsoft.Azure.SqlDatabase.ElasticScale.ShardManagement;
using DatabasePerTenant.Shared.Configuration;
using System.Data.SqlClient;
using System.Threading.Tasks;

namespace DatabasePerTenant.Data.Catalog.Sharding
{
    public interface IShardingManager
    {
        void Initialize();

        void RegisterShardIfNonExisting(string tenantDatabase, int tenantId, string tenantServer);

        void RemoveShard(string tenantDatbase, int tenantId, string tenantServer);

        Task<string> OpenConnectionForTenant(int tenantId);
    }

    public class ShardingManager : IShardingManager
    {
        private readonly DatabaseConfig DatabaseConfig;
        private readonly CatalogConfig CatalogConfig;
        private readonly IKeyVaultClient KeyVaultClient;

        private ShardMapManager ShardMapManager { get; set; }
        private static ListShardMap<int> ShardMap { get; set; }

        private const int DatabaseServerPort = 1433;

        public ShardingManager(
            DatabaseConfig databaseConfig,
            CatalogConfig catalogConfig,
            IKeyVaultClient keyVaultClient)
        {
            DatabaseConfig = databaseConfig;
            CatalogConfig = catalogConfig;
            KeyVaultClient = keyVaultClient;
        }

        public void Initialize()
        {
            ShardMapManager = GetShardMapManager();

            ShardMap = GetShardMap();
        }

        private ShardMapManager GetShardMapManager()
        {
            var catalogDbConnectionString = GetConnectionStingWithCredentials();

            return !ShardMapManagerFactory.TryGetSqlShardMapManager(catalogDbConnectionString, ShardMapManagerLoadPolicy.Lazy, out ShardMapManager shardMapManager)
                                ? ShardMapManagerFactory.CreateSqlShardMapManager(catalogDbConnectionString)
                                : shardMapManager;
        }

        private ListShardMap<int> GetShardMap()
        {
            return !ShardMapManager.TryGetListShardMap(CatalogConfig.CatalogDatabase, out ListShardMap<int> shardMap)
                            ? ShardMapManager.CreateListShardMap<int>(CatalogConfig.CatalogDatabase)
                            : shardMap;
        }

        public void RegisterShardIfNonExisting(string tenantDatbase, int tenantId, string tenantServer)
        {
            var shardLocation = new ShardLocation(tenantServer, tenantDatbase, SqlProtocol.Tcp, DatabaseServerPort);

            if (!ShardMap.TryGetShard(shardLocation, out Shard shard))
            {
                shard = ShardMap.CreateShard(shardLocation);
            }

            // Register the mapping of the tenant to the shard in the shard map.
            // After this step, DDR on the shard map can be used
            if (!ShardMap.TryGetMappingForKey(tenantId, out PointMapping<int> _))
            {
                ShardMap.CreatePointMapping(tenantId, shard);
            }
        }

        public void RemoveShard(string tenantDatbase, int tenantId, string tenantServer)
        {
            if (ShardMap.TryGetMappingForKey(tenantId, out PointMapping<int> mapping))
            {
                if (mapping.Status == MappingStatus.Online)
                {
                    mapping = ShardMap.MarkMappingOffline(mapping, MappingOptions.None);
                }

                ShardMap.DeleteMapping(mapping);

                var shardLocation = new ShardLocation(tenantServer, tenantDatbase, SqlProtocol.Tcp, DatabaseServerPort);

                if (ShardMap.TryGetShard(shardLocation, out Shard shard))
                {
                    ShardMap.DeleteShard(shard);
                }
            }
        }

        public async Task<string> OpenConnectionForTenant(int tenantId)
        {
            var baseConnSting = await GetBaseShardConnectionSting(tenantId);

            var connection = await ShardMap.OpenConnectionForKeyAsync(tenantId, baseConnSting, ConnectionOptions.Validate);

            return connection.ConnectionString;
        }

        private async Task<string> GetBaseShardConnectionSting(int tenantId)
        {
            var userName = await KeyVaultClient.GetSecret(KeyVaultClient.GetUsernameKey(tenantId));
            var password = await KeyVaultClient.GetSecret(KeyVaultClient.GetPasswordKey(tenantId));

            var connSting = $"User ID={userName}; Password='{password}'; Trusted_Connection=False; Encrypt=True; Connection Timeout=30;Persist Security Info=True;";

            return connSting;
        }

        private string GetConnectionStingWithCredentials()
        {
            var connectionStringBuilder = new SqlConnectionStringBuilder
            {
                UserID = DatabaseConfig.DatabaseUser,
                Password = DatabaseConfig.DatabasePassword,
                ApplicationName = "EntityFramework",
                ConnectTimeout = DatabaseConfig.ConnectionTimeOut,
                LoadBalanceTimeout = 15,
                DataSource = $"{SqlProtocol.Tcp}:{CatalogConfig.CatalogServer},{DatabaseServerPort}",
                InitialCatalog = CatalogConfig.CatalogDatabase
            };

            return connectionStringBuilder.ConnectionString;
        }
    }
}