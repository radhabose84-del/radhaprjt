using FinanceManagement.Application.JournalMaster.RecurringJournalTemplate.Commands.CreateRecurringJournalTemplate;
using FinanceManagement.Application.JournalMaster.RecurringJournalTemplate.Commands.DeleteRecurringJournalTemplate;
using FinanceManagement.Application.JournalMaster.RecurringJournalTemplate.Commands.GenerateRecurringTemplateNow;
using FinanceManagement.Application.JournalMaster.RecurringJournalTemplate.Commands.UpdateRecurringJournalTemplate;
using FinanceManagement.Application.JournalMaster.RecurringJournalTemplate.Queries.GetAllRecurringJournalTemplate;
using FinanceManagement.Application.JournalMaster.RecurringJournalTemplate.Queries.GetPendingRecurringTemplates;
using FinanceManagement.Application.JournalMaster.RecurringJournalTemplate.Queries.GetRecurringGeneratedInstances;
using FinanceManagement.Application.JournalMaster.RecurringJournalTemplate.Queries.GetRecurringJournalTemplateAutoComplete;
using FinanceManagement.Application.JournalMaster.RecurringJournalTemplate.Queries.GetRecurringJournalTemplateById;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace FinanceManagement.Presentation.Controllers.JournalMaster
{
    [Route("api/finance/[controller]")]
    public class RecurringJournalTemplateController : FinanceManagement.Presentation.Controllers.ApiControllerBase
    {
        public RecurringJournalTemplateController(IMediator mediator) : base(mediator) { }

        [HttpGet]
        public async Task<IActionResult> GetAllAsync(
            [FromQuery] int PageNumber,
            [FromQuery] int PageSize,
            [FromQuery] string? SearchTerm = null)
        {
            var result = await Mediator.Send(new GetAllRecurringJournalTemplateQuery
            {
                PageNumber = PageNumber,
                PageSize = PageSize,
                SearchTerm = SearchTerm
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

        [HttpGet("by-name")]
        public async Task<IActionResult> GetAutoCompleteAsync([FromQuery] string? term = null)
        {
            var result = await Mediator.Send(new GetRecurringJournalTemplateAutoCompleteQuery(term ?? string.Empty));

            return Ok(new { StatusCode = StatusCodes.Status200OK, data = result });
        }

        // Templates awaiting the current user's approval (approver inbox).
        [HttpGet("pending-approval")]
        public async Task<IActionResult> GetPendingApprovalAsync(
            [FromQuery] int PageNumber = 1,
            [FromQuery] int PageSize = 50)
        {
            var result = await Mediator.Send(new GetPendingRecurringTemplatesQuery { PageNumber = PageNumber, PageSize = PageSize });

            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                data = result.Data,
                TotalCount = result.TotalCount,
                PageNumber = result.PageNumber,
                PageSize = result.PageSize
            });
        }

        // "Generated Instances" grid — journals created from recurring templates (company-scoped).
        [HttpGet("generated-instances")]
        public async Task<IActionResult> GetGeneratedInstancesAsync(
            [FromQuery] int PageNumber = 1,
            [FromQuery] int PageSize = 50)
        {
            var result = await Mediator.Send(new GetRecurringGeneratedInstancesQuery { PageNumber = PageNumber, PageSize = PageSize });

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
            var result = await Mediator.Send(new GetRecurringJournalTemplateByIdQuery { Id = id });

            return Ok(new { StatusCode = StatusCodes.Status200OK, data = result });
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateRecurringJournalTemplateCommand command)
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
        public async Task<IActionResult> Update([FromBody] UpdateRecurringJournalTemplateCommand command)
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

        // "Generate" button — force-generate ONE template's JV now for the scheduled period.
        [HttpPost("generate-now")]
        public async Task<IActionResult> GenerateNow([FromBody] GenerateRecurringTemplateNowCommand command)
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
        public async Task<IActionResult> Delete(int id)
        {
            var result = await Mediator.Send(new DeleteRecurringJournalTemplateCommand(id));

            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                isSuccess = result,
                message = result ? "Recurring journal template deleted successfully." : "Failed to delete recurring journal template."
            });
        }
    }
}
