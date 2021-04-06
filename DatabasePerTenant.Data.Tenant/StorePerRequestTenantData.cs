using Microsoft.AspNetCore.Http;

namespace DatabasePerTenant.Data.Tenant
{
    public interface IStorePerRequestTenantData
    {
        public int GetTenantId();
    }

    public class StorePerRequestTenantData : IStorePerRequestTenantData
    {
        private readonly HttpContext HttpContext;

        public StorePerRequestTenantData(HttpContext httpContext)
        {
            HttpContext = httpContext;
        }

        public int GetTenantId()
        {
            int tenantId = -1;
            if (HttpContext != null && HttpContext.Request.Headers.TryGetValue("tenantId", out var tenantIdFromHeader))
            {
                tenantId = int.Parse(tenantIdFromHeader);
            }

            return tenantId;
        }
    }
}
