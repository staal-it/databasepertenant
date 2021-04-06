using DatabasePerTenant.Data.Catalog.InfrastructureManagement.Templates.TemplateParameters;

namespace DatabasePerTenant.Data.Catalog.InfrastructureManagement
{
    public class DbCloneArmParameters
    {
        public ArmParameterValue databaseName { get; set; }
        public ArmParameterValue serverName { get; set; }
        public ArmParameterValue location { get; set; }
        public ArmParameterValue sourceDatabaseId { get; set; }
        public ArmParameterValue elasticPoolName { get; set; }
    }
}
