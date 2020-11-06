using Microsoft.EntityFrameworkCore;
using DatabasePerTenant.Data.Catalog.Model;

namespace DatabasePerTenant.Data.Catalog
{
    public partial class CatalogDbContext : DbContext
    {
        public virtual DbSet<Customer> Customers { get; set; }
        public virtual DbSet<Tenant> Tenants { get; set; }
        public virtual DbSet<ElasticPool> ElasticPools { get; set; }
        public virtual DbSet<Server> Servers { get; set; }

        public CatalogDbContext(DbContextOptions<CatalogDbContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Customer>(entity =>
            {
                entity.HasKey(e => e.CustomerId);

                entity.Property(e => e.CustomerName)
                    .IsRequired()
                    .HasMaxLength(128);

                entity.Property(e => e.LastUpdated)
                    .IsRequired();
            });

            modelBuilder.Entity<Tenant>(entity =>
            {
                entity.HasKey(e => e.TenantId);

                entity.Property(e => e.TenantId)
                    .ValueGeneratedNever();

                entity.Property(e => e.HashedTenantId)
                   .IsRequired();

                entity.Property(e => e.DatabaseName)
                    .IsRequired()
                    .HasMaxLength(128);

                entity.Property(e => e.TenantId).HasMaxLength(128);

                entity.Property(e => e.LastUpdated)
                    .IsRequired();
            });

            modelBuilder.Entity<ElasticPool>(entity =>
            {
                entity.HasKey( e => e.ElasticPoolId );

                entity.Property(e => e.ElasticPoolName)
                    .IsRequired()
                    .HasMaxLength(128);

                entity.Property(e => e.LastUpdated)
                    .IsRequired();
            });

            modelBuilder.Entity<Server>(entity =>
            {
                entity.HasKey(e => e.ServerId);

                entity.Property(e => e.ServerName)
                    .IsRequired()
                    .HasMaxLength(128);

                entity.Property(e => e.LastUpdated)
                    .IsRequired();
            });
        }
    }
}