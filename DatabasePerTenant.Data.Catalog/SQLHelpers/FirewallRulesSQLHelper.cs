using DatabasePerTenant.Shared.Configuration;
using DatabasePerTenant.Shared.TenantManagement.Dtos;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Threading.Tasks;

namespace DatabasePerTenant.Data.Catalog.SQLHelpers
{
    public interface IFirewallRulesSQLHelper
    {
        Task<List<DatabaseFirewallRuleDto>> GetDatabaseFirewallRules(string tenantServer, string tenantDatabase);
        Task AddDatabaseFirewallRules(string tenantServer, string tenantDatabase, DatabaseFirewallRuleDto databaseFirewallRuleDto);
        Task RemoveDatabaseFirewallRule(string tenantServer, string tenantDatabase, string databaseFirewallRuleName);
    }

    public class FirewallRulesSQLHelper : SQLHelperBase, IFirewallRulesSQLHelper
    {
        public FirewallRulesSQLHelper(DatabaseConfig databaseConfig) : base(databaseConfig)
        {
        }

        public async Task<List<DatabaseFirewallRuleDto>> GetDatabaseFirewallRules(string tenantServer, string tenantDatabase)
        {
            var connectionSting = GetConnectionString(tenantServer, tenantDatabase);

            var ipRules = new List<DatabaseFirewallRuleDto>();

            using (var con = new SqlConnection(connectionSting))
            {
                con.Open();

                using (var cmd = new SqlCommand("SELECT * FROM sys.database_firewall_rules", con))
                {
                    using (var dataReader = await cmd.ExecuteReaderAsync())
                    {
                        while (dataReader.Read())
                        {
                            ipRules.Add(
                                new DatabaseFirewallRuleDto
                                {
                                    RuleName = dataReader["name"].ToString(),
                                    StartIp = dataReader["start_ip_address"].ToString(),
                                    EndIp = dataReader["end_ip_address"].ToString()
                                }
                            );
                        }
                    }
                }
            }

            return ipRules;
        }

        public async Task AddDatabaseFirewallRules(string tenantServer, string tenantDatabase, DatabaseFirewallRuleDto databaseFirewallRuleDto)
        {
            var connectionSting = GetConnectionString(tenantServer, tenantDatabase);

            using (var con = new SqlConnection(connectionSting))
            {
                con.Open();

                var command = $"EXECUTE sp_set_database_firewall_rule @name = N'{databaseFirewallRuleDto.RuleName}', @start_ip_address = '{databaseFirewallRuleDto.StartIp}', @end_ip_address = '{databaseFirewallRuleDto.EndIp}'";
                using (var cmd = new SqlCommand(command, con))
                {
                    await cmd.ExecuteNonQueryAsync();
                }
            }
        }

        public async Task RemoveDatabaseFirewallRule(string tenantServer, string tenantDatabase, string databaseFirewallRuleName)
        {
            var connectionSting = GetConnectionString(tenantServer, tenantDatabase);

            using (var con = new SqlConnection(connectionSting))
            {
                con.Open();

                var command = $"EXECUTE sp_delete_database_firewall_rule @name = N'{databaseFirewallRuleName}'";
                using (var cmd = new SqlCommand(command, con))
                {
                    await cmd.ExecuteNonQueryAsync();
                }
            }
        }
    }
}
