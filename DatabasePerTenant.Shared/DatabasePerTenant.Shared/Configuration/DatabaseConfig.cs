namespace DatabasePerTenant.Shared.Configuration
{
    public class DatabaseConfig
    {
        public string DatabaseUser { get; set; }
        public string DatabasePassword { get; set; }
        public int ConnectionTimeOut { get; set; }
    }
}
