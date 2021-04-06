using DatabasePerTenant.Data.Tenant;
using DatabasePerTenant.Shared.Tenant;
using System.Threading.Tasks;

namespace DatabasePerTenant.Business
{
    public interface IAddressManager
    {
        Task<AddressDto[]> GetAddresses();
    }

    public class AddressManager : IAddressManager
    {
        private readonly IAddressRepository addressRepository;

        public AddressManager(IAddressRepository addressRepository)
        {
            this.addressRepository = addressRepository;
        }

        public async Task<AddressDto[]> GetAddresses()
        {
            var addresses = await addressRepository.GetAddresses();

            return addresses;
        }
    }
}
