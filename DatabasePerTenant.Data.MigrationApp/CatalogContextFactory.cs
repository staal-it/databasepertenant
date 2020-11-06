using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using DatabasePerTenant.Data.Catalog;

namespace DatabasePerTenant.Data.MigrationApp
{
    public class CatalogContextFactory : IDesignTimeDbContextFactory<CatalogDbContext>
    {
        public CatalogDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<CatalogDbContext>();
            optionsBuilder.UseSqlServer("Data Source=localhost;Initial Catalog=catalog;persist security info=True;Integrated Security=SSPI;");

            return new CatalogDbContext(optionsBuilder.Options);
        }
    }
}