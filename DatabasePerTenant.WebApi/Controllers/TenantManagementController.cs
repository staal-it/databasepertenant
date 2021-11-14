using DatabasePerTenant.Business;
using DatabasePerTenant.Shared.TenantManagement.Dtos;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DatabasePerTenant.WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TenantManagementController : ControllerBase
    {
        private readonly ITenantManager _tenantManager;

        public TenantManagementController(ITenantManager tenantManager)
        {
            _tenantManager = tenantManager;
        }

        [HttpPost]
        [Route("new")]
        public async Task<ActionResult<NewTenantDatabaseDto>> CreateNewTenant([FromBody] TenantDto dto)
        {
            var result = await _tenantManager.CreateNewTenant(dto);

            return Ok(result);
        }

        [HttpPost]
        [Route("clone")]
        public async Task<ActionResult<CloneTenantDto>> CloneTenant([FromBody] CloneTenantDto dto)
        {
            var result = await _tenantManager.CloneTenant(dto);

            return Ok(result);
        }

        [HttpDelete]
        [Route("remove/{tenantId}")]
        public async Task<ActionResult<RemoveTenantDto>> RemoveTenant(int tenantId)
        {
            await _tenantManager.RemoveTenant(tenantId);

            return Ok();
        }

        [HttpGet]
        [Route("firewallrules")]
        public async Task<ActionResult<List<DatabaseFirewallRuleDto>>> GetDatabaseFirewallRulesAsync()
        {
            var result = await _tenantManager.GetDatabaseFirewallRulesAsync();

            return Ok(result);
        }

        [HttpPost]
        [Route("firewallrules")]
        public async Task<ActionResult<DatabaseFirewallRuleDto>> CreateNewDatabaseFirewallRulesAsync([FromBody] DatabaseFirewallRuleDto dto)
        {
            var result = await _tenantManager.CreateNewDatabaseFirewallRulesAsync(dto);

            return Ok(result);
        }

        [HttpDelete]
        [Route("firewallrules/{databaseFirewallRuleName}")]
        public async Task<ActionResult<DatabaseFirewallRuleDto>> RemoveDatabaseFirewallRulesAsync(string databaseFirewallRuleName)
        {
            await _tenantManager.RemoveDatabaseFirewallRulesAsync(databaseFirewallRuleName);

            return Ok();
        }
    }
}
