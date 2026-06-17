using FinanceManagement.Application.TaxCode.Commands.CreateTaxCodeMaster;
using FinanceManagement.Application.TaxCode.Commands.UpdateTaxCodeMaster;
using FinanceManagement.Application.TaxCode.Queries.GetAllTaxCodeMaster;
using FinanceManagement.Application.TaxCode.Queries.GetTaxCodeGlMappingSummary;
using FinanceManagement.Application.TaxCode.Queries.GetTaxCodeMasterAutoComplete;
using FinanceManagement.Application.TaxCode.Queries.GetTaxCodeMasterById;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace FinanceManagement.Presentation.Controllers
{
    // US-GL02-05A — Tax Code & TDS Section Master (catalogue + effective-dated rates).
    [Route("api/finance/[controller]")]
    public class TaxCodeMasterController : ApiControllerBase
    {
        public TaxCodeMasterController(IMediator mediator) : base(mediator) { }

        [HttpGet]
        public async Task<IActionResult> GetAllTaxCodesAsync(
            [FromQuery] int PageNumber,
            [FromQuery] int PageSize,
            [FromQuery] string? SearchTerm = null,
            [FromQuery] string? TaxType = null)
        {
            var result = await Mediator.Send(new GetAllTaxCodeMasterQuery
            {
                PageNumber = PageNumber,
                PageSize = PageSize,
                SearchTerm = SearchTerm,
                TaxType = TaxType
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

        // Tax Code Registry summary — each tax code + current rate + GL-account mapping count.
        [HttpGet("summary")]
        public async Task<IActionResult> GetTaxCodeGlMappingSummaryAsync(
            [FromQuery] int PageNumber,
            [FromQuery] int PageSize,
            [FromQuery] string? SearchTerm = null,
            [FromQuery] string? TaxType = null)
        {
            var result = await Mediator.Send(new GetTaxCodeGlMappingSummaryQuery
            {
                PageNumber = PageNumber,
                PageSize = PageSize,
                SearchTerm = SearchTerm,
                TaxType = TaxType
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
        public async Task<IActionResult> GetTaxCodeByIdAsync(int id)
        {
            var result = await Mediator.Send(new GetTaxCodeMasterByIdQuery { Id = id });
            return Ok(new { StatusCode = StatusCodes.Status200OK, data = result });
        }

        [HttpGet("by-name")]
        public async Task<IActionResult> GetTaxCodeAutoCompleteAsync(
            [FromQuery] string? term = null,
            [FromQuery] string? TaxType = null)
        {
            var result = await Mediator.Send(new GetTaxCodeMasterAutoCompleteQuery(term ?? string.Empty, TaxType));
            return Ok(new { StatusCode = StatusCodes.Status200OK, data = result });
        }

        [HttpPost]
        public async Task<IActionResult> CreateTaxCode([FromBody] CreateTaxCodeMasterCommand command)
        {
            var result = await Mediator.Send(command);
            return Ok(new { StatusCode = StatusCodes.Status200OK, isSuccess = result.IsSuccess, message = result.Message, data = result.Data });
        }

        [HttpPut]
        public async Task<IActionResult> UpdateTaxCode([FromBody] UpdateTaxCodeMasterCommand command)
        {
            var result = await Mediator.Send(command);
            return Ok(new { StatusCode = StatusCodes.Status200OK, isSuccess = result.IsSuccess, message = result.Message, data = result.Data });
        }
    }
}
