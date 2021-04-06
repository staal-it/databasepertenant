using DatabasePerTenant.Data.Catalog;
using DatabasePerTenant.Shared.Configuration;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DatabasePerTenant.WebApi.Extensions
{
    public static class DatabaseConnection
    {
        public static void SetDatabaseConnection(this IServiceCollection services, IConfiguration configuration)
        {
            RegisterCatalogDatabase(services, configuration);
        }

        private static void RegisterCatalogDatabase(IServiceCollection services, IConfiguration configuration)
        {
            var databaseConfig = configuration.GetSection("DatabaseConfig").Get<DatabaseConfig>();
            var catalogConfig = configuration.GetSection("CatalogConfig").Get<CatalogConfig>();

            services.AddDbContext<CatalogDbContext>(options => options.UseSqlServer(GetCatalogConnectionString(catalogConfig, databaseConfig)));
        }

        private static string GetCatalogConnectionString(CatalogConfig catalogConfig, DatabaseConfig databaseConfig)
        {
            return $"Server=tcp:{catalogConfig.CatalogServer},1433;Database={catalogConfig.CatalogDatabase};User ID={databaseConfig.DatabaseUser};Password={databaseConfig.DatabasePassword};Trusted_Connection=False;Encrypt=True;";
        }
    }
}
