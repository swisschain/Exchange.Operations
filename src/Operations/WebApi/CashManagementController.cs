using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Operations.DomainService;
using Operations.DomainService.Model;

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
        public async Task<IActionResult> CashInAsync([FromBody] CashOperationModel model)
        {
            await _cashOperations.CashInAsync(model);

            return NoContent();
        }

        [HttpPost("cash-out")]
        public async Task<IActionResult> CashOutAsync([FromBody] CashOperationModel model)
        {
            await _cashOperations.CashOutAsync(model);

            return NoContent();
        }


    }
}
