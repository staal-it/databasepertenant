using AutoMapper;
using DatabasePerTenant.Shared.Tenant;

namespace DatabasePerTenant.Data.Tenant.Models
{
    public class MapperConfig : Profile
    {
        public MapperConfig()
        {
            CreateMap<Address, AddressDto>();
        }
    }
}
