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

        void RemoveShard(string tenantDatabase, int tenantId, string tenantServer);

        Task<string> OpenConnectionForTenant(int tenantId);
    }

    public class ShardingManager : IShardingManager
    {
        private readonly DatabaseConfig _databaseConfig;
        private readonly CatalogConfig _catalogConfig;
        private readonly IKeyVaultClient _keyVaultClient;

        private ShardMapManager ShardMapManager { get; set; }
        private static ListShardMap<int> ShardMap { get; set; }

        private const int DatabaseServerPort = 1433;

        public ShardingManager(
            DatabaseConfig databaseConfig,
            CatalogConfig catalogConfig,
            IKeyVaultClient keyVaultClient)
        {
            _databaseConfig = databaseConfig;
            _catalogConfig = catalogConfig;
            _keyVaultClient = keyVaultClient;
        }

        public void Initialize()
        {
            var catalogDbConnectionString = GetConnectionStingWithCredentials();

            ShardMapManager = !ShardMapManagerFactory.TryGetSqlShardMapManager(catalogDbConnectionString, ShardMapManagerLoadPolicy.Lazy, out ShardMapManager shardMapManager)
                ? ShardMapManagerFactory.CreateSqlShardMapManager(catalogDbConnectionString)
                : shardMapManager;

            ShardMap = !ShardMapManager.TryGetListShardMap(_catalogConfig.CatalogDatabase, out ListShardMap<int> shardMap)
                ? ShardMapManager.CreateListShardMap<int>(_catalogConfig.CatalogDatabase)
                : shardMap;
        }

        public void RegisterShardIfNonExisting(string tenantDatabase, int tenantId, string tenantServer)
        {
            var shardLocation = new ShardLocation(tenantServer, tenantDatabase, SqlProtocol.Tcp, DatabaseServerPort);

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

        public void RemoveShard(string tenantDatabase, int tenantId, string tenantServer)
        {
            if (ShardMap.TryGetMappingForKey(tenantId, out PointMapping<int> mapping))
            {
                if (mapping.Status == MappingStatus.Online)
                {
                    mapping = ShardMap.MarkMappingOffline(mapping, MappingOptions.None);
                }

                ShardMap.DeleteMapping(mapping);

                var shardLocation = new ShardLocation(tenantServer, tenantDatabase, SqlProtocol.Tcp, DatabaseServerPort);

                if (ShardMap.TryGetShard(shardLocation, out Shard shard))
                {
                    ShardMap.DeleteShard(shard);
                }
            }
        }

        public async Task<string> OpenConnectionForTenant(int tenantId)
        {
            var baseConnectionString = await GetBaseShardConnectionSting(tenantId);

            var connection = await ShardMap.OpenConnectionForKeyAsync(tenantId, baseConnectionString, ConnectionOptions.Validate);

            return connection.ConnectionString;
        }

        private async Task<string> GetBaseShardConnectionSting(int tenantId)
        {
            var userName = await _keyVaultClient.GetSecret(_keyVaultClient.GetUsernameKey(tenantId));
            var password = await _keyVaultClient.GetSecret(_keyVaultClient.GetPasswordKey(tenantId));

            var baseConnectionString = $"User ID={userName}; Password='{password}'; Trusted_Connection=False; " +
                                        $"Encrypt=True; Connection Timeout=30;Persist Security Info=True;";

            return baseConnectionString;
        }

        private string GetConnectionStingWithCredentials()
        {
            var connectionStringBuilder = new SqlConnectionStringBuilder
            {
                UserID = _databaseConfig.DatabaseUser,
                Password = _databaseConfig.DatabasePassword,
                ApplicationName = "EntityFramework",
                ConnectTimeout = _databaseConfig.ConnectionTimeOut,
                LoadBalanceTimeout = 15,
                DataSource = $"{SqlProtocol.Tcp}:{_catalogConfig.CatalogServer},{DatabaseServerPort}",
                InitialCatalog = _catalogConfig.CatalogDatabase
            };

            return connectionStringBuilder.ConnectionString;
        }
    }
}