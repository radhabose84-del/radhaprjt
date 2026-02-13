using MediatR;
using Microsoft.AspNetCore.Mvc;
using UserManagement.Application.Entity.Queries.GetEntity;
using UserManagement.Application.Entity.Queries.GetEntityById;
using UserManagement.Application.Entity.Queries.GetEntityLastCode;
using UserManagement.Application.Entity.Commands.CreateEntity;
using UserManagement.Application.Entity.Commands.UpdateEntity;
using UserManagement.Application.Entity.Commands.DeleteEntity;
using FluentValidation;
using UserManagement.Infrastructure.Data;
using UserManagement.Application.Entity.Queries.GetEntityAutoComplete;
using System.Net;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using Microsoft.AspNetCore.Authorization;
using UserManagement.Application.Entity.Queries.GetEntityBasedCompany;
using UserManagement.Application.Entity.Queries.GetCompanyBasedUnit;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;

namespace UserManagement.Presentation.Controllers
{
    [Route("api/[controller]")]
    [ApiController]

    public class EntityController : ApiControllerBase
    {

        private readonly IMediator _mediator;

        private readonly ILogger<EntityController> _logger;
        public EntityController(IMediator mediator,
                             ILogger<EntityController> logger)
        : base(mediator)
        {

            _mediator = mediator;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllEntityAsync([FromQuery] int PageNumber, [FromQuery] int PageSize, [FromQuery] string? SearchTerm = null)
        {

            var result = await Mediator.Send(
            new GetEntityQuery
            {
                PageNumber = PageNumber,
                PageSize = PageSize,
                SearchTerm = SearchTerm
            });
            if (result is null || result.Data is null || !result.Data.Any())
            {
                _logger.LogWarning($"No Entity Record {result.Data} not found in DB.");
                return NotFound(new
                {
                    message = result.Message,
                    statusCode = StatusCodes.Status404NotFound

                });
            }
            _logger.LogInformation($"Entity {result.Data.Count} Listed successfully.");
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
                _logger.LogWarning($"EntityId {id} not found.");
                return BadRequest(new
                {
                    StatusCode = StatusCodes.Status400BadRequest,
                    message = "Invalid Entity ID"
                });
            }

            var result = await Mediator.Send(new GetEntityByIdQuery { EntityId = id });


            return Ok(new
            {
                message = "Entity Listed Successfully",
                statusCode = StatusCodes.Status200OK,
                data = result
            });


        }

        [HttpGet("new-code")]
        public async Task<IActionResult> GenerateEntityCodeAsync()
        {


            var lastEntityCode = await Mediator.Send(new GetEntityLastCodeQuery());

            if (lastEntityCode.IsSuccess)
            {
                _logger.LogInformation($"EntityCode {lastEntityCode.Data} Generated successfully.");
                return Ok(new
                {
                    message = lastEntityCode.Message,
                    statusCode = StatusCodes.Status200OK,
                    data = lastEntityCode.Data
                });
            }
            _logger.LogInformation($"EntityCode {lastEntityCode.Data} Not found.");
            return BadRequest(new
            {
                message = lastEntityCode.Message,
                statusCode = StatusCodes.Status400BadRequest
            });

        }

        [HttpPost]
        public async Task<IActionResult> CreateAsync(CreateEntityCommand createEntityCommand)
        {



            // Process the command
            var createdEntityId = await _mediator.Send(createEntityCommand);

            _logger.LogInformation($"EntityName {createEntityCommand.EntityName} created successfully.");
            return Ok(new
            {
                StatusCode = StatusCodes.Status201Created,
                message = "Entity Created Successfully",
                data = createdEntityId
            });


        }

        [HttpPut]
        public async Task<IActionResult> UpdateAsync(UpdateEntityCommand updateEntityCommand)
        {


            await _mediator.Send(updateEntityCommand);


            _logger.LogInformation($"EntityName {updateEntityCommand.EntityName} updated successfully.");
            return Ok(new
            {
                message = "Entity updated successfully",
                statusCode = StatusCodes.Status200OK
            });




        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteEntityAsync(int id)
        {
            var command = new DeleteEntityCommand { EntityId = id };

            // Process the delete command
            await _mediator.Send(command);

            _logger.LogInformation($"EntityId {id} deleted successfully.");
            return Ok(new
            {
                message = "Entity deleted successfully",
                statusCode = StatusCodes.Status200OK
            });




        }

        [HttpGet("by-name")]
        public async Task<IActionResult> GetEntity([FromQuery] string? EntityName)
        {
            // Fetch entities based on search pattern
            var entities = await Mediator.Send(new GetEntityAutocompleteQuery { SearchPattern = EntityName ?? string.Empty });
            _logger.LogInformation("Search pattern: {SearchPattern}", EntityName);


            return Ok(new
            {
                message = "Entity List",
                statusCode = StatusCodes.Status200OK,
                data = entities
            });


        }

        [HttpGet("{entityId}/companies")]
        [ActionName(nameof(GetCompaniesAsync))]
        public async Task<IActionResult> GetCompaniesAsync(int entityId, CancellationToken cancellationToken)
        {
            if (entityId <= 0)
            {
                _logger.LogWarning($"Invalid EntityId {entityId} requested.");
                return BadRequest(new
                {
                    StatusCode = StatusCodes.Status400BadRequest,
                    message = "Invalid Entity ID"
                });
            }

            var result = await _mediator.Send(new GetEntityBasedCompanyQuery { EntityId = entityId }, cancellationToken);

            if (result == null || result.Count == 0)
            {
                _logger.LogInformation($"No companies found for EntityId {entityId}.");
                return NotFound(new
                {
                    StatusCode = StatusCodes.Status404NotFound,
                    message = "No companies found for the given Entity ID"
                });
            }

            return Ok(new
            {
                message = "Companies Listed Successfully",
                statusCode = StatusCodes.Status200OK,
                data = result
            });
        }
        
         [HttpGet("UnitsLoad")]
        public async Task<IActionResult> GetUnitsBasedCompanies([FromQuery] string companyIds)
        {
            if (string.IsNullOrWhiteSpace(companyIds))
            {
                return BadRequest("CompanyIds are required.");
            }
            var parsedIds = companyIds
                .Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(id => int.Parse(id.Trim()))
                .ToList();

            var query = new GetCompanyBasedUnitQuery { CompanyIds = companyIds.Split(',').Select(int.Parse).ToList() };

            var result = await _mediator.Send(query);

            if (result == null || !result.Any())
            {
                return NotFound(new
                {
                    StatusCode = StatusCodes.Status404NotFound,
                    data = (object?)null,
                    message = $"Company with ID {companyIds} not found"
                });
            }

            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                data = result,
                message = "ID fetched successfully"
            });

        }
    }
}