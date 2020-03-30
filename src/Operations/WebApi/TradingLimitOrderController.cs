using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Operations.DomainService;
using Operations.DomainService.Model;

namespace Operations.WebApi
{
    [Authorize]
    [ApiController]
    [Route("api/trading/limit-order")]
    public class TradingLimitOrderController : ControllerBase
    {
        private readonly ILimitOrderOperations _limitOrderOperations;

        public TradingLimitOrderController(ILimitOrderOperations limitOrderOperations)
        {
            _limitOrderOperations = limitOrderOperations;
        }

        [HttpPost]
        [ProducesResponseType(typeof(LimitOrderResponse), StatusCodes.Status200OK)]
        public async Task<IActionResult> CreateAsync([FromBody] LimitOrderCreateModel model)
        {
            var result = await _limitOrderOperations.CreateAsync(model);
            return Ok(result);
        }

        [HttpDelete("{limitOrderId}")]
        public async Task<IActionResult> CancelAsync(Guid limitOrderId)
        {
            await _limitOrderOperations.CancelAsync(limitOrderId);
            return NoContent();
        }
    }
}
