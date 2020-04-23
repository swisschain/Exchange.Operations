using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Operations.DomainService;
using Operations.DomainService.Model;
using Swisschain.Sdk.Server.Authorization;
using Swisschain.Sdk.Server.WebApi.Common;

namespace Operations.WebApi
{
    [Authorize]
    [ApiController]
    [Route("api/cash-management")]
    public class CashManagementController : ControllerBase
    {
        private readonly ICashOperations _cashOperations;

        public CashManagementController(ICashOperations cashOperations)
        {
            _cashOperations = cashOperations;
        }

        [HttpPost("cash-in")]
        [ProducesResponseType(typeof(OperationResponse), StatusCodes.Status200OK)]
        public async Task<IActionResult> CashInAsync([FromBody] CashInOutModel model)
        {
            var response = await _cashOperations.CashInAsync(User.GetTenantId(), model);

            return Ok(response);
        }

        [HttpPost("cash-out")]
        [ProducesResponseType(typeof(OperationResponse), StatusCodes.Status200OK)]
        public async Task<IActionResult> CashOutAsync([FromBody] CashInOutModel model)
        {
            var response = await _cashOperations.CashOutAsync(User.GetTenantId(), model);

            return Ok(response);
        }

        [HttpPost("transfer")]
        [ProducesResponseType(typeof(OperationResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ModelStateDictionaryErrorResponse), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> CashTransferAsync([FromBody] CashTransferModel model)
        {
            var response = await _cashOperations.CashTransferAsync(User.GetTenantId(), model);

            return Ok(response);
        }
    }
}
