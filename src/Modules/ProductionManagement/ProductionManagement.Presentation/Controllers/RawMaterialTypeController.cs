using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ProductionManagement.Application.RawMaterialType.Commands.CreateRawMaterialType;
using ProductionManagement.Application.RawMaterialType.Commands.DeleteRawMaterialType;
using ProductionManagement.Application.RawMaterialType.Commands.UpdateRawMaterialType;
using ProductionManagement.Application.RawMaterialType.Queries.GetAllRawMaterialType;
using ProductionManagement.Application.RawMaterialType.Queries.GetRawMaterialTypeAutoComplete;
using ProductionManagement.Application.RawMaterialType.Queries.GetRawMaterialTypeById;

namespace ProductionManagement.Presentation.Controllers
{
    [Route("api/[controller]")]
    public class RawMaterialTypeController : ApiControllerBase
    {
        public RawMaterialTypeController(IMediator mediator) : base(mediator) { }

        [HttpGet]
        public async Task<IActionResult> GetAllRawMaterialTypeAsync(
            [FromQuery] int PageNumber,
            [FromQuery] int PageSize,
            [FromQuery] string? SearchTerm = null)
        {
            var result = await Mediator.Send(new GetAllRawMaterialTypeQuery
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
        public async Task<IActionResult> GetRawMaterialTypeByIdAsync(int id)
        {
            var result = await Mediator.Send(new GetRawMaterialTypeByIdQuery { Id = id });
            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                data = result
            });
        }

        [HttpGet("by-name")]
        public async Task<IActionResult> GetRawMaterialTypeAutoCompleteAsync([FromQuery] string? term = null)
        {
            var result = await Mediator.Send(new GetRawMaterialTypeAutoCompleteQuery(term));
            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                data = result
            });
        }

        [HttpPost]
        public async Task<IActionResult> CreateRawMaterialType([FromBody] CreateRawMaterialTypeCommand command)
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
        public async Task<IActionResult> UpdateRawMaterialType([FromBody] UpdateRawMaterialTypeCommand command)
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
        public async Task<IActionResult> DeleteRawMaterialType([FromQuery] int id)
        {
            var result = await Mediator.Send(new DeleteRawMaterialTypeCommand(id));
            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                isSuccess = result,
                message = result ? "Raw Material Type deleted successfully." : "Raw Material Type not found."
            });
        }
    }
}
