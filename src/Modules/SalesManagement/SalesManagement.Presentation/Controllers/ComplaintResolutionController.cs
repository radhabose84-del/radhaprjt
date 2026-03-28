using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SalesManagement.Application.ComplaintResolution.Commands.SubmitResolution;
using SalesManagement.Application.ComplaintResolution.Commands.UpdateResolution;
using SalesManagement.Application.ComplaintResolution.Queries.GetResolutionByComplaintId;

namespace SalesManagement.Presentation.Controllers
{
    [Route("api/[controller]")]
    public class ComplaintResolutionController : ApiControllerBase
    {
        public ComplaintResolutionController(IMediator mediator) : base(mediator) { }

        [HttpGet("by-complaint/{complaintHeaderId}")]
        public async Task<IActionResult> GetByComplaintIdAsync(int complaintHeaderId)
        {
            var result = await Mediator.Send(new GetResolutionByComplaintIdQuery
            {
                ComplaintHeaderId = complaintHeaderId
            });

            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                isSuccess = result != null,
                message = result != null ? "Resolution found." : "No resolution found for this complaint.",
                data = result
            });
        }

        [HttpPost]
        public async Task<IActionResult> SubmitResolution([FromBody] SubmitResolutionCommand command)
        {
            var result = await Mediator.Send(command);
            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                isSuccess = result.IsSuccess,
                message = result.Message,
                data = result.Data
            });
        }

        [HttpPut]
        public async Task<IActionResult> UpdateResolution([FromBody] UpdateResolutionCommand command)
        {
            var result = await Mediator.Send(command);
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
