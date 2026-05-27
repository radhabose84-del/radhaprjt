using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using QCManagement.Application.QualityTemplate.Commands.CreateQualityTemplate;
using QCManagement.Application.QualityTemplate.Commands.DeleteQualityTemplate;
using QCManagement.Application.QualityTemplate.Commands.UpdateQualityTemplate;
using QCManagement.Application.QualityTemplate.Queries.GetAllQualityTemplate;
using QCManagement.Application.QualityTemplate.Queries.GetQualityTemplateAutoComplete;
using QCManagement.Application.QualityTemplate.Queries.GetQualityTemplateById;

namespace QCManagement.Presentation.Controllers
{
    [Route("api/qc/[controller]")]
    public class QualityTemplateController : ApiControllerBase
    {
        public QualityTemplateController(IMediator mediator) : base(mediator) { }

        [HttpGet]
        public async Task<IActionResult> GetAllQualityTemplateAsync(
            [FromQuery] int PageNumber,
            [FromQuery] int PageSize,
            [FromQuery] string? SearchTerm = null,
            [FromQuery] bool? IsActive = null)
        {
            var result = await Mediator.Send(new GetAllQualityTemplateQuery
            {
                PageNumber = PageNumber,
                PageSize = PageSize,
                SearchTerm = SearchTerm,
                IsActive = IsActive
            });

            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                data = result.Data,
                result.TotalCount,
                result.PageNumber,
                result.PageSize
            });
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetQualityTemplateByIdAsync(int id)
        {
            var result = await Mediator.Send(new GetQualityTemplateByIdQuery { Id = id });

            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                data = result
            });
        }

        [HttpGet("by-name")]
        public async Task<IActionResult> GetQualityTemplateAutoCompleteAsync([FromQuery] string? term = null)
        {
            var result = await Mediator.Send(new GetQualityTemplateAutoCompleteQuery(term ?? string.Empty));

            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                data = result
            });
        }

        [HttpPost]
        public async Task<IActionResult> CreateQualityTemplate([FromBody] CreateQualityTemplateCommand command)
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
        public async Task<IActionResult> UpdateQualityTemplate([FromBody] UpdateQualityTemplateCommand command)
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
        public async Task<IActionResult> DeleteQualityTemplate(int id)
        {
            var result = await Mediator.Send(new DeleteQualityTemplateCommand(id));

            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                isSuccess = result,
                message = result ? "Quality Template deleted successfully." : "Failed to delete Quality Template."
            });
        }
    }
}
