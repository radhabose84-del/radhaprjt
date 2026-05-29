using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using QCManagement.Application.QualitySpecification.Commands.CreateQualitySpecification;
using QCManagement.Application.QualitySpecification.Commands.DeleteQualitySpecification;
using QCManagement.Application.QualitySpecification.Commands.UpdateQualitySpecification;
using QCManagement.Application.QualitySpecification.Queries.GetAllQualitySpecification;
using QCManagement.Application.QualitySpecification.Queries.GetQualitySpecificationAutoComplete;
using QCManagement.Application.QualitySpecification.Queries.GetQualitySpecificationById;

namespace QCManagement.Presentation.Controllers
{
    [Route("api/qc/[controller]")]
    public class QualitySpecificationController : ApiControllerBase
    {
        public QualitySpecificationController(IMediator mediator) : base(mediator) { }

        [HttpGet]
        public async Task<IActionResult> GetAllQualitySpecificationAsync(
            [FromQuery] int PageNumber,
            [FromQuery] int PageSize,
            [FromQuery] string? SearchTerm = null,
            [FromQuery] int? QualityTemplateId = null,
            [FromQuery] int? ApplicableLevelId = null,
            [FromQuery] int? QcTypeId = null,
            [FromQuery] int? ItemCategoryId = null,
            [FromQuery] int? ItemId = null,
            [FromQuery] bool? IsActive = null)
        {
            var result = await Mediator.Send(new GetAllQualitySpecificationQuery
            {
                PageNumber = PageNumber,
                PageSize = PageSize,
                SearchTerm = SearchTerm,
                QualityTemplateId = QualityTemplateId,
                ApplicableLevelId = ApplicableLevelId,
                QcTypeId = QcTypeId,
                ItemCategoryId = ItemCategoryId,
                ItemId = ItemId,
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
        public async Task<IActionResult> GetQualitySpecificationByIdAsync(int id)
        {
            var result = await Mediator.Send(new GetQualitySpecificationByIdQuery { Id = id });

            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                data = result
            });
        }

        [HttpGet("by-name")]
        public async Task<IActionResult> GetQualitySpecificationAutoCompleteAsync([FromQuery] string? term = null)
        {
            var result = await Mediator.Send(new GetQualitySpecificationAutoCompleteQuery(term ?? string.Empty));

            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                data = result
            });
        }

        [HttpPost]
        public async Task<IActionResult> CreateQualitySpecification([FromBody] CreateQualitySpecificationCommand command)
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
        public async Task<IActionResult> UpdateQualitySpecification([FromBody] UpdateQualitySpecificationCommand command)
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
        public async Task<IActionResult> DeleteQualitySpecification(int id)
        {
            var result = await Mediator.Send(new DeleteQualitySpecificationCommand(id));

            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                isSuccess = result,
                message = result ? "Quality Specification deleted successfully." : "Failed to delete Quality Specification."
            });
        }
    }
}
