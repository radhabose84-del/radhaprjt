using FinanceManagement.Application.JournalMaster.JournalImport.Commands.ImportJournals;
using FinanceManagement.Application.JournalMaster.JournalImport.Queries.GetAllJournalImportBatch;
using FinanceManagement.Application.JournalMaster.JournalImport.Queries.GetJournalImportBatchById;
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
