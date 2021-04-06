using DatabasePerTenant.Shared.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using DatabasePerTenant.Data.Catalog.Sharding;
using DatabasePerTenant.Data.Catalog.InfrastructureManagement;
using DatabasePerTenant.Data.Catalog.SQLHelpers;
using DatabasePerTenant.Data.Catalog;
using DatabasePerTenant.Data.Tenant;
using DatabasePerTenant.Business;

namespace DatabasePerTenant.WebApi.Extensions
{
    public static class DependencyInjectionRegister
    {
        public static void BuildDependencyRegister(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddScoped<IAddressManager, AddressManager>();
            services.AddScoped<IAddressRepository, AddressRepository>();

            services.AddScoped<ITenantManager, TenantManager>();

            services.AddScoped<ICatalogRepository, CatalogRepository>();
            services.AddDbContext<TenantDatabaseContext>();

            services.AddScoped<IShardingSQLHelper, ShardingSQLHelper>();
            services.AddScoped<IFirewallRulesSQLHelper, FirewallRulesSQLHelper>();
            services.AddSingleton<IShardingManager, ShardingManager>();
            services.AddSingleton<IKeyVaultClient, KeyVaultClient>();
            services.AddScoped<IInfrastructureClient, InfrastructureClient>();
            services.AddScoped<ITenantDbManager, TenantDbManager>();
            services.AddScoped<IShardingSQLHelper, ShardingSQLHelper>();
            services.AddScoped<IFirewallRulesSQLHelper, FirewallRulesSQLHelper>();
            services.AddSingleton<IShardingManager, ShardingManager>();
            services.AddScoped<IStorePerRequestTenantData, StorePerRequestTenantData>();
            services.AddScoped<ITenantDbConnectionStringfactory, TenantDbConnectionStringfactory>();

            services.AddAutoMapper(typeof(IAutoRegisterData));

            RegisterConfiguration(services, configuration);
        }

        private static void RegisterConfiguration(IServiceCollection services, IConfiguration configuration)
        {
            var catalogConfig = new CatalogConfig();
            configuration.Bind("CatalogConfig", catalogConfig);
            services.AddSingleton(catalogConfig);

            var databaseConfig = new DatabaseConfig();
            configuration.Bind("DatabaseConfig", databaseConfig);
            services.AddSingleton(databaseConfig);
        }
    }
}
