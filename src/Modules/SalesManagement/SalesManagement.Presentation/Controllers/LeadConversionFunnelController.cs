using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SalesManagement.Application.LeadConversionFunnel.Queries.GetLeadConversionFunnel;

namespace SalesManagement.Presentation.Controllers
{
    [Route("api/[controller]")]
    public class LeadConversionFunnelController : ApiControllerBase
    {
        public LeadConversionFunnelController(ISender mediator) : base(mediator) { }

        [HttpGet]
        public async Task<IActionResult> GetLeadConversionFunnelAsync()
        {
            var result = await Mediator.Send(new GetLeadConversionFunnelQuery());

            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                isSuccess = result.IsSuccess,
                message = result.Message,
                data = result.Data
            });
        }
    }
}
