using DatabasePerTenant.Data.Catalog.InfrastructureManagement;
using DatabasePerTenant.Data.Catalog.Sharding;
using DatabasePerTenant.Data.Catalog.SQLHelpers;
using DatabasePerTenant.Shared.TenantManagement.Dtos;
using Microsoft.Extensions.Configuration;
using System;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace DatabasePerTenant.Data.Catalog
{
    public interface ITenantDbManager
    {
        Task InitializeAndRegisterAllTenants();

        Task<NewTenantDatabaseDto> RegisterNewTenantAsync(string tenantName);

        Task<int> CloneTenantAsync(int tenantToCloneId, string tenantName);

        Task RemoveTenantAsync(int tenantId);
    }

    public class TenantDbManager : ITenantDbManager
    {
        private readonly IShardingManager _shardingManager;
        private readonly ICatalogRepository _catalogRepository;
        private readonly IInfrastructureClient _infrastructureClient;
        private readonly IShardingSqlHelper _shardingSqlHelper;
        private readonly IKeyVaultClient _keyVaultClient;
        private readonly IConfiguration _configuration;

        private readonly string _currentTenantDatabase;

        public TenantDbManager(
            IShardingManager shardingManager,
            ICatalogRepository catalogRepository,
            IInfrastructureClient infrastructureClient,
            IShardingSqlHelper shardingSqlHelper,
            IKeyVaultClient keyVaultClient,
            IConfiguration configuration)
        {
            _shardingManager = shardingManager;
            _catalogRepository = catalogRepository;
            _infrastructureClient = infrastructureClient;
            _shardingSqlHelper = shardingSqlHelper;
            _keyVaultClient = keyVaultClient;
            _configuration = configuration;

            _currentTenantDatabase = $"{_configuration["TenantConfig:TenantServer"]}.database.windows.net";
        }

        public async Task InitializeAndRegisterAllTenants()
        {
            _shardingManager.Initialize();

            // Only enable while debugging shard functionality locally:
            //await RegisterAllTenantInShardAsync();
        }

        private async Task RegisterAllTenantInShardAsync()
        {
            var tenantNames = _shardingSqlHelper.GetAllTenantNames(_currentTenantDatabase);

            foreach (var tenantName in tenantNames)
            {
                await ReregisterExistingTenantAsync(tenantName);
            }
        }

        public async Task<NewTenantDatabaseDto> RegisterNewTenantAsync(string tenantName)
        {
            var tenantServer = FindAvailableTenantServer();

            var normalizedTenantName = NormalizeTenantName(tenantName);

            var tenantDatabase = await _infrastructureClient.CreateNewTenantDatabase(normalizedTenantName);

            var tenantId = await RegisterTenantAsync(tenantName, tenantDatabase, tenantServer);

            var newTenantDatabase = await CreateDatabaseUser(tenantId, normalizedTenantName, tenantServer, tenantDatabase);

            return newTenantDatabase;
        }

        private async Task<NewTenantDatabaseDto> CreateDatabaseUser(int tenantId, string tenantName, string tenantServer, string tenantDatabase)
        {
            var username = tenantName;

            var password = PasswordGenerator.GeneratePassword(16, 6);

            await _shardingSqlHelper.AddNewUser(username, password, tenantServer, tenantDatabase);

            await _keyVaultClient.AddSecret(_keyVaultClient.GetUsernameKey(tenantId), username);
            await _keyVaultClient.AddSecret(_keyVaultClient.GetPasswordKey(tenantId), password);

            return new NewTenantDatabaseDto
            {
                TenantId = tenantId,
                Username = username,
                Password = password,
                Database = tenantDatabase,
                Server = tenantServer
            };
        }

        public async Task<int> CloneTenantAsync(int tenantToCloneId, string tenantName)
        {
            var tenantDatabase = await _infrastructureClient.CloneTenantDatabase(tenantToCloneId, NormalizeTenantName(tenantName));

            var tenantServer = FindAvailableTenantServer();

            _shardingSqlHelper.ClearTenantDbOfShardingDataAfterClone(tenantServer, tenantDatabase);

            return await RegisterTenantAsync(tenantName, tenantDatabase, tenantServer);
        }

        public async Task RemoveTenantAsync(int tenantId)
        {
            var tenant = await _catalogRepository.GetTenant(tenantId);

            if (tenant != null)
            {
                _shardingManager.RemoveShard(tenant.DatabaseName, tenantId, tenant.ElasticPool.Server.ServerName);

                await _catalogRepository.RemoveTenantAsync(tenantId);

                await _catalogRepository.SaveChangesAsync();

                await _shardingSqlHelper.DropDatabase(tenant.ElasticPool.Server.ServerName, tenant.DatabaseName);
            }
        }

        private async Task ReregisterExistingTenantAsync(string tenantDatabase)
        {
            var tenantServer = GetServerByTenant(tenantDatabase);

            await RegisterTenantAsync(tenantDatabase, tenantDatabase, tenantServer);
        }

        private async Task<int> RegisterTenantAsync(string tenantName, string tenantDatabase, string tenantServer)
        {
            var tenantId = GetTenantKey(tenantDatabase);

            _shardingManager.RegisterShardIfNonExisting(tenantDatabase, tenantId, tenantServer);

            await AddTenant(tenantId, tenantName, tenantDatabase, tenantServer).ConfigureAwait(false);

            return tenantId;
        }

        private async Task AddTenant(int tenantId, string tenantName, string tenantDatabase, string tenantServerName)
        {
            var server = await _catalogRepository.GetOrCreateTenantServer(tenantServerName);

            var elasticpool = await _catalogRepository.GetOrCreateElasticPool(server);

            await _catalogRepository.AddTenant(1, tenantId, tenantName, tenantDatabase, elasticpool);

            await _catalogRepository.SaveChangesAsync();
        }

        private string FindAvailableTenantServer()
        {
            return _currentTenantDatabase;
        }

        private string GetServerByTenant(string tenantName)
        {
            return _currentTenantDatabase;
        }

        private int GetTenantKey(string tenantName)
        {
            var normalizedTenantName = NormalizeTenantName(tenantName);

            var tenantNameBytes = Encoding.UTF8.GetBytes(normalizedTenantName);

            var tenantHashBytes = MD5.Create().ComputeHash(tenantNameBytes);

            int tenantKey = BitConverter.ToInt32(tenantHashBytes, 0);

            return tenantKey;
        }

        private static string NormalizeTenantName(string tenantName)
        {
            return tenantName.Replace(" ", string.Empty).ToLower();
        }
    }
}
