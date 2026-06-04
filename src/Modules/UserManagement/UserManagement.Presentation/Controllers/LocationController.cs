using UserManagement.Application.Location.Command.CreateLocation;
using UserManagement.Application.Location.Command.DeleteLocation;
using UserManagement.Application.Location.Command.UpdateLocation;
using UserManagement.Application.Location.Queries.GetAllLocation;
using UserManagement.Application.Location.Queries.GetLocationAutoSearch;
using UserManagement.Application.Location.Queries.GetLocationById;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;

namespace UserManagement.Presentation.Controllers
{
    [Route("api/[controller]")]
    public class LocationController : ApiControllerBase
    {
        public LocationController(ISender mediator) : base(mediator)
        {
        }

        [HttpPost]
        public async Task<IActionResult> CreateAsync([FromBody] CreateLocationCommand command)
        {
            var createLocation = await Mediator.Send(command);

            return Ok(new
            {
                StatusCode = StatusCodes.Status201Created,
                Message = "Location Created Successfully",
                Data = createLocation
            });
        }

        [HttpGet("GetAllLocation")]
        public async Task<IActionResult> GetAllLocationAsync([FromQuery] int PageNumber, [FromQuery] int PageSize, [FromQuery] string? SearchTerm = null)
        {
            var locations = await Mediator.Send(
                new GetAllLocationQuery
                {
                    PageNumber = PageNumber,
                    PageSize = PageSize,
                    SearchTerm = SearchTerm
                });

            if (locations.Data == null || !locations.Data.Any())
            {
                return NotFound(new
                {
                    StatusCode = StatusCodes.Status404NotFound,
                    message = locations.Message
                });
            }

            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                data = locations.Data,
                totalCount = locations.TotalCount,
                pageNumber = locations.PageNumber,
                pageSize = locations.PageSize
            });
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetByIdAsync(int id)
        {
            var result = await Mediator.Send(new GetLocationByIdQuery { Id = id });

            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                Data = result
            });
        }

        [HttpPut("update")]
        public async Task<IActionResult> UpdateAsync([FromBody] UpdateLocationCommand command)
        {
            var result = await Mediator.Send(command);

            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                Message = "Location Updated Successfully",
                Data = result
            });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var command = new DeleteLocationCommand { Id = id };

            await Mediator.Send(command);

            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                Message = "Location Deleted Successfully",
            });
        }

        [HttpGet("by-name")]
        public async Task<IActionResult> GetAllLocationAutocompleteAsync([FromQuery] string? name)
        {
            var result = await Mediator.Send(new GetLocationAutoCompleteQuery { SearchPattern = name });

            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                message = "Location List",
                data = result
            });
        }
    }
}
