using InventoryManagement.Application.ProcurementType.Commands.CreateProcurementType;
using InventoryManagement.Application.ProcurementType.Commands.DeleteProcurementType;
using InventoryManagement.Application.ProcurementType.Commands.UpdateProcurementType;
using InventoryManagement.Application.ProcurementType.Queries.GetAllProcurementType;
using InventoryManagement.Application.ProcurementType.Queries.GetProcurementTypeAutoComplete;
using InventoryManagement.Application.ProcurementType.Queries.GetProcurementTypeById;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace InventoryManagement.Presentation.Controllers
{
    [Route("api/[controller]")]
    public class ProcurementTypeController : ApiControllerBase
    {
        public ProcurementTypeController(ISender mediator) : base(mediator) { }

        [HttpGet]
        public async Task<IActionResult> GetAllProcurementTypeAsync(
            [FromQuery] int PageNumber,
            [FromQuery] int PageSize,
            [FromQuery] string? SearchTerm = null)
        {
            var result = await Mediator.Send(new GetAllProcurementTypeQuery
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
        public async Task<IActionResult> GetProcurementTypeByIdAsync(int id)
        {
            var result = await Mediator.Send(new GetProcurementTypeByIdQuery { Id = id });
            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                data = result
            });
        }

        [HttpGet("by-name")]
        public async Task<IActionResult> GetProcurementTypeAutoCompleteAsync([FromQuery] string? term = null)
        {
            var result = await Mediator.Send(new GetProcurementTypeAutoCompleteQuery(term));
            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                data = result
            });
        }

        [HttpPost]
        public async Task<IActionResult> CreateProcurementType([FromBody] CreateProcurementTypeCommand command)
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
        public async Task<IActionResult> UpdateProcurementType([FromBody] UpdateProcurementTypeCommand command)
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
        public async Task<IActionResult> DeleteProcurementType(int id)
        {
            var result = await Mediator.Send(new DeleteProcurementTypeCommand(id));
            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                isSuccess = result,
                message = result ? "ProcurementType deleted successfully." : "ProcurementType not found."
            });
        }
    }
}
