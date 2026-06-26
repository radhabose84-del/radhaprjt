using FinanceManagement.Application.JournalMaster.LogAnalysis.Queries.GetAllLogAnalysis;
using FinanceManagement.Application.JournalMaster.LogAnalysis.Queries.GetLogAnalysisSummary;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace FinanceManagement.Presentation.Controllers.JournalMaster
{
    // Unified log-analysis feed across the four Journal log sources:
    // SecurityViolation, SequenceGap, RecurringGeneration, JournalFlag.
    [Route("api/finance/[controller]")]
    public class LogAnalysisController : FinanceManagement.Presentation.Controllers.ApiControllerBase
    {
        public LogAnalysisController(IMediator mediator) : base(mediator) { }

        [HttpGet]
        public async Task<IActionResult> GetAllAsync(
            [FromQuery] int PageNumber = 1,
            [FromQuery] int PageSize = 50,
            [FromQuery] string? LogType = null,
            [FromQuery] DateTimeOffset? DateFrom = null,
            [FromQuery] DateTimeOffset? DateTo = null)
        {
            var result = await Mediator.Send(new GetAllLogAnalysisQuery
            {
                PageNumber = PageNumber,
                PageSize = PageSize,
                LogType = LogType,
                DateFrom = DateFrom,
                DateTo = DateTo
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

        // Per-source counts for the header cards.
        [HttpGet("summary")]
        public async Task<IActionResult> GetSummaryAsync(
            [FromQuery] DateTimeOffset? DateFrom = null,
            [FromQuery] DateTimeOffset? DateTo = null)
        {
            var result = await Mediator.Send(new GetLogAnalysisSummaryQuery(DateFrom, DateTo));
            return Ok(new { StatusCode = StatusCodes.Status200OK, data = result });
        }
    }
}
