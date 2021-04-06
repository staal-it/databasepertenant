using AutoMapper;
using DatabasePerTenant.Shared.Tenant;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace DatabasePerTenant.Data.Tenant
{
    public interface IAddressRepository
    {
        Task<AddressDto[]> GetAddresses();
    }

    public class AddressRepository : IAddressRepository
    {
        private readonly TenantDatabaseContext TenantDatabaseContext;
        private readonly IMapper Mapper;

        public AddressRepository(TenantDatabaseContext tenantDatabaseContext, IMapper mapper)
        {
            TenantDatabaseContext = tenantDatabaseContext;
            Mapper = mapper;
        }

        public async Task<AddressDto[]> GetAddresses()
        {
            var addresses = await TenantDatabaseContext.Address.Take(25).ToArrayAsync();

            return Mapper.Map<AddressDto[]>(addresses);
        }
    }
}
