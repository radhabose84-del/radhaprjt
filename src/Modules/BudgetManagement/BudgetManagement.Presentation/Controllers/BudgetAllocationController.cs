using BudgetManagement.Application.BudgetAllocation.Command.Create;
using BudgetManagement.Application.BudgetAllocation.Queries.GetBudgetBalanceReport;
using BudgetManagement.Application.BudgetAllocation.Queries.GetRemainingBalance;
using BudgetManagement.Application.BudgetAllocation.Queries.GetSpindleDetailsMonthwise;
using BudgetManagement.Application.BudgetAllocation.Queries.GetSpindleMonthwiseReport;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;

namespace BudgetManagement.Presentation.Controllers
{

    [Route("api/[controller]")]
    public class BudgetAllocationController : ApiControllerBase
    {

        private readonly IMediator _mediator;

        public BudgetAllocationController(IMediator mediator)
        : base(mediator)
        {
            _mediator = mediator;

        }
        [HttpGet("SpindleDetailsMonthwise")]
        public async Task<IActionResult> GetSpindleDetailsMonthwiseAsync(
            int PageNumber,
            [FromQuery] int PageSize,
            [FromQuery] string? SearchTerm = null)
        {
            // Send query to mediator
            var response = await _mediator.Send(new GetSpindleDetailsMonthwiseQuery
            {
                PageNumber = PageNumber,
                PageSize = PageSize,
                SearchTerm = SearchTerm
            });

            if (response == null || response.Data == null || !response.Data.Any())
            {
                return NotFound(new
                {
                    StatusCode = StatusCodes.Status404NotFound,
                    data = (object?)null,
                    message = "No spindle allocation details found"
                });
            }

            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                data = response.Data,
                TotalCount = response.TotalCount,
                PageNumber = response.PageNumber,
                PageSize = response.PageSize
            });
        }


        [HttpPost]
        public async Task<IActionResult> CreateAsync(CreateBudgetAllocationCommand createBudgetAllocationCommand)
        {
            var CreateBudgetAllocation = await _mediator.Send(createBudgetAllocationCommand);

            return Ok(new
            {
                StatusCode = StatusCodes.Status201Created,
                message = "Created successfully.",
                data = CreateBudgetAllocation
            });
        }
        [HttpGet("SpindleDetailsMonthwiseReport/{financialYearId}")]
        public async Task<IActionResult> GetSpindleDetailsMonthwiseReport(
              int financialYearId,
              [FromQuery] int? departmentId,
              [FromQuery] int? costCenterId,
              [FromQuery] int? allocationTypeId,
              [FromQuery] int? budgetGroupId,
              [FromQuery] DateOnly? budgetDate)
        {
            var query = new GetSpindleMonthwiseReportQuery
            {
                FinancialYearId = financialYearId,
                DepartmentId = departmentId,
                CostCenterId = costCenterId,
                AllocationTypeId = allocationTypeId,
                BudgetGroupId = budgetGroupId,
                BudgetDate = budgetDate
            };

            var data = await _mediator.Send(query);

            if (data == null || data.Count == 0)
                return NotFound(new
                {
                    StatusCode = StatusCodes.Status404NotFound,
                    Message = "No Results found",
                    Data = new List<GetSpindleMonthwiseReportDto>()
                });

            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                Data = data,
                Message = "Details fetched successfully"
            });

        }
        [HttpGet("remainingbalance")]
        public async Task<IActionResult> GetRemainingBalance(
            [FromQuery] int budgetGroupId,
            [FromQuery] string? date,
            [FromQuery] int? monthId,
            [FromQuery] int? requestById, [FromQuery] int? ProjectId, [FromQuery] int? WbsId,
            [FromQuery] int? financialYearId,
            CancellationToken ct)
        {
            DateOnly? budgetDate = null;

            if (!string.IsNullOrWhiteSpace(date))
            {
                if (!DateOnly.TryParse(date, out var parsed))
                    return BadRequest("Invalid date format. Expected yyyy-MM-dd");

                budgetDate = parsed;
            }

            var dto = await _mediator.Send(new GetRemainingBalanceQuery
            {
                BudgetGroupId = budgetGroupId,
                BudgetDate = budgetDate,
                RequestById = requestById,
                MonthId = monthId,
                ProjectId = ProjectId,
                WbsId = WbsId,
                FinancialYearId = financialYearId
            }, ct);

            return Ok(new
            {
                dto.BudgetGroupId,
                BudgetDate = dto.BudgetDate?.ToString("yyyy-MM-dd"),
                dto.RequestById,
                dto.MonthId,
                dto.CurrentRemainingBalance,
                dto.PreviousRemainingBalance
            });
        }
       [HttpGet("BudgetBalanceReport/{financialYearId}")]
        public async Task<IActionResult> GetBudgetBalanceReport(
            int financialYearId)
        {
            var query = new GetBudgetBalanceReportQuery
            {
                FinancialYearId = financialYearId
                
            };

            var data = await _mediator.Send(query);

            if (data == null || data.Count == 0)
            {
                return NotFound(new
                {
                    StatusCode = StatusCodes.Status404NotFound,
                    Message = "No Results found",
                    Data = new List<BudgetBalanceReportDto>()
                });
            }

            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                Data = data,
                Message = "Budget balance report fetched successfully"
            });
        }




    }
}