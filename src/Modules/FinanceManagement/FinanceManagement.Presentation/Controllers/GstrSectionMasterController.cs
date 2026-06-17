using FinanceManagement.Application.TaxCode.Commands.CreateGstrSectionMaster;
using FinanceManagement.Application.TaxCode.Commands.DeleteGstrSectionMaster;
using FinanceManagement.Application.TaxCode.Commands.UpdateGstrSectionMaster;
using FinanceManagement.Application.TaxCode.Queries.GetAllGstrSectionMaster;
using FinanceManagement.Application.TaxCode.Queries.GetGstrSectionMasterAutoComplete;
using FinanceManagement.Application.TaxCode.Queries.GetGstrSectionMasterById;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace FinanceManagement.Presentation.Controllers
{
    // GSTR-1 / GSTR-3B return section master (Report from MiscMaster + Section code/name).
    [Route("api/finance/[controller]")]
    public class GstrSectionMasterController : ApiControllerBase
    {
        public GstrSectionMasterController(IMediator mediator) : base(mediator) { }

        [HttpGet]
        public async Task<IActionResult> GetAllAsync(
            [FromQuery] int PageNumber,
            [FromQuery] int PageSize,
            [FromQuery] string? SearchTerm = null,
            [FromQuery] int? ReportTypeId = null)
        {
            var result = await Mediator.Send(new GetAllGstrSectionMasterQuery
            {
                PageNumber = PageNumber,
                PageSize = PageSize,
                SearchTerm = SearchTerm,
                ReportTypeId = ReportTypeId
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

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetByIdAsync(int id)
        {
            var result = await Mediator.Send(new GetGstrSectionMasterByIdQuery { Id = id });
            return Ok(new { StatusCode = StatusCodes.Status200OK, data = result });
        }

        [HttpGet("by-name")]
        public async Task<IActionResult> GetAutoCompleteAsync(
            [FromQuery] string? term = null,
            [FromQuery] int? ReportTypeId = null)
        {
            var result = await Mediator.Send(new GetGstrSectionMasterAutoCompleteQuery(term ?? string.Empty, ReportTypeId));
            return Ok(new { StatusCode = StatusCodes.Status200OK, data = result.Data });
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateGstrSectionMasterCommand command)
        {
            var result = await Mediator.Send(command);
            return Ok(new { StatusCode = StatusCodes.Status200OK, isSuccess = result.IsSuccess, message = result.Message, data = result.Data });
        }

        [HttpPut]
        public async Task<IActionResult> Update([FromBody] UpdateGstrSectionMasterCommand command)
        {
            var result = await Mediator.Send(command);
            return Ok(new { StatusCode = StatusCodes.Status200OK, isSuccess = result.IsSuccess, message = result.Message, data = result.Data });
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await Mediator.Send(new DeleteGstrSectionMasterCommand(id));
            return Ok(new { StatusCode = StatusCodes.Status200OK, isSuccess = result, message = result ? "GSTR section deleted successfully." : "Failed to delete GSTR section." });
        }
    }
}
