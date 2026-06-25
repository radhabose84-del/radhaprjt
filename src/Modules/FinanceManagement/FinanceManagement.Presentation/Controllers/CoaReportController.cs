using FinanceManagement.Application.CoaReport.Queries.GetAccountUsageReport;
using FinanceManagement.Application.CoaReport.Queries.GetCoaListing;
using FinanceManagement.Application.CoaReport.Queries.GetCoaListingPdf;
using FinanceManagement.Application.CoaReport.Queries.GetFsMappingValidation;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace FinanceManagement.Presentation.Controllers
{
    // US-GL02-15 COA Listing & Structure Reports. Read-only; company from session. No configuration changes.
    [Route("api/finance/coa-report")]
    public class CoaReportController : ApiControllerBase
    {
        public CoaReportController(IMediator mediator) : base(mediator) { }

        // AC1 — COA listing grid (hierarchy + attributes + posting count + FS-mapping).
        [HttpGet("listing")]
        public async Task<IActionResult> GetListingAsync(
            [FromQuery] int? AccountTypeId = null,
            [FromQuery] int? AccountGroupId = null,
            [FromQuery] bool ActiveOnly = false,
            [FromQuery] string? SearchTerm = null)
        {
            var result = await Mediator.Send(new GetCoaListingQuery
            {
                AccountTypeId = AccountTypeId,
                AccountGroupId = AccountGroupId,
                ActiveOnly = ActiveOnly,
                SearchTerm = SearchTerm
            });
            return Ok(new { StatusCode = StatusCodes.Status200OK, data = result.Data, TotalCount = result.TotalCount });
        }

        // AC1/AC5 — auditor-ready COA listing PDF.
        [HttpGet("listing/pdf")]
        public async Task<IActionResult> GetListingPdfAsync(
            [FromQuery] int? AccountTypeId = null,
            [FromQuery] int? AccountGroupId = null,
            [FromQuery] bool ActiveOnly = false,
            [FromQuery] string? SearchTerm = null)
        {
            var file = await Mediator.Send(new GetCoaListingPdfQuery
            {
                AccountTypeId = AccountTypeId,
                AccountGroupId = AccountGroupId,
                ActiveOnly = ActiveOnly,
                SearchTerm = SearchTerm
            });
            return File(file.Content, file.ContentType, file.FileName);
        }

        // AC2/AC3 — account-usage / deactivation-candidate report.
        [HttpGet("account-usage")]
        public async Task<IActionResult> GetAccountUsageAsync([FromQuery] int MonthsSincePosting = 12)
        {
            var result = await Mediator.Send(new GetAccountUsageReportQuery { MonthsSincePosting = MonthsSincePosting });
            return Ok(new { StatusCode = StatusCodes.Status200OK, data = result.Data, TotalCount = result.TotalCount });
        }

        // AC4 — FS-mapping (Schedule III) validation before go-live.
        [HttpGet("fs-mapping-validation")]
        public async Task<IActionResult> GetFsMappingValidationAsync()
        {
            var result = await Mediator.Send(new GetFsMappingValidationQuery());
            return Ok(new { StatusCode = StatusCodes.Status200OK, isSuccess = result.IsSuccess, message = result.Message, data = result.Data });
        }
    }
}
