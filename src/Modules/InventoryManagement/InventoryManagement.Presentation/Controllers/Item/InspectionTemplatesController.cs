using InventoryManagement.Application.Item.Templates.Commands.CreateTemplate;
using InventoryManagement.Application.Item.Templates.Commands.DeleteTemplate;
using InventoryManagement.Application.Item.Templates.Commands.UpdateTemplate;
using InventoryManagement.Application.Item.Templates.Queries.GetInspectionTemplateAutoComplete;
using InventoryManagement.Application.Item.Templates.Queries.GetInspectionTemplateById;
using InventoryManagement.Application.Item.Templates.Queries.GetInspectionTemplates;
using InventoryManagement.Presentation.Controllers; 
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;

namespace InventoryManagement.Presentation.Controllers.Item
{
    [Route("api/[controller]")]
    public class InspectionTemplateController : ApiControllerBase
    {
        private readonly IMediator _mediator;

        public InspectionTemplateController(IMediator mediator) : base(mediator)
        {
            _mediator = mediator;
        }

        // GET: api/InspectionTemplate
        [HttpGet]
        public async Task<IActionResult> GetAllAsync([FromQuery] int PageNumber, [FromQuery] int PageSize, [FromQuery] string? SearchTerm = null)
        {
            var result = await Mediator.Send(new GetAllTemplatesQuery
            {
                PageNumber = PageNumber,
                PageSize = PageSize,
                SearchTerm = SearchTerm
            });

            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                data = result.Items,
                TotalCount = result.TotalCount,
                PageNumber = result.Page,
                PageSize = result.PageSize
            });
        }

        // GET: api/InspectionTemplate/by-name?Name=...
        [HttpGet("by-name")]
        public async Task<IActionResult> AutoCompleteAsync([FromQuery] string? Name)
        {
            var list = await Mediator.Send(new GetTemplateAutoCompleteQuery
            {
                SearchPattern = Name ?? string.Empty,
                Take = 10
            });

            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                data = list
            });
        }

        // GET: api/InspectionTemplate/5
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetByIdAsync(int id)
        {
            var dto = await Mediator.Send(new GetInspectionTemplateByIdQuery { Id = id });
            if (dto is null)
                return NotFound(new { StatusCode = StatusCodes.Status404NotFound, message = "Template not found" });

            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                data = dto,
                message = "Template fetched successfully"
            });
        }

        // POST: api/InspectionTemplate
        [HttpPost]
        public async Task<IActionResult> CreateAsync([FromBody] CreateTemplateCommand cmd)
        {
            var id = await _mediator.Send(cmd);
            return Ok(new
            {
                StatusCode = StatusCodes.Status201Created,
                message = "Created successfully.",
                data = id
            });
        }

        // PUT: api/InspectionTemplate
        [HttpPut]
        public async Task<IActionResult> UpdateAsync([FromBody] UpdateTemplateCommand cmd)
        {
            var ok = await _mediator.Send(cmd);
            if (!ok)
                return NotFound(new { StatusCode = StatusCodes.Status404NotFound, message = "Template not found" });

            return Ok(new
            {
                message = "Updated successfully.",
                statusCode = StatusCodes.Status200OK
            });
        }

        // DELETE: api/InspectionTemplate?id=5
        [HttpDelete]
        public async Task<IActionResult> DeleteAsync([FromQuery] int id)
        {
            var ok = await _mediator.Send(new DeleteTemplateCommand { Id = id });
            if (!ok)
                return NotFound(new { StatusCode = StatusCodes.Status404NotFound, message = "Template not found" });

            return Ok(new
            {
                message = "Deleted successfully.",
                statusCode = StatusCodes.Status200OK
            });
        }
    }
}
