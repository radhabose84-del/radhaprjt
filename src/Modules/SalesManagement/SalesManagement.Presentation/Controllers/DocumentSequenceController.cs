using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SalesManagement.Application.DocumentSequence.Commands.CreateDocumentSequence;
using SalesManagement.Application.DocumentSequence.Commands.DeleteDocumentSequence;
using SalesManagement.Application.DocumentSequence.Commands.UpdateDocumentSequence;
using SalesManagement.Application.DocumentSequence.Queries.GetAllDocumentSequence;
using SalesManagement.Application.DocumentSequence.Queries.GetDocumentNumberByTypeId;
using SalesManagement.Application.DocumentSequence.Queries.GetDocumentSequenceAutoComplete;
using SalesManagement.Application.DocumentSequence.Queries.GetDocumentSequenceById;

namespace SalesManagement.Presentation.Controllers
{
    [Route("api/[controller]")]
    public class DocumentSequenceController : ApiControllerBase
    {
        public DocumentSequenceController(IMediator mediator) : base(mediator) { }

        [HttpGet]
        public async Task<IActionResult> GetAllDocumentSequenceAsync(
            [FromQuery] int PageNumber,
            [FromQuery] int PageSize,
            [FromQuery] string? SearchTerm = null)
        {
            var result = await Mediator.Send(new GetAllDocumentSequenceQuery
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

        [HttpGet("{id}")]
        public async Task<IActionResult> GetDocumentSequenceByIdAsync(int id)
        {
            var result = await Mediator.Send(new GetDocumentSequenceByIdQuery { Id = id });
            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                data = result
            });
        }

        [HttpGet("by-name")]
        public async Task<IActionResult> GetDocumentSequenceAutoCompleteAsync([FromQuery] string? term = null)
        {
            var result = await Mediator.Send(new GetDocumentSequenceAutoCompleteQuery(term));
            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                data = result
            });
        }

        [HttpGet("by-type/{typeId}")]
        public async Task<IActionResult> GetDocumentNumberByTypeIdAsync(int typeId)
        {
            var result = await Mediator.Send(new GetDocumentNumberByTypeIdQuery { TypeId = typeId });
            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                data = result
            });
        }

        [HttpPost]
        public async Task<IActionResult> CreateDocumentSequence([FromBody] CreateDocumentSequenceCommand command)
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
        public async Task<IActionResult> UpdateDocumentSequence([FromBody] UpdateDocumentSequenceCommand command)
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
        public async Task<IActionResult> DeleteDocumentSequence(int id)
        {
            var result = await Mediator.Send(new DeleteDocumentSequenceCommand(id));
            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                isSuccess = result,
                message = result ? "Document Sequence deleted successfully." : "Document Sequence not found."
            });
        }
    }
}
