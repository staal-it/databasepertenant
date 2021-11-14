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
        private readonly TenantDatabaseContext _tenantDatabaseContext;
        private readonly IMapper _mapper;

        public AddressRepository(TenantDatabaseContext tenantDatabaseContext, IMapper mapper)
        {
            _tenantDatabaseContext = tenantDatabaseContext;
            _mapper = mapper;
        }

        public async Task<AddressDto[]> GetAddresses()
        {
            var addresses = await _tenantDatabaseContext.Address.Take(25).ToArrayAsync();

            return _mapper.Map<AddressDto[]>(addresses);
        }
    }
}
