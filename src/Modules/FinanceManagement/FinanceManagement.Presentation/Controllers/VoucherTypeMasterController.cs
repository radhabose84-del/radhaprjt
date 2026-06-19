using FinanceManagement.Application.VoucherType.Commands.CreateVoucherType;
using FinanceManagement.Application.VoucherType.Commands.DeleteVoucherType;
using FinanceManagement.Application.VoucherType.Commands.ResetVoucherTypeSeries;
using FinanceManagement.Application.VoucherType.Commands.UpdateVoucherType;
using FinanceManagement.Application.VoucherType.Queries.GetAllVoucherType;
using FinanceManagement.Application.VoucherType.Queries.GetVoucherTypeAutoComplete;
using FinanceManagement.Application.VoucherType.Queries.GetVoucherTypeById;
using FinanceManagement.Application.VoucherType.Queries.GetVoucherTypeNumberSeries;
using FinanceManagement.Application.VoucherType.Queries.GetVoucherTypeSummary;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace FinanceManagement.Presentation.Controllers
{
    [Route("api/finance/[controller]")]
    public class VoucherTypeMasterController : ApiControllerBase
    {
        public VoucherTypeMasterController(IMediator mediator) : base(mediator) { }

        [HttpGet]
        public async Task<IActionResult> GetAllVoucherTypeAsync(
            [FromQuery] int PageNumber,
            [FromQuery] int PageSize,
            [FromQuery] string? SearchTerm = null,
            [FromQuery] int? FinancialYearId = null)
        {
            var result = await Mediator.Send(new GetAllVoucherTypeQuery
            {
                PageNumber = PageNumber,
                PageSize = PageSize,
                SearchTerm = SearchTerm,
                FinancialYearId = FinancialYearId
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

        [HttpGet("summary")]
        public async Task<IActionResult> GetVoucherTypeSummaryAsync()
        {
            var result = await Mediator.Send(new GetVoucherTypeSummaryQuery());

            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                data = result
            });
        }

        [HttpGet("number-series")]
        public async Task<IActionResult> GetVoucherTypeNumberSeriesAsync(
            [FromQuery] int FinancialYearId)
        {
            var result = await Mediator.Send(new GetVoucherTypeNumberSeriesQuery
            {
                FinancialYearId = FinancialYearId
            });

            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                data = result.Data,
                TotalCount = result.TotalCount
            });
        }

        [HttpGet("by-name")]
        public async Task<IActionResult> GetVoucherTypeAutoCompleteAsync(
            [FromQuery] string? term = null)
        {
            var result = await Mediator.Send(new GetVoucherTypeAutoCompleteQuery(term ?? string.Empty));

            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                data = result
            });
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetVoucherTypeByIdAsync(int id)
        {
            var result = await Mediator.Send(new GetVoucherTypeByIdQuery { Id = id });

            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                data = result
            });
        }

        [HttpPost]
        public async Task<IActionResult> CreateVoucherType([FromBody] CreateVoucherTypeCommand command)
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
        public async Task<IActionResult> UpdateVoucherType([FromBody] UpdateVoucherTypeCommand command)
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

        [HttpPost("reset-series")]
        public async Task<IActionResult> ResetVoucherTypeSeries([FromBody] ResetVoucherTypeSeriesCommand command)
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
        public async Task<IActionResult> DeleteVoucherType(int id)
        {
            var result = await Mediator.Send(new DeleteVoucherTypeCommand(id));

            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                isSuccess = result,
                message = result ? "Voucher Type deleted successfully." : "Failed to delete Voucher Type."
            });
        }
    }
}
