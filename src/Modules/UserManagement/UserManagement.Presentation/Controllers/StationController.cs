using UserManagement.Application.Station.Command.CreateStation;
using UserManagement.Application.Station.Command.DeleteStation;
using UserManagement.Application.Station.Command.UpdateStation;
using UserManagement.Application.Station.Queries.GetAllStation;
using UserManagement.Application.Station.Queries.GetStationAutoSearch;
using UserManagement.Application.Station.Queries.GetStationById;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;

namespace UserManagement.Presentation.Controllers
{
    [Route("api/[controller]")]
    public class StationController : ApiControllerBase
    {
        public StationController(ISender mediator) : base(mediator)
        {
        }

        [HttpPost]
        public async Task<IActionResult> CreateAsync([FromBody] CreateStationCommand command)
        {
            var createStation = await Mediator.Send(command);

            return Ok(new
            {
                StatusCode = StatusCodes.Status201Created,
                Message = "Station Created Successfully",
                Data = createStation
            });
        }

        [HttpGet("GetAllStation")]
        public async Task<IActionResult> GetAllStationAsync([FromQuery] int PageNumber, [FromQuery] int PageSize, [FromQuery] string? SearchTerm = null)
        {
            var stations = await Mediator.Send(
                new GetAllStationQuery
                {
                    PageNumber = PageNumber,
                    PageSize = PageSize,
                    SearchTerm = SearchTerm
                });

            if (stations.Data == null || !stations.Data.Any())
            {
                return NotFound(new
                {
                    StatusCode = StatusCodes.Status404NotFound,
                    message = stations.Message
                });
            }

            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                data = stations.Data,
                totalCount = stations.TotalCount,
                pageNumber = stations.PageNumber,
                pageSize = stations.PageSize
            });
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetByIdAsync(int id)
        {
            var result = await Mediator.Send(new GetStationByIdQuery { Id = id });

            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                Data = result
            });
        }

        [HttpPut("update")]
        public async Task<IActionResult> UpdateAsync([FromBody] UpdateStationCommand command)
        {
            var result = await Mediator.Send(command);

            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                Message = "Station Updated Successfully",
                Data = result
            });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var command = new DeleteStationCommand { Id = id };

            await Mediator.Send(command);

            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                Message = "Station Deleted Successfully",
            });
        }

        [HttpGet("by-name")]
        public async Task<IActionResult> GetAllStationAutocompleteAsync([FromQuery] string? name)
        {
            var result = await Mediator.Send(new GetStationAutoCompleteQuery { SearchPattern = name });

            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                message = "Station List",
                data = result
            });
        }
    }
}
