using System.Threading.Tasks;
using MatchingEngine.Client;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Operations.WebApi
{
    [Authorize]
    [ApiController]
    [Route("api/temp")]
    public class TempController : ControllerBase
    {
        private readonly IMatchingEngineClient _matchingEngineClient;

        public TempController(IMatchingEngineClient matchingEngineClient)
        {
            _matchingEngineClient = matchingEngineClient;
        }

        [HttpGet("balance/{walletId}")]
        public async Task<IActionResult> GetBalance([FromRoute] string walletId)
        {
            var balances = await _matchingEngineClient.Balances.GetAllAsync(walletId);

            return Ok(balances);
        }
    }
}
