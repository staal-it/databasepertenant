using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using DatabasePerTenant.Data.Catalog.Model;
using AutoMapper;
using System;
using System.Net;
using DatabasePerTenant.Shared.TenantManagement.Dtos;

namespace DatabasePerTenant.Data.Catalog
{
    public interface ICatalogRepository
    {
        Task<List<TenantDto>> GetAllTenants();

        Task<Tenant> GetTenant(int tenantId);

        Task RemoveTenantAsync(int tenantId);

        Task AddTenant(int customerId, int tenantId, string tenantName, string databaseName, ElasticPool elasticPool);

        Task<ElasticPool> GetOrCreateElasticPool(Server server);

        Task<Server> GetOrCreateTenantServer(string tenantServerName);

        Task SaveChangesAsync();
    }
    
    public class CatalogRepository : ICatalogRepository
    {
        private readonly CatalogDbContext _catalogDbContext;
        private readonly IMapper _mapper;

        private static readonly string DefaultElasticPoolName = "Pool1";

        public CatalogRepository(
            CatalogDbContext catalogDbContext, 
            IMapper mpper)
        {
            _catalogDbContext = catalogDbContext;
            _mapper = mpper;
        }

        public async Task<List<TenantDto>> GetAllTenants()
        {
            var allTenantsList = await _catalogDbContext.Tenants.ToListAsync();

            if (allTenantsList.Any())
            {
                return _mapper.Map<List<TenantDto>>(allTenantsList);
            }

            return null;
        }

        public async Task<Tenant> GetTenant(int tenantId)
        {
            var tenant = await _catalogDbContext.Tenants.Include(x => x.ElasticPool.Server).FirstOrDefaultAsync(i => i.TenantId == tenantId);

            return tenant;
        }

        public async Task AddTenant(int customerId, int tenantId, string tenantName, string databaseName, ElasticPool elasticPool)
        {
            var tenantExists = await _catalogDbContext.Tenants.AnyAsync(x => x.TenantId == tenantId);

            if (!tenantExists)
            {
                var tenant = new Tenant
                {
                    TenantId = tenantId,
                    CustomerId = customerId,
                    HashedTenantId = ConvertIntKeyToBytesArray(tenantId),
                    TenantName = tenantName,
                    DatabaseName = databaseName,
                    ElasticPool = elasticPool,
                    LastUpdated = DateTime.Now
                };

                _catalogDbContext.Tenants.Add(tenant);
            }
        }

        public async Task<ElasticPool> GetOrCreateElasticPool(Server server)
        {
            var elasticPool = await _catalogDbContext.ElasticPools.FirstOrDefaultAsync(x => x.ElasticPoolName == DefaultElasticPoolName);

            if (elasticPool == null)
            {
                elasticPool = new ElasticPool
                {
                    ElasticPoolName = DefaultElasticPoolName,
                    Server = server,
                    LastUpdated = DateTime.Now
                };

                _catalogDbContext.ElasticPools.Add(elasticPool);
            }

            return elasticPool;
        }

        public async Task<Server> GetOrCreateTenantServer(string tenantServerName)
        {
            var server = await _catalogDbContext.Servers.FirstOrDefaultAsync(x => x.ServerName == tenantServerName);

            if (server == null)
            {
                server = new Server
                {
                    ServerName = tenantServerName,
                    LastUpdated = DateTime.Now
                };

                _catalogDbContext.Servers.Add(server);
            }

            return server;
        }

        public async Task RemoveTenantAsync(int tenantId)
        {
            var tenant = await _catalogDbContext.Tenants.FirstOrDefaultAsync(i => i.TenantId == tenantId);

            if(tenant != null)
            {
                _catalogDbContext.Tenants.Remove(tenant);
            }
        }

        public async Task SaveChangesAsync()
        {
            await _catalogDbContext.SaveChangesAsync();
        }

        private byte[] ConvertIntKeyToBytesArray(int key)
        {
            var normalized = BitConverter.GetBytes(IPAddress.HostToNetworkOrder(key));

            // Maps Int32.Min - Int32.Max to UInt32.Min - UInt32.Max.
            normalized[0] ^= 0x80;

            return normalized;
        }
    }
}
