﻿using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Operations.DomainService;
using Operations.DomainService.Model;
using Swisschain.Sdk.Server.Authorization;

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
        public async Task<IActionResult> CashInAsync([FromBody] CashOperationModel model)
        {
            var response = await _cashOperations.CashInAsync(User.GetTenantId(), model);

            return Ok(response);
        }

        [HttpPost("cash-out")]
        [ProducesResponseType(typeof(OperationResponse), StatusCodes.Status200OK)]
        public async Task<IActionResult> CashOutAsync([FromBody] CashOperationModel model)
        {
            var response = await _cashOperations.CashOutAsync(User.GetTenantId(), model);

            return Ok(response);
        }
    }
}
