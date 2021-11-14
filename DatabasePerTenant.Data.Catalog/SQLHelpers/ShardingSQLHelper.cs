using DatabasePerTenant.Shared.Configuration;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

namespace DatabasePerTenant.Data.Catalog.SQLHelpers
{
    public interface IShardingSqlHelper
    {
        Task AddNewUser(string userName, string password, string tenantServer, string databaseName);
        void ClearTenantDbOfShardingDataAfterClone(string tenantServer, string databaseName);
        Task DropDatabase(string tenantServer, string databaseToDrop);
        List<string> GetAllTenantNames(string tenantServer);
    }

    public class ShardingSqlHelper : SqlHelperBase, IShardingSqlHelper
    {
        public ShardingSqlHelper(DatabaseConfig databaseConfig) : base(databaseConfig)
        {
        }

        public List<string> GetAllTenantNames(string tenantServer)
        {
            var list = new List<string>();

            var connectionSting = GetConnectionString(tenantServer);

            using (var con = new SqlConnection(connectionSting))
            {
                con.Open();

                using (var cmd = new SqlCommand("SELECT name from sys.databases WHERE name NOT IN ('master')", con))
                {
                    using (var dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            list.Add(dr[0].ToString());
                        }
                    }
                }
            }

            return list;
        }

        public async Task DropDatabase(string tenantServer, string databaseToDrop)
        {
            var connectionSting = GetConnectionString(tenantServer);

            using (var con = new SqlConnection(connectionSting))
            {
                con.Open();

                using (var cmd = new SqlCommand($"DROP DATABASE [{databaseToDrop}]", con))
                {
                    await cmd.ExecuteNonQueryAsync();
                }
            }
        }

        public async Task AddNewUser(string userName, string password, string tenantServer, string databaseName)
        {
            var connectionSting = GetConnectionString(tenantServer, databaseName);

            using (var con = new SqlConnection(connectionSting))
            {
                con.Open();

                using (var cmd = new SqlCommand($"create user {userName} with password ='{password}'", con))
                {
                    await cmd.ExecuteNonQueryAsync();
                }

                using (var cmd = new SqlCommand($"EXEC sp_addrolemember 'db_owner', '{userName}';", con))
                {
                    await cmd.ExecuteNonQueryAsync();
                }
            }
        }

        public void ClearTenantDbOfShardingDataAfterClone(string tenantServer, string databaseName)
        {
            var connectionSting = GetConnectionString(tenantServer, databaseName);

            using (var con = new SqlConnection(connectionSting))
            {
                con.Open();

                RemoveStoredProcedures(con);

                DropTables(con);

                RemoveObjects(con);
            }
        }

        private static void RemoveStoredProcedures(SqlConnection con)
        {
            var storedProcedures = new List<string>();

            using (var cmd = new SqlCommand("select *  from information_schema.routines where routine_type = 'PROCEDURE'", con))
            {
                using (var dataReader = cmd.ExecuteReader())
                {
                    while (dataReader.Read())
                    {
                        storedProcedures.Add($"[{dataReader["SPECIFIC_SCHEMA"]}].[{dataReader["SPECIFIC_NAME"]}]");
                    }
                }
            }

            foreach (var sp in storedProcedures.Where(x => x.StartsWith("[__ShardManagement")))
            {
                using (var cmd = new SqlCommand($"DROP PROCEDURE {sp}", con))
                {
                    cmd.ExecuteNonQuery();
                }
            }
        }

        private static void DropTables(SqlConnection con)
        {
            var tablesToDelete = new List<string>
                {
                    "[__ShardManagement].[ShardMappingsLocal]",
                    "[__ShardManagement].[ShardMapManagerLocal]",
                    "[__ShardManagement].[ShardsLocal]",
                    "[__ShardManagement].[ShardMapsLocal]"
                };

            foreach (var table in tablesToDelete)
            {
                using (var cmd = new SqlCommand($"DROP TABLE {table}", con))
                {
                    cmd.ExecuteNonQuery();
                }
            }
        }

        private static void RemoveObjects(SqlConnection con)
        {
            var objects = new List<string>();

            using (var cmd = new SqlCommand("SELECT * FROM sys.objects WHERE schema_id IN (SELECT schema_id FROM sys.schemas where name = '__ShardManagement')", con))
            {
                using (var dataReader = cmd.ExecuteReader())
                {
                    while (dataReader.Read())
                    {
                        objects.Add(dataReader["name"].ToString());
                    }
                }
            }

            foreach (var sp in objects)
            {
                using (var cmd = new SqlCommand($"DROP FUNCTION [__ShardManagement].[{sp}]", con))
                {
                    cmd.ExecuteNonQuery();
                }
            }
        }
    }
}
