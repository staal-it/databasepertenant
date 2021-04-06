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
        private readonly string ResourceGroupName;
        private readonly string SubscriptionId;
        private readonly string TenantId;
        private readonly string ClientId;
        private readonly string ClientSecret;

        private readonly ICatalogRepository CatalogRepository;
        private readonly IConfiguration Configuration;

        public InfrastructureClient(
            IConfiguration configuration,
            ICatalogRepository catalogRepository)
        {
            TenantId = configuration["TenantId"];
            SubscriptionId = configuration["SubscriptionId"];
            ClientId = configuration["ResourceManagerAppRegistration:ClientId"];
            ClientSecret = configuration["ResourceManagerAppRegistration:ClientSecret"];
            ResourceGroupName = configuration["SqlResourceGroup"];
            Configuration = configuration;
            CatalogRepository = catalogRepository;
        }

        public async Task<string> CreateNewTenantDatabase(string tenantName)
        {
            string pathToTemplateFile = "InfrastructureManagement/Templates/tenantdatabasetemplate.json";
            var templateFileContents = GetJsonFileContents(pathToTemplateFile);

            var databaseName = $"sqldb-databasepertenant-{tenantName}-test";

            var parameters = JObject.FromObject(
                new DbCreateArmParameters
                {
                    databaseName = new ArmParameterValue { Value = databaseName },
                    serverName = new ArmParameterValue { Value = Configuration["TenantConfig:TenantServer"] },
                    location = new ArmParameterValue { Value = "westeurope" },
                    elasticPoolName = new ArmParameterValue { Value = Configuration["TenantConfig:TenantPool"] }
                });

            await DeployTemplate(ResourceGroupName, databaseName, templateFileContents, parameters);

            return databaseName;
        }

        public async Task<string> CloneTenantDatabase(int tenantToCloneId, string tenantName)
        {
            string pathToTemplateFile = "InfrastructureManagement/Templates/tenantdatabasecopytemplate.json";
            var templateFileContents = GetJsonFileContents(pathToTemplateFile);

            var databaseName = $"sqldb-databasepertenant-{tenantName}-test";

            var currentTenant = await CatalogRepository.GetTenant(tenantToCloneId);

            var servername = currentTenant.ElasticPool.Server.ServerName.Replace(".database.windows.net", string.Empty);

            var sourceDatabaseId = $"/subscriptions/{SubscriptionId}/resourcegroups/{ResourceGroupName}/providers/Microsoft.Sql/servers/{servername}/databases/{currentTenant.DatabaseName}";

            var parameters = JObject.FromObject(
                new DbCloneArmParameters
                {
                    databaseName = new ArmParameterValue { Value = databaseName },
                    serverName = new ArmParameterValue { Value = Configuration["TenantConfig:TenantServer"] },
                    location = new ArmParameterValue { Value = "westeurope" },
                    sourceDatabaseId = new ArmParameterValue { Value = sourceDatabaseId },
                    elasticPoolName = new ArmParameterValue { Value = Configuration["TenantConfig:TenantPool"] }
                });

            await DeployTemplate(ResourceGroupName, databaseName, templateFileContents, parameters);

            return databaseName;
        }

        private JObject GetJsonFileContents(string pathToJson)
        {
            var path = Path.Combine(System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location), pathToJson);
            using (StreamReader file = File.OpenText(path))
            {
                using (JsonTextReader reader = new JsonTextReader(file))
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

            var serviceCreds = await ApplicationTokenProvider.LoginSilentAsync(TenantId, ClientId, ClientSecret);
            var ResourceManagementClient = new ResourceManagementClient(serviceCreds)
            {
                SubscriptionId = SubscriptionId
            };

            ResourceManagementClient.Deployments.CreateOrUpdate(resourceGroupName, deploymentName, deployment);
        }
    }
}