using FinanceManagement.Application.FinancialYearMaster.Commands.CreateFinancialYearMaster;
using FinanceManagement.Application.FinancialYearMaster.Commands.DeleteFinancialYearMaster;
using FinanceManagement.Application.FinancialYearMaster.Commands.UpdateFinancialYearMaster;
using FinanceManagement.Application.FinancialYearMaster.Queries.GetAllFinancialYearMaster;
using FinanceManagement.Application.FinancialYearMaster.Queries.GetFinancialPeriodsForCompany;
using FinanceManagement.Application.FinancialYearMaster.Queries.GetFinancialYearMasterAutoComplete;
using FinanceManagement.Application.FinancialYearMaster.Queries.GetFinancialYearMasterById;
using FinanceManagement.Application.FinancialYearMaster.Queries.GetPeriodForDate;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace FinanceManagement.Presentation.Controllers
{
    [Route("api/finance/[controller]")]
    public class FinancialYearMasterController : ApiControllerBase
    {
        public FinancialYearMasterController(IMediator mediator) : base(mediator) { }

        [HttpGet]
        public async Task<IActionResult> GetAllFinancialYearMasterAsync(
            [FromQuery] int PageNumber,
            [FromQuery] int PageSize,
            [FromQuery] string? SearchTerm = null,
            [FromQuery] int? StatusId = null)
        {
            var result = await Mediator.Send(new GetAllFinancialYearMasterQuery
            {
                PageNumber = PageNumber,
                PageSize = PageSize,
                SearchTerm = SearchTerm,
                StatusId = StatusId
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
        public async Task<IActionResult> GetFinancialYearMasterByIdAsync(int id)
        {
            var result = await Mediator.Send(new GetFinancialYearMasterByIdQuery { Id = id });
            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                data = result
            });
        }

        // Task 5 / AC#5 — posting-engine read endpoint
        [HttpGet("{companyId:int}/periods")]
        public async Task<IActionResult> GetPeriodsForCompanyAsync(int companyId)
        {
            var result = await Mediator.Send(new GetFinancialPeriodsForCompanyQuery(companyId));
            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                data = result
            });
        }

        [HttpGet("period-for-date")]
        public async Task<IActionResult> GetPeriodForDateAsync([FromQuery] DateOnly date)
        {
            var result = await Mediator.Send(new GetPeriodForDateQuery(date));
            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                data = result
            });
        }

        [HttpGet("by-name")]
        public async Task<IActionResult> GetFinancialYearMasterAutoCompleteAsync([FromQuery] string? term = null)
        {
            var result = await Mediator.Send(new GetFinancialYearMasterAutoCompleteQuery(term ?? string.Empty));
            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                data = result
            });
        }

        [HttpPost]
        public async Task<IActionResult> CreateFinancialYearMaster([FromBody] CreateFinancialYearMasterCommand command)
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
        public async Task<IActionResult> UpdateFinancialYearMaster([FromBody] UpdateFinancialYearMasterCommand command)
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
        public async Task<IActionResult> DeleteFinancialYearMaster(int id)
        {
            var result = await Mediator.Send(new DeleteFinancialYearMasterCommand(id));
            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                isSuccess = result,
                message = result ? "Financial Year deleted successfully." : "Failed to delete Financial Year."
            });
        }
    }
}
