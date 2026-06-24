using FinanceManagement.Application.JournalMaster.Journal.Commands.CopyJournal;
using FinanceManagement.Application.JournalMaster.Journal.Commands.CreateJournal;
using FinanceManagement.Application.JournalMaster.Journal.Commands.DeleteJournal;
using FinanceManagement.Application.JournalMaster.Journal.Commands.PostJournal;
using FinanceManagement.Application.JournalMaster.Journal.Commands.PostJournalBatch;
using FinanceManagement.Application.JournalMaster.Journal.Commands.ReverseJournal;
using FinanceManagement.Application.JournalMaster.Journal.Commands.UpdateJournal;
using FinanceManagement.Application.JournalMaster.JournalThresholdRule.Commands.RunGapScan;
using FinanceManagement.Application.JournalMaster.Journal.Queries.GetAllJournal;
using FinanceManagement.Application.JournalMaster.Journal.Queries.GetJournalById;
using FinanceManagement.Application.JournalMaster.Journal.Queries.GetPendingApprovalJournals;
using FinanceManagement.Application.JournalMaster.Journal.Queries.GetPostableJournals;
using FinanceManagement.Application.JournalMaster.Journal.Queries.SearchJournal;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace FinanceManagement.Presentation.Controllers.JournalMaster
{
    [Route("api/finance/[controller]")]
    public class JournalController : FinanceManagement.Presentation.Controllers.ApiControllerBase
    {
        public JournalController(IMediator mediator) : base(mediator) { }

        [HttpGet]
        public async Task<IActionResult> GetAllJournalAsync(
            [FromQuery] int PageNumber,
            [FromQuery] int PageSize,
            [FromQuery] string? SearchTerm = null,
            [FromQuery] int? StatusId = null)
        {
            var result = await Mediator.Send(new GetAllJournalQuery
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

        // Advanced filtered search (Journal List & Search screen).
        [HttpGet("search")]
        public async Task<IActionResult> SearchJournalAsync([FromQuery] SearchJournalQuery query)
        {
            var result = await Mediator.Send(query);

            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                data = result.Data,
                TotalCount = result.TotalCount,
                PageNumber = result.PageNumber,
                PageSize = result.PageSize
            });
        }

        // Vouchers ready to post (APPROVED, or system journals still in DRAFT) — feeds the bulk-post screen.
        [HttpGet("postable")]
        public async Task<IActionResult> GetPostableJournalsAsync(
            [FromQuery] int PageNumber = 1,
            [FromQuery] int PageSize = 50)
        {
            var result = await Mediator.Send(new GetPostableJournalsQuery
            {
                PageNumber = PageNumber,
                PageSize = PageSize
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

        // Manual DRAFT vouchers awaiting the current user's approval (approver inbox).
        [HttpGet("pending-approval")]
        public async Task<IActionResult> GetPendingApprovalJournalsAsync(
            [FromQuery] int PageNumber = 1,
            [FromQuery] int PageSize = 50)
        {
            var result = await Mediator.Send(new GetPendingApprovalJournalsQuery
            {
                PageNumber = PageNumber,
                PageSize = PageSize
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
        public async Task<IActionResult> GetJournalByIdAsync(int id)
        {
            var result = await Mediator.Send(new GetJournalByIdQuery { Id = id });

            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                data = result
            });
        }

        [HttpPost]
        public async Task<IActionResult> CreateJournal([FromBody] CreateJournalCommand command)
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
        public async Task<IActionResult> UpdateJournal([FromBody] UpdateJournalCommand command)
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

        [HttpPost("post")]
        public async Task<IActionResult> PostJournal([FromBody] PostJournalCommand command)
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

        // Run the voucher-number gap scan now (also runs nightly via Hangfire). Writes SequenceGapScanLog rows.
        [HttpPost("gap-scan")]
        public async Task<IActionResult> RunGapScan()
        {
            var result = await Mediator.Send(new RunGapScanCommand());

            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                isSuccess = result.IsSuccess,
                message = result.Message,
                data = result.Data
            });
        }

        // Duplicate a journal into a new editable draft (Copy button on the detail screen).
        [HttpPost("copy")]
        public async Task<IActionResult> CopyJournal([FromBody] CopyJournalCommand command)
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

        [HttpPost("reverse")]
        public async Task<IActionResult> ReverseJournal([FromBody] ReverseJournalCommand command)
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

        [HttpPost("post-batch")]
        public async Task<IActionResult> PostJournalBatch([FromBody] PostJournalBatchCommand command)
        {
            var result = await Mediator.Send(command);

            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                isSuccess = result.IsSuccess,
                message = result.Message,
                data = result.Data,
                TotalCount = result.TotalCount
            });
        }

        [HttpDelete]
        public async Task<IActionResult> DeleteJournal(int id)
        {
            var result = await Mediator.Send(new DeleteJournalCommand(id));

            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                isSuccess = result,
                message = result ? "Journal voucher deleted successfully." : "Failed to delete journal voucher."
            });
        }
    }
}
