using DatabasePerTenant.Business;
using DatabasePerTenant.Shared.Tenant;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DatabasePerTenant.WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AddressController : ControllerBase
    {
        private readonly IAddressManager addressManager;

        public AddressController(IAddressManager addressManager)
        {
            this.addressManager = addressManager;
        }

        [HttpGet]
        [Route("")]
        public async Task<ActionResult<AddressDto>> GetAddressAsync()
        {
            var result = default(AddressDto);

            return Ok(result);
        }

        [HttpGet]
        [Route("all")]
        public async Task<ActionResult<List<AddressDto>>> GetAddressesAsync()
        {
            var result = await addressManager.GetAddresses();

            return Ok(result);
        }
    }
}
