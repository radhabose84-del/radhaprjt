using PurchaseManagement.Application.PartyMaster.Queries.GetPartyDetails;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace PurchaseManagement.Presentation.Controllers
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