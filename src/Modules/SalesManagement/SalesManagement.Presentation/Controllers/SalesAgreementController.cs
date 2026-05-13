using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SalesManagement.Application.SalesAgreement.Commands.CreateSalesAgreement;
using SalesManagement.Application.SalesAgreement.Commands.DeleteSalesAgreement;
using SalesManagement.Application.SalesAgreement.Commands.UploadSalesAgreementDocument;
using SalesManagement.Application.SalesAgreement.Commands.DeleteSalesAgreementDocument;
using SalesManagement.Application.SalesAgreement.Queries.GetAllSalesAgreement;
using SalesManagement.Application.SalesAgreement.Queries.GetSalesAgreementAutoComplete;
using SalesManagement.Application.SalesAgreement.Queries.GetSalesAgreementById;

namespace SalesManagement.Presentation.Controllers
{
    [Route("api/[controller]")]
    public class SalesAgreementController : ApiControllerBase
    {
        public SalesAgreementController(IMediator mediator) : base(mediator) { }

        [HttpGet]
        public async Task<IActionResult> GetAllSalesAgreementAsync(
            [FromQuery] int PageNumber,
            [FromQuery] int PageSize,
            [FromQuery] string? SearchTerm = null,
            [FromQuery] string? StatusName = null)
        {
            var result = await Mediator.Send(new GetAllSalesAgreementQuery
            {
                PageNumber = PageNumber,
                PageSize = PageSize,
                SearchTerm = SearchTerm,
                StatusName = StatusName
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
        public async Task<IActionResult> GetSalesAgreementByIdAsync(int id)
        {
            var result = await Mediator.Send(new GetSalesAgreementByIdQuery { Id = id });

            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                data = result
            });
        }

        [HttpGet("by-name")]
        public async Task<IActionResult> GetSalesAgreementAutoCompleteAsync(
            [FromQuery] string? term = null,
            [FromQuery] int? customerId = null)
        {
            var result = await Mediator.Send(new GetSalesAgreementAutoCompleteQuery(term ?? string.Empty, customerId));

            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                data = result
            });
        }

        [HttpPost]
        public async Task<IActionResult> CreateSalesAgreement([FromBody] CreateSalesAgreementCommand command)
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
        public async Task<IActionResult> DeleteSalesAgreement(int id)
        {
            var result = await Mediator.Send(new DeleteSalesAgreementCommand(id));

            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                isSuccess = result,
                message = result ? "Sales Agreement deleted successfully." : "Failed to delete Sales Agreement."
            });
        }

        [HttpPost("upload-document")]
        public async Task<IActionResult> UploadSalesAgreementDocument([FromForm] UploadSalesAgreementDocumentCommand command)
        {
            var result = await Mediator.Send(command);

            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                isSuccess = true,
                message = "Document uploaded successfully.",
                data = result
            });
        }

        [HttpDelete("delete-document")]
        public async Task<IActionResult> DeleteSalesAgreementDocument([FromQuery] string filePath)
        {
            var result = await Mediator.Send(new DeleteSalesAgreementDocumentCommand { FilePath = filePath });

            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                isSuccess = result,
                message = result ? "Document deleted successfully." : "Failed to delete document."
            });
        }
    }
}
