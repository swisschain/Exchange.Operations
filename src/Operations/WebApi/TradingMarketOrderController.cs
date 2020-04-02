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
    [Route("api/trading/market-order")]
    public class TradingMarketOrderController : ControllerBase
    {
        private readonly IMarketOrderOperations _marketOrderOperations;

        public TradingMarketOrderController(IMarketOrderOperations marketOrderOperations)
        {
            _marketOrderOperations = marketOrderOperations;
        }

        [HttpPost]
        [ProducesResponseType(typeof(CreateMarketOrderResponse), StatusCodes.Status200OK)]
        public async Task<IActionResult> CreateAsync([FromBody] MarketOrderCreateModel model)
        {
            var result = await _marketOrderOperations.CreateAsync(model);

            return Ok(result);
        }
    }
}
