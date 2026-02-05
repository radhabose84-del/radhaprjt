using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using PurchaseManagement.Application.PartyMaster.Queries.GetPartyDetails;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace PurchaseManagement.API.Controllers
{
    [Route("api/purchase/[controller]")]
    public class PartyMasterController : ApiControllerBase
    {
        private readonly IMediator _mediator;

        public PartyMasterController(IMediator mediator)
        : base(mediator)
        {
            _mediator = mediator;
        }
        
        [HttpGet("GetPartyDetails")]
        public async Task<IActionResult> GetPartyDetails([FromQuery] string oldunitCode, [FromQuery] string searchPattern)
        {
            var result = await _mediator.Send(new GetPartyDetailsQuery
            {
                OldunitCode = oldunitCode,
                SearchPattern = searchPattern
            });

            return Ok(result);
        }

       
    }
}