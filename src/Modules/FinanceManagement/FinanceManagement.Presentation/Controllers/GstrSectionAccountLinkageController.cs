using FinanceManagement.Application.TaxCode.Commands.CreateGstrSectionAccountLinkage;
using FinanceManagement.Application.TaxCode.Commands.DeleteGstrSectionAccountLinkage;
using FinanceManagement.Application.TaxCode.Commands.UpdateGstrSectionAccountLinkage;
using FinanceManagement.Application.TaxCode.Queries.GetAllGstrSectionAccountLinkage;
using FinanceManagement.Application.TaxCode.Queries.GetGstrSectionAccountLinkageById;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace FinanceManagement.Presentation.Controllers
{
    // GSTR-1 / GSTR-3B Account-Range Mapping (section -> GL account range, Derived vs Expected ± Tolerance).
    [Route("api/finance/[controller]")]
    public class GstrSectionAccountLinkageController : ApiControllerBase
    {
        public GstrSectionAccountLinkageController(IMediator mediator) : base(mediator) { }

        [HttpGet]
        public async Task<IActionResult> GetAllAsync(
            [FromQuery] int PageNumber,
            [FromQuery] int PageSize,
            [FromQuery] string? SearchTerm = null)
        {
            var result = await Mediator.Send(new GetAllGstrSectionAccountLinkageQuery
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

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetByIdAsync(int id)
        {
            var result = await Mediator.Send(new GetGstrSectionAccountLinkageByIdQuery { Id = id });
            return Ok(new { StatusCode = StatusCodes.Status200OK, data = result });
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateGstrSectionAccountLinkageCommand command)
        {
            var result = await Mediator.Send(command);
            return Ok(new { StatusCode = StatusCodes.Status200OK, isSuccess = result.IsSuccess, message = result.Message, data = result.Data });
        }

        [HttpPut]
        public async Task<IActionResult> Update([FromBody] UpdateGstrSectionAccountLinkageCommand command)
        {
            var result = await Mediator.Send(command);
            return Ok(new { StatusCode = StatusCodes.Status200OK, isSuccess = result.IsSuccess, message = result.Message, data = result.Data });
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await Mediator.Send(new DeleteGstrSectionAccountLinkageCommand(id));
            return Ok(new { StatusCode = StatusCodes.Status200OK, isSuccess = result, message = result ? "Mapping deleted successfully." : "Failed to delete mapping." });
        }
    }
}
