using DatabasePerTenant.Shared.Configuration;
using Microsoft.Azure.SqlDatabase.ElasticScale.ShardManagement;

namespace DatabasePerTenant.Data.Catalog.SQLHelpers
{
    public class SqlHelperBase
    {
        private const int DatabaseServerPort = 1433;
        private readonly DatabaseConfig _databaseConfig;

        public SqlHelperBase(DatabaseConfig databaseConfig)
        {
            _databaseConfig = databaseConfig;
        }

        protected string GetConnectionString(string tenantServer, string databaseName = "")
        {
            var connectionSting = $"Server={SqlProtocol.Tcp}:{tenantServer},{DatabaseServerPort};Database={databaseName};User ID={_databaseConfig.DatabaseUser};Password={_databaseConfig.DatabasePassword};Trusted_Connection=False;Encrypt=True;Connection Timeout={_databaseConfig.ConnectionTimeOut};";

            return connectionSting;
        }
    }
}
