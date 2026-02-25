using PurchaseManagement.Application.PurchaseIndents.Command.CreatePurchaseIndent;
using PurchaseManagement.Application.PurchaseIndents.Command.DeletePurchaseIndent;
using PurchaseManagement.Application.PurchaseIndents.Command.UpdatePurchaseIndent;
using PurchaseManagement.Application.PurchaseIndents.Queries.ApprovedIndentDetailsForPO;
using PurchaseManagement.Application.PurchaseIndents.Queries.GetAllPurchaseIndent;
using PurchaseManagement.Application.PurchaseIndents.Queries.GetPendingIndent;
using PurchaseManagement.Application.PurchaseIndents.Queries.GetPendingIndentById;
using PurchaseManagement.Application.PurchaseIndents.Queries.GetPurchaseIndentAutoComplete;
using PurchaseManagement.Application.PurchaseIndents.Queries.GetPurchaseIndentById;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;

namespace PurchaseManagement.Presentation.Controllers
{
    [Route("api/[controller]")]
    public class PurchaseIndentController : ApiControllerBase
    {
        private readonly IMediator _mediator;
        private readonly ILogger<PurchaseIndentController> _logger;
        public PurchaseIndentController(IMediator mediator, ILogger<PurchaseIndentController> logger)
        : base(mediator)
        {
            _mediator = mediator;
            _logger = logger;
        }
        [HttpGet]
        public async Task<IActionResult> GetAllPurchaseIndentAsync([FromQuery] int PageNumber, [FromQuery] int PageSize, [FromQuery] string? SearchTerm = null, [FromQuery] int? StatusId = null)
        {

            var PurchaseIndent = await Mediator.Send(
             new GetAllPurchaseIndentQuery
             {
                 PageNumber = PageNumber,
                 PageSize = PageSize,
                 SearchTerm = SearchTerm,
                 StatusId = StatusId
             });
            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                data = PurchaseIndent.Data,
                TotalCount = PurchaseIndent.TotalCount,
                PageNumber = PurchaseIndent.PageNumber,
                PageSize = PurchaseIndent.PageSize
            });
        }

        [HttpPost]
        public async Task<IActionResult> CreateAsync(CreatePurchaseIndentCommand createPurchaseIndentCommand)
        {
            _logger.LogInformation("Action: Create, Method: CreateAsync, request: {@createPurchaseIndentCommand}", createPurchaseIndentCommand);
            var CreatedIndent = await _mediator.Send(createPurchaseIndentCommand);
            return Ok(new
            {
                StatusCode = StatusCodes.Status201Created,
                message = "Created successfully.",
                data = CreatedIndent
            });

        }
        [HttpPut]
        public async Task<IActionResult> UpdateAsync(UpdatePurchaseIndentCommand updatePurchaseIndentCommand)
        {
            await _mediator.Send(updatePurchaseIndentCommand);
            return Ok(new
            {
                message = "Updated successfully.",
                statusCode = StatusCodes.Status200OK
            });
        }

        [HttpDelete]
        public async Task<IActionResult> DeleteAsync(int id)
        {
            await _mediator.Send(new DeletePurchaseIndentCommand { Id = id });
            return Ok(new
            {
                message = "Deleted successfully.",
                statusCode = StatusCodes.Status200OK
            });

        }
        /* [HttpGet("{id}")]
        public async Task<IActionResult> GetByIdAsync(int id,int? sourceId)
        {
            var Indent = await Mediator.Send(new GetPurchaseIndentByIdQuery() { Id = id,SourceId=sourceId });
            return Ok(new { StatusCode = StatusCodes.Status200OK, data = Indent, message = "" });
        } */
        [HttpGet("{id}")]
        public async Task<IActionResult> GetByIdAsync(int id, [FromQuery] int? sourceId)
        {
            var indent = await Mediator.Send(new GetPurchaseIndentByIdQuery
            {
                Id = id,
                SourceId = sourceId
            });

            return Ok(new
            {
                statusCode = StatusCodes.Status200OK,
                data = indent,
                message = ""
            });
        }

        [HttpGet("pending")]
        public async Task<IActionResult> GetPendingPurchaseIndentAsync([FromQuery] int PageNumber, [FromQuery] int PageSize, [FromQuery] string? SearchTerm = null)
        {
            var PurchaseIndent = await Mediator.Send(
             new GetPendingIndentQuery
             {
                 PageNumber = PageNumber,
                 PageSize = PageSize,
                 SearchTerm = SearchTerm
             });
            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                data = PurchaseIndent.Data,
                TotalCount = PurchaseIndent.TotalCount,
                PageNumber = PurchaseIndent.PageNumber,
                PageSize = PurchaseIndent.PageSize
            });
        }

        [HttpGet("pending/{id}")]
        public async Task<IActionResult> GetPendingIndentByIdAsync(int id)
        {
            var Indent = await Mediator.Send(new GetPendingIndentByIdQuery() { Id = id });
            return Ok(new { StatusCode = StatusCodes.Status200OK, data = Indent, message = "" });
        }
        [HttpGet("autocomplete")]
        public async Task<IActionResult> GetPurchaseIndentAutoCompleteAsync([FromQuery] string Status, [FromQuery] string? SearchTerm = null, [FromQuery] bool allIndents = false)
        {
            var PurchaseIndent = await Mediator.Send(
             new GetPurchaseIndentAutoCompleteQuery
             {
                 Status = Status,
                 SearchTerm = SearchTerm,
                 AllIndents = allIndents
             });
            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                data = PurchaseIndent
            });
        }

        [HttpGet("indentdetailsforpo")]
        public async Task<IActionResult> GetIndentDetailsForPOAsync([FromQuery] int? vendorId,[FromQuery] int? departmentId)
        {
  
            var result = await Mediator.Send(new ApprovedIndentDetailsForPOQuery { VendorId = vendorId,DepartmentId=departmentId });
            return Ok(new { StatusCode = StatusCodes.Status200OK, data = result, message = "" });
        }


    }
}