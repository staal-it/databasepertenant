using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using DatabasePerTenant.Data.Catalog;
using DatabasePerTenant.Data.Tenant;

namespace DatabasePerTenant.Data.MigrationApp
{
    public class CatalogContextFactory : IDesignTimeDbContextFactory<CatalogDbContext>
    {
        public CatalogDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<CatalogDbContext>();
            optionsBuilder.UseSqlServer("Server=tcp:sql-databasepertenant-catalog-test.database.windows.net,1433;Initial Catalog=sqldb-dafaultdatabase;Persist Security Info=False;User ID=databasepertenant;Password=1234@Demo;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;");

            return new CatalogDbContext(optionsBuilder.Options);
        }

        public class TenantDatabaseContextFactory : IDesignTimeDbContextFactory<TenantDatabaseContext>
        {
            public TenantDatabaseContext CreateDbContext(string[] args)
            {
                var optionsBuilder = new DbContextOptionsBuilder<TenantDatabaseContext>();
                optionsBuilder.UseSqlServer("Data Source=localhost;Initial Catalog=databasepertenant;persist security info=True;Integrated Security=SSPI;");

                return new TenantDatabaseContext(optionsBuilder.Options, new MigrationTenantDbConnectionStringFactory());
            }
        }

        public class MigrationTenantDbConnectionStringFactory : ITenantDbConnectionStringFactory
        {
            public string GetConnectionSting()
            {
                string sqlConnectionsting = "Data Source=localhost;Initial Catalog=databasepertenant;persist security info=True;Integrated Security=SSPI;";
                return sqlConnectionsting;
            }
        }
    }
}