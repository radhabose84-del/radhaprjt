using FinanceManagement.Application.TaxCode.Commands.CreateTaxAccountLinkage;
using FinanceManagement.Application.TaxCode.Commands.SubmitLinkageChangeRequest;
using FinanceManagement.Application.TaxCode.Queries.GetAllTaxAccountLinkage;
using FinanceManagement.Application.TaxCode.Queries.GetTaxAccountLinkageByAccount;
using FinanceManagement.Application.TaxCode.Queries.GetTaxAccountLinkageById;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace FinanceManagement.Presentation.Controllers
{
    // US-GL02-05B — Tax-Account Linkage (tax code -> GL control account, effective-dated,
    // initial create auto-APPROVED, modifications go through dual approval; change history = Finance.ActivityLog).
    [Route("api/finance/[controller]")]
    public class TaxAccountLinkageController : ApiControllerBase
    {
        public TaxAccountLinkageController(IMediator mediator) : base(mediator) { }

        [HttpGet]
        public async Task<IActionResult> GetAllLinkagesAsync(
            [FromQuery] int PageNumber,
            [FromQuery] int PageSize,
            [FromQuery] string? SearchTerm = null,
            [FromQuery] int? StatusId = null)
        {
            var result = await Mediator.Send(new GetAllTaxAccountLinkageQuery
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

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetLinkageByIdAsync(int id)
        {
            var result = await Mediator.Send(new GetTaxAccountLinkageByIdQuery { Id = id });
            return Ok(new { StatusCode = StatusCodes.Status200OK, data = result });
        }

        [HttpGet("by-account/{glAccountId:int}")]
        public async Task<IActionResult> GetLinkageByAccountAsync(int glAccountId)
        {
            var result = await Mediator.Send(new GetTaxAccountLinkageByAccountQuery { GlAccountId = glAccountId });
            return Ok(new { StatusCode = StatusCodes.Status200OK, isSuccess = result.IsSuccess, message = result.Message, data = result.Data });
        }

        [HttpPost]
        public async Task<IActionResult> CreateLinkage([FromBody] CreateTaxAccountLinkageCommand command)
        {
            var result = await Mediator.Send(command);
            return Ok(new { StatusCode = StatusCodes.Status200OK, isSuccess = result.IsSuccess, message = result.Message, data = result.Data });
        }

        // No public activate endpoint — StatusId (PENDING -> APPROVED) is flipped by the
        // BackgroundService Workflow module on approval-completion, which sends
        // ActivateTaxAccountLinkageCommand internally (see ActivateTaxAccountLinkageCommandHandler).

        [HttpPost("change-request")]
        public async Task<IActionResult> SubmitLinkageChangeRequest([FromBody] SubmitLinkageChangeRequestCommand command)
        {
            var result = await Mediator.Send(command);
            return Ok(new { StatusCode = StatusCodes.Status200OK, isSuccess = result.IsSuccess, message = result.Message, data = result.Data });
        }
    }
}
