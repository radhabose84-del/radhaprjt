using PartyManagement.Application.GST.Queries;
using PartyManagement.Application.Interfaces.GST;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;

namespace WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class GSTController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly IGSTAuthService _gstService;
        public GSTController(IMediator mediator, IGSTAuthService gstService)
        {
            _mediator = mediator;
            _gstService = gstService;
        }
         [HttpGet("auth")]
        public async Task<IActionResult> GetAuthToken()
        {
            var result = await _gstService.GetAuthTokenAsync();
            return Ok(result);
        }

        [HttpGet("gstin/{gstin}")]
        public async Task<IActionResult> GetGSTDetails(string gstin)
        {
            //var result = await _mediator.Send(new GetGSTINDetailsQuery(gstin));
            //return Ok(result);
            var result = await _mediator.Send(new GetGSTINDetailsQuery(gstin));           
           if (result == null)
            {
                return NotFound(new
                {
                    StatusCode = StatusCodes.Status404NotFound,
                    message = "No records found for the given GSTIN.",
                    data = (object)null
                });
            }

            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                message = "Fetched successfully.",
                data = result
            });   
        }
    }
}
