using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using GateEntryManagement.Application.GatePass.Commands.CreateGatePass;
using GateEntryManagement.Application.GatePass.Commands.DeleteGatePass;
using GateEntryManagement.Application.GatePass.Queries.GetAllGatePass;
using GateEntryManagement.Application.GatePass.Queries.GetGatePassById;
using GateEntryManagement.Application.GatePass.Queries.GetGatePassAutoComplete;

namespace GateEntryManagement.Presentation.Controllers
{
    [Route("api/gateentry/[controller]")]
    public class GatePassController : ApiControllerBase
    {
        public GatePassController(ISender mediator) : base(mediator) { }

        [HttpGet]
        public async Task<IActionResult> GetAllGatePassAsync(
            [FromQuery] int PageNumber,
            [FromQuery] int PageSize,
            [FromQuery] string? SearchTerm = null)
        {
            var result = await Mediator.Send(new GetAllGatePassQuery
            {
                PageNumber = PageNumber,
                PageSize = PageSize,
                SearchTerm = SearchTerm
            });

            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                data = result.Data,
                TotalCount = result.TotalCount,
                PageNumber = result.PageNumber,
                PageSize = result.PageSize
            });
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetGatePassByIdAsync(int id)
        {
            var result = await Mediator.Send(new GetGatePassByIdQuery { Id = id });

            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                data = result
            });
        }

        [HttpGet("by-name")]
        public async Task<IActionResult> GetGatePassAutoCompleteAsync([FromQuery] string? term = null)
        {
            var result = await Mediator.Send(new GetGatePassAutoCompleteQuery(term ?? string.Empty));

            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                data = result
            });
        }

        [HttpPost]
        public async Task<IActionResult> CreateGatePass([FromBody] CreateGatePassCommand command)
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

        [HttpDelete]
        public async Task<IActionResult> DeleteGatePass(int id)
        {
            var result = await Mediator.Send(new DeleteGatePassCommand(id));

            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                isSuccess = result,
                message = result ? "Gate Pass deleted successfully." : "Failed to delete Gate Pass."
            });
        }
    }
}
