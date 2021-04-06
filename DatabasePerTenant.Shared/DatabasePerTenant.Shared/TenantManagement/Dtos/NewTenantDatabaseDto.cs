using System;
using System.Collections.Generic;
using System.Text;

namespace DatabasePerTenant.Shared.TenantManagement.Dtos
{
    public class NewTenantDatabaseDto
    {
        public int TenantId { get; set; }
        public string Database { get; set; }
        public string Server { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
    }
}
