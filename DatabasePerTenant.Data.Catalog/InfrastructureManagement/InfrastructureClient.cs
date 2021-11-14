using DatabasePerTenant.Data.Catalog.InfrastructureManagement.Templates.TemplateParameters;
using Microsoft.Azure.Management.ResourceManager;
using Microsoft.Azure.Management.ResourceManager.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Rest.Azure.Authentication;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.IO;
using System.Threading.Tasks;

namespace DatabasePerTenant.Data.Catalog.InfrastructureManagement
{
    public interface IInfrastructureClient
    {
        Task<string> CreateNewTenantDatabase(string tenantName);

        Task<string> CloneTenantDatabase(int tenantToCloneId, string tenantName);
    }

    public class InfrastructureClient : IInfrastructureClient
    {
        private readonly string _resourceGroupName;
        private readonly string _subscriptionId;
        private readonly string _tenantId;
        private readonly string _clientId;
        private readonly string _clientSecret;

        private readonly ICatalogRepository _catalogRepository;
        private readonly IConfiguration _configuration;

        public InfrastructureClient(
            IConfiguration configuration,
            ICatalogRepository catalogRepository)
        {
            _tenantId = configuration["TenantId"];
            _subscriptionId = configuration["SubscriptionId"];
            _clientId = configuration["ResourceManagerAppRegistration:ClientId"];
            _clientSecret = configuration["ResourceManagerAppRegistration:ClientSecret"];
            _resourceGroupName = configuration["SqlResourceGroup"];
            _configuration = configuration;
            _catalogRepository = catalogRepository;
        }

        public async Task<string> CreateNewTenantDatabase(string tenantName)
        {
            const string pathToTemplateFile = "InfrastructureManagement/Templates/tenantdatabasetemplate.json";
            var templateFileContents = GetJsonFileContents(pathToTemplateFile);

            var databaseName = $"sqldb-databasepertenant-{tenantName}-test";

            var parameters = JObject.FromObject(
                new DbCreateArmParameters
                {
                    databaseName = new ArmParameterValue { Value = databaseName },
                    serverName = new ArmParameterValue { Value = _configuration["TenantConfig:TenantServer"] },
                    location = new ArmParameterValue { Value = "westeurope" },
                    elasticPoolName = new ArmParameterValue { Value = _configuration["TenantConfig:TenantPool"] }
                });

            await DeployTemplate(_resourceGroupName, databaseName, templateFileContents, parameters);

            return databaseName;
        }

        public async Task<string> CloneTenantDatabase(int tenantToCloneId, string tenantName)
        {
            const string pathToTemplateFile = "InfrastructureManagement/Templates/tenantdatabasecopytemplate.json";
            var templateFileContents = GetJsonFileContents(pathToTemplateFile);

            var databaseName = $"sqldb-databasepertenant-{tenantName}-test";

            var currentTenant = await _catalogRepository.GetTenant(tenantToCloneId);

            var servername = currentTenant.ElasticPool.Server.ServerName.Replace(".database.windows.net", string.Empty);

            var sourceDatabaseId = $"/subscriptions/{_subscriptionId}/resourcegroups/{_resourceGroupName}/providers/Microsoft.Sql/servers/{servername}/databases/{currentTenant.DatabaseName}";

            var parameters = JObject.FromObject(
                new DbCloneArmParameters
                {
                    databaseName = new ArmParameterValue { Value = databaseName },
                    serverName = new ArmParameterValue { Value = _configuration["TenantConfig:TenantServer"] },
                    location = new ArmParameterValue { Value = "westeurope" },
                    sourceDatabaseId = new ArmParameterValue { Value = sourceDatabaseId },
                    elasticPoolName = new ArmParameterValue { Value = _configuration["TenantConfig:TenantPool"] }
                });

            await DeployTemplate(_resourceGroupName, databaseName, templateFileContents, parameters);

            return databaseName;
        }

        private static JObject GetJsonFileContents(string pathToJson)
        {
            var path = Path.Combine(System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location), pathToJson);
            using (var file = File.OpenText(path))
            {
                using (var reader = new JsonTextReader(file))
                {
                    return (JObject)JToken.ReadFrom(reader);
                }
            }
        }

        private async Task DeployTemplate(string resourceGroupName, string deploymentName, JObject templateFileContents, JObject parameters)
        {
            var deployment = new Deployment
            {
                Properties = new DeploymentProperties
                {
                    Mode = DeploymentMode.Incremental,
                    Template = templateFileContents,
                    Parameters = parameters
                }
            };

            var serviceCredentials = await ApplicationTokenProvider.LoginSilentAsync(_tenantId, _clientId, _clientSecret);
            var resourceManagementClient = new ResourceManagementClient(serviceCredentials)
            {
                SubscriptionId = _subscriptionId
            };

            await resourceManagementClient.Deployments.CreateOrUpdateAsync(resourceGroupName, deploymentName, deployment);
        }
    }
}