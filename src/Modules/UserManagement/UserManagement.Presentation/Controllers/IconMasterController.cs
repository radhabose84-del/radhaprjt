#nullable disable
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using UserManagement.Application.IconMaster.Commands.CreateIconMaster;
using UserManagement.Application.IconMaster.Commands.DeleteIconMaster;
using UserManagement.Application.IconMaster.Commands.UpdateIconMaster;
using UserManagement.Application.IconMaster.Queries.GetIconMaster;
using UserManagement.Application.IconMaster.Queries.GetIconMasterAutoComplete;
using UserManagement.Application.IconMaster.Queries.GetIconMasterById;

namespace UserManagement.Presentation.Controllers
{
    [Route("api/[controller]")]
    public class IconMasterController : ApiControllerBase
    {
        private readonly IMediator _mediator;
        private readonly ILogger<IconMasterController> _logger;

        public IconMasterController(IMediator mediator, ILogger<IconMasterController> logger)
            : base(mediator)
        {
            _mediator = mediator;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllIconMasterAsync([FromQuery] int PageNumber, [FromQuery] int PageSize, [FromQuery] string SearchTerm = null)
        {
            var result = await Mediator.Send(new GetIconMasterQuery
            {
                PageNumber = PageNumber,
                PageSize = PageSize,
                SearchTerm = SearchTerm
            });

            return Ok(new
            {
                message = result.Message,
                data = result.Data,
                statusCode = StatusCodes.Status200OK,
                TotalCount = result.TotalCount,
                PageNumber = result.PageNumber,
                PageSize = result.PageSize
            });
        }

        [HttpGet("{id}")]
        [ActionName(nameof(GetByIdAsync))]
        public async Task<IActionResult> GetByIdAsync(int id)
        {
            if (id <= 0)
            {
                return BadRequest(new
                {
                    StatusCode = StatusCodes.Status400BadRequest,
                    message = "Invalid IconMaster ID"
                });
            }

            var result = await Mediator.Send(new GetIconMasterByIdQuery { IconMasterId = id });
            return Ok(new
            {
                message = "IconMaster Listed Successfully",
                statusCode = StatusCodes.Status200OK,
                data = result
            });
        }

        [HttpGet("by-name")]
        public async Task<IActionResult> GetIconMaster([FromQuery] string term)
        {
            var result = await Mediator.Send(new GetIconMasterAutocompleteQuery { SearchPattern = term ?? string.Empty });
            return Ok(new
            {
                message = "IconMaster List",
                statusCode = StatusCodes.Status200OK,
                data = result
            });
        }

        [HttpPost]
        public async Task<IActionResult> CreateAsync(CreateIconMasterCommand createIconMasterCommand)
        {
            var createdId = await _mediator.Send(createIconMasterCommand);
            return Ok(new
            {
                StatusCode = StatusCodes.Status201Created,
                message = "IconMaster Created Successfully",
                data = createdId
            });
        }

        [HttpPut]
        public async Task<IActionResult> UpdateAsync(UpdateIconMasterCommand updateIconMasterCommand)
        {
            await _mediator.Send(updateIconMasterCommand);
            return Ok(new
            {
                message = "IconMaster Updated Successfully",
                statusCode = StatusCodes.Status200OK
            });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteIconMasterAsync(int id)
        {
            await _mediator.Send(new DeleteIconMasterCommand { Id = id });
            return Ok(new
            {
                message = "IconMaster Deleted Successfully",
                statusCode = StatusCodes.Status200OK
            });
        }
    }
}
