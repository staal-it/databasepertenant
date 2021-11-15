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
        private readonly IAddressManager _addressManager;

        public AddressController(IAddressManager addressManager)
        {
            this._addressManager = addressManager;
        }

        [HttpGet]
        [Route("")]
        public async Task<ActionResult<AddressDto>> GetAddressAsync()
        {
            var result = await Task.FromResult(default(AddressDto));

            return Ok(result);
        }

        [HttpGet]
        [Route("all")]
        public async Task<ActionResult<List<AddressDto>>> GetAddressesAsync()
        {
            var result = await _addressManager.GetAddresses();

            return Ok(result);
        }
    }
}
