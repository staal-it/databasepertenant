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
        private readonly IShardingManager ShardingManager;
        private readonly ICatalogRepository CatalogRepository;
        private readonly IInfrastructureClient InfrastructureClient;
        private readonly IShardingSQLHelper ShardingSQLHelper;
        private readonly IKeyVaultClient KeyVaultClient;
        private readonly IConfiguration Configuration;

        private readonly string CurrentTenantDatabase;

        public TenantDbManager(
            IShardingManager shardingManager,
            ICatalogRepository catalogRepository,
            IInfrastructureClient infrastructureClient,
            IShardingSQLHelper shardingSQLHelper,
            IKeyVaultClient keyVaultClient,
            IConfiguration configuration)
        {
            ShardingManager = shardingManager;
            CatalogRepository = catalogRepository;
            InfrastructureClient = infrastructureClient;
            ShardingSQLHelper = shardingSQLHelper;
            KeyVaultClient = keyVaultClient;
            Configuration = configuration;

            CurrentTenantDatabase = $"{Configuration["TenantConfig:TenantServer"]}.database.windows.net";
        }

        public async Task InitializeAndRegisterAllTenants()
        {
            ShardingManager.Initialize();

            // Only enable while debugging shard functionality locally:
            //await RegisterAllTenantInShardAsync();
        }

        private async Task RegisterAllTenantInShardAsync()
        {
            var tenantNames = ShardingSQLHelper.GetAllTenantNames(CurrentTenantDatabase);

            foreach (var tenantName in tenantNames)
            {
                await ReregisterExistingTenantAsync(tenantName);
            }
        }

        public async Task<NewTenantDatabaseDto> RegisterNewTenantAsync(string tenantName)
        {
            var tenantServer = FindAvailableTenantServer();

            var normalizedTenantName = NormalizeTenantName(tenantName);
            var tenantDatabase = await InfrastructureClient.CreateNewTenantDatabase(normalizedTenantName);

            var tenantId = await RegisterTenantAsync(tenantName, tenantDatabase, tenantServer);

            var newTenantDatabase = await CreateDatabaseUser(tenantId, normalizedTenantName, tenantServer, tenantDatabase);

            return newTenantDatabase;
        }

        private async Task<NewTenantDatabaseDto> CreateDatabaseUser(int tenantId, string tenantName, string tenantServer, string tenantDatabase)
        {
            var username = tenantName;

            var password = PasswordGenerator.GeneratePassword(16, 6);

            await ShardingSQLHelper.AddNewUser(username, password, tenantServer, tenantDatabase);

            await KeyVaultClient.AddSecret(KeyVaultClient.GetUsernameKey(tenantId), username);
            await KeyVaultClient.AddSecret(KeyVaultClient.GetPasswordKey(tenantId), password);

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
            var tenantDatabase = await InfrastructureClient.CloneTenantDatabase(tenantToCloneId, NormalizeTenantName(tenantName));

            var tenantServer = FindAvailableTenantServer();

            ShardingSQLHelper.ClearTenantDBOfShardingDataAfterClone(tenantServer, tenantDatabase);

            return await RegisterTenantAsync(tenantName, tenantDatabase, tenantServer);
        }

        public async Task RemoveTenantAsync(int tenantId)
        {
            var tenant = await CatalogRepository.GetTenant(tenantId);

            if (tenant != null)
            {
                ShardingManager.RemoveShard(tenant.DatabaseName, tenantId, tenant.ElasticPool.Server.ServerName);

                await CatalogRepository.RemoveTenantAsync(tenantId);

                await CatalogRepository.SaveChangesAsync();

                await ShardingSQLHelper.DropDatabase(tenant.ElasticPool.Server.ServerName, tenant.DatabaseName);
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

            ShardingManager.RegisterShardIfNonExisting(tenantDatabase, tenantId, tenantServer);

            await AddTenant(tenantId, tenantName, tenantDatabase, tenantServer).ConfigureAwait(false);

            return tenantId;
        }

        private async Task AddTenant(int tenantId, string tenantName, string tenantDatabase, string tenantServerName)
        {
            var server = await CatalogRepository.GetOrCreateTenantServer(tenantServerName);

            var elasticpool = await CatalogRepository.GetOrCreateElasticPool(server);

            await CatalogRepository.AddTenant(1, tenantId, tenantName, tenantDatabase, elasticpool);

            await CatalogRepository.SaveChangesAsync();
        }

        private string FindAvailableTenantServer()
        {
            return CurrentTenantDatabase;
        }

        private string GetServerByTenant(string tenantName)
        {
            return CurrentTenantDatabase;
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
