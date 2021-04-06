using DatabasePerTenant.Shared.Configuration;
using Microsoft.Azure.SqlDatabase.ElasticScale.ShardManagement;

namespace DatabasePerTenant.Data.Catalog.SQLHelpers
{
    public class SQLHelperBase
    {
        private const int DatabaseServerPort = 1433;
        private readonly DatabaseConfig DatabaseConfig;

        public SQLHelperBase(DatabaseConfig databaseConfig)
        {
            DatabaseConfig = databaseConfig;
        }

        protected string GetConnectionString(string tenantServer, string databaseName = "")
        {
            var connectionSting = $"Server={SqlProtocol.Tcp}:{tenantServer},{DatabaseServerPort};Database={databaseName};User ID={DatabaseConfig.DatabaseUser};Password={DatabaseConfig.DatabasePassword};Trusted_Connection=False;Encrypt=True;Connection Timeout={DatabaseConfig.ConnectionTimeOut};";

            return connectionSting;
        }
    }
}
