using FinanceManagement.Application.JournalMaster.JournalImport.Commands.ImportJournals;
using FinanceManagement.Application.JournalMaster.JournalImport.Commands.ImportJournalsFile;
using FinanceManagement.Application.JournalMaster.JournalImport.Queries.GetAllJournalImportBatch;
using FinanceManagement.Application.JournalMaster.JournalImport.Queries.GetJournalImportBatchById;
using FinanceManagement.Application.JournalMaster.JournalImport.Queries.GetJournalImportTemplate;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace FinanceManagement.Presentation.Controllers.JournalMaster
{
    [Route("api/finance/[controller]")]
    public class JournalImportController : FinanceManagement.Presentation.Controllers.ApiControllerBase
    {
        public JournalImportController(IMediator mediator) : base(mediator) { }

        [HttpPost]
        public async Task<IActionResult> Import([FromBody] ImportJournalsCommand command)
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

        // Browse + upload an Excel/CSV file (multipart/form-data, field name "file").
        [HttpPost("upload")]
        public async Task<IActionResult> Upload(IFormFile? file)
        {
            var result = await Mediator.Send(new ImportJournalsFileCommand { File = file });

            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                isSuccess = result.IsSuccess,
                message = result.Message,
                data = result.Data
            });
        }

        // Download the import template (.xlsx) with headers + a sample balanced voucher.
        [HttpGet("template")]
        public async Task<IActionResult> DownloadTemplate()
        {
            var file = await Mediator.Send(new GetJournalImportTemplateQuery());
            return File(file.Content, file.ContentType, file.FileName);
        }

        [HttpGet]
        public async Task<IActionResult> GetAllAsync([FromQuery] int PageNumber, [FromQuery] int PageSize)
        {
            var result = await Mediator.Send(new GetAllJournalImportBatchQuery { PageNumber = PageNumber, PageSize = PageSize });

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
        public async Task<IActionResult> GetByIdAsync(int id)
        {
            var result = await Mediator.Send(new GetJournalImportBatchByIdQuery { Id = id });
            return Ok(new { StatusCode = StatusCodes.Status200OK, data = result });
        }
    }
}
