using FinanceManagement.Application.ScheduleIII.Commands.ImportScheduleIII;
using FinanceManagement.Application.ScheduleIII.Commands.ImportScheduleIIIFile;
using FinanceManagement.Application.ScheduleIII.Queries.GetScheduleIIIImportTemplate;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace FinanceManagement.Presentation.Controllers
{
    // Bulk import of Schedule III sections + line items (mirrors the Journal import).
    [Route("api/finance/[controller]")]
    public class ScheduleIIIImportController : ApiControllerBase
    {
        public ScheduleIIIImportController(IMediator mediator) : base(mediator) { }

        // Browse + upload an Excel/CSV file (multipart/form-data, field name "file").
        [HttpPost("upload")]
        public async Task<IActionResult> Upload(IFormFile? file)
        {
            var result = await Mediator.Send(new ImportScheduleIIIFileCommand { File = file });

            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                isSuccess = result.IsSuccess,
                message = result.Message,
                data = result.Data
            });
        }

        // Download the import template (.xlsx) with headers + sample rows.
        [HttpGet("template")]
        public async Task<IActionResult> DownloadTemplate()
        {
            var file = await Mediator.Send(new GetScheduleIIIImportTemplateQuery());
            return File(file.Content, file.ContentType, file.FileName);
        }

        // Import already-parsed rows (JSON).
        [HttpPost]
        public async Task<IActionResult> Import([FromBody] ImportScheduleIIICommand command)
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
    }
}
