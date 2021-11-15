using Microsoft.AspNetCore.Http;

namespace DatabasePerTenant.Data.Tenant
{
    public interface IStorePerRequestTenantData
    {
        public int GetTenantId();
    }

    public class StorePerRequestTenantData : IStorePerRequestTenantData
    {
        private readonly HttpContext _httpContext;

        public StorePerRequestTenantData(HttpContext httpContext)
        {
            _httpContext = httpContext;
        }

        public int GetTenantId()
        {
            var tenantId = -1;
            if (_httpContext != null && _httpContext.Request.Headers.TryGetValue("tenantId", out var tenantIdFromHeader))
            {
                tenantId = int.Parse(tenantIdFromHeader);
            }

            return tenantId;
        }
    }
}
