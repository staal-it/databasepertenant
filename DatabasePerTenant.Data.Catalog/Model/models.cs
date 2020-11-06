using System;
using System.Collections.Generic;

namespace DatabasePerTenant.Data.Catalog.Model
{
    public partial class Customer
    {
        public int CustomerId { get; set; }
        public string CustomerName { get; set; }
        public List<Tenant> Tenants { get; set; }
        public DateTime LastUpdated { get; set; }
    }

    public partial class Tenant
    {
        public int TenantId { get; set; }
        public byte[] HashedTenantId { get; set; }
        public string TenantName { get; set; }
        public string DatabaseName { get; set; }
        public int ElasticPoolId { get; set; }
        public ElasticPool ElasticPool { get; set; }
        public int CustomerId { get; set; }
        public Customer Customer { get; set; }
        public DateTime LastUpdated { get; set; }
    }

    public partial class ElasticPool
    {
        public int ElasticPoolId { get; set; }
        public string ElasticPoolName { get; set; }
        public int ServerId { get; set; }
        public Server Server { get; set; }
        public DateTime LastUpdated { get; set; }
    }

    public partial class Server
    {
        public int ServerId { get; set; }
        public string ServerName { get; set; }
        public List<ElasticPool> ElasticPools { get; set; }
        public DateTime LastUpdated { get; set; }
    }
}
