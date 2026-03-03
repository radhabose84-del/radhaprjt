using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SalesManagement.Application.PackType.Commands.CreatePackType;
using SalesManagement.Application.PackType.Commands.UpdatePackType;
using SalesManagement.Application.PackType.Commands.DeletePackType;
using SalesManagement.Application.PackType.Queries.GetAllPackType;
using SalesManagement.Application.PackType.Queries.GetPackTypeById;
using SalesManagement.Application.PackType.Queries.GetPackTypeAutoComplete;

namespace SalesManagement.Presentation.Controllers
{
    [Route("api/[controller]")]
    public class PackTypeController : ApiControllerBase
    {
        public PackTypeController(IMediator mediator) : base(mediator) { }

        [HttpGet]
        public async Task<IActionResult> GetAllPackTypeAsync(
            [FromQuery] int PageNumber,
            [FromQuery] int PageSize,
            [FromQuery] string? SearchTerm = null)
        {
            var result = await Mediator.Send(new GetAllPackTypeQuery
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
        public async Task<IActionResult> GetPackTypeByIdAsync(int id)
        {
            var result = await Mediator.Send(new GetPackTypeByIdQuery { Id = id });

            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                data = result
            });
        }

        [HttpGet("by-name")]
        public async Task<IActionResult> GetPackTypeAutoCompleteAsync([FromQuery] string? term = null)
        {
            var result = await Mediator.Send(new GetPackTypeAutoCompleteQuery(term ?? string.Empty));

            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                data = result
            });
        }

        [HttpPost]
        public async Task<IActionResult> CreatePackType([FromBody] CreatePackTypeCommand command)
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
        public async Task<IActionResult> UpdatePackType([FromBody] UpdatePackTypeCommand command)
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
        public async Task<IActionResult> DeletePackType(int id)
        {
            var result = await Mediator.Send(new DeletePackTypeCommand(id));

            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                isSuccess = result,
                message = result ? "PackType deleted successfully." : "Failed to delete PackType."
            });
        }
    }
}
