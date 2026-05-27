using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using QCManagement.Application.QualityParameter.Commands.CreateQualityParameter;
using QCManagement.Application.QualityParameter.Commands.DeleteQualityParameter;
using QCManagement.Application.QualityParameter.Commands.UpdateQualityParameter;
using QCManagement.Application.QualityParameter.Queries.GetAllQualityParameter;
using QCManagement.Application.QualityParameter.Queries.GetQualityParameterAutoComplete;
using QCManagement.Application.QualityParameter.Queries.GetQualityParameterById;

namespace QCManagement.Presentation.Controllers
{
    [Route("api/qc/[controller]")]
    public class QualityParameterController : ApiControllerBase
    {
        public QualityParameterController(IMediator mediator) : base(mediator) { }

        [HttpGet]
        public async Task<IActionResult> GetAllQualityParameterAsync(
            [FromQuery] int PageNumber,
            [FromQuery] int PageSize,
            [FromQuery] string? SearchTerm = null,
            [FromQuery] int? ParameterGroupId = null)
        {
            var result = await Mediator.Send(new GetAllQualityParameterQuery
            {
                PageNumber = PageNumber,
                PageSize = PageSize,
                SearchTerm = SearchTerm,
                ParameterGroupId = ParameterGroupId
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
        public async Task<IActionResult> GetQualityParameterByIdAsync(int id)
        {
            var result = await Mediator.Send(new GetQualityParameterByIdQuery { Id = id });

            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                data = result
            });
        }

        [HttpGet("by-name")]
        public async Task<IActionResult> GetQualityParameterAutoCompleteAsync([FromQuery] string? term = null)
        {
            var result = await Mediator.Send(new GetQualityParameterAutoCompleteQuery(term ?? string.Empty));

            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                data = result
            });
        }

        [HttpPost]
        public async Task<IActionResult> CreateQualityParameter([FromBody] CreateQualityParameterCommand command)
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
        public async Task<IActionResult> UpdateQualityParameter([FromBody] UpdateQualityParameterCommand command)
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
        public async Task<IActionResult> DeleteQualityParameter(int id)
        {
            var result = await Mediator.Send(new DeleteQualityParameterCommand(id));

            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                isSuccess = result,
                message = result ? "Quality Parameter deleted successfully." : "Failed to delete Quality Parameter."
            });
        }
    }
}
