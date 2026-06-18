using FinanceManagement.Application.GlAccountImport.Commands.ActivateGlAccountImportBatch;
using FinanceManagement.Application.GlAccountImport.Commands.ImportGlAccounts;
using FinanceManagement.Application.GlAccountImport.Queries.DownloadGlAccountTemplate;
using FinanceManagement.Application.GlAccountImport.Queries.ExportGlAccounts;
using FinanceManagement.Application.GlAccountImport.Queries.GetGlAccountImportErrors;
using FinanceManagement.Application.GlAccountImport.Queries.GetGlAccountImportLogs;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace FinanceManagement.Presentation.Controllers
{
    /// <summary>COA bulk import / export endpoints (GL02-FR-006).</summary>
    [Route("api/finance/[controller]")]
    public class GlAccountImportController : ApiControllerBase
    {
        public GlAccountImportController(IMediator mediator) : base(mediator) { }

        // Download the validated template (header + sample GROUP/ACCOUNT rows).
        [HttpGet("template")]
        public async Task<IActionResult> DownloadTemplateAsync([FromQuery] string? format = null)
        {
            var file = await Mediator.Send(new DownloadGlAccountTemplateQuery(format));
            return File(file.Content, file.ContentType, file.FileName);
        }

        // Full COA export (re-imports cleanly — AC5).
        [HttpGet("export")]
        public async Task<IActionResult> ExportAsync([FromQuery] string? format = null)
        {
            var file = await Mediator.Send(new ExportGlAccountsQuery(format));
            return File(file.Content, file.ContentType, file.FileName);
        }

        // Validate + import. Mode = "AllOrNothing" (default) or "ValidRowsOnly".
        [HttpPost]
        public async Task<IActionResult> ImportAsync(IFormFile? file, [FromForm] string? mode = null)
        {
            var result = await Mediator.Send(new ImportGlAccountsCommand { File = file, Mode = mode });

            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                isSuccess = result.IsSuccess,
                message = result.Message,
                data = result.Data
            });
        }

        // Activate every account created by an import batch (AC3 bulk activate).
        [HttpPost("activate-batch/{importLogId}")]
        public async Task<IActionResult> ActivateBatchAsync(int importLogId)
        {
            var result = await Mediator.Send(new ActivateGlAccountImportBatchCommand(importLogId));

            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                isSuccess = result.IsSuccess,
                message = result.Message,
                data = result.Data
            });
        }

        // Import-run history.
        [HttpGet("logs")]
        public async Task<IActionResult> GetLogsAsync([FromQuery] int PageNumber = 1, [FromQuery] int PageSize = 20)
        {
            var result = await Mediator.Send(new GetGlAccountImportLogsQuery { PageNumber = PageNumber, PageSize = PageSize });

            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                data = result.Data,
                TotalCount = result.TotalCount,
                PageNumber = result.PageNumber,
                PageSize = result.PageSize
            });
        }

        // Row-level error report for one import run.
        [HttpGet("logs/{importLogId}/errors")]
        public async Task<IActionResult> GetErrorsAsync(int importLogId)
        {
            var result = await Mediator.Send(new GetGlAccountImportErrorsQuery(importLogId));

            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                isSuccess = result.IsSuccess,
                message = result.Message,
                data = result.Data,
                TotalCount = result.TotalCount
            });
        }
    }
}
