using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Contracts.Common;
using MaintenanceManagement.Application.Common.Interfaces.Power.IFeederGroup;

using MaintenanceManagement.Application.Power.FeederGroup.Command.CreateFeederGroup;
using MaintenanceManagement.Application.Power.FeederGroup.Command.DeleteFeederGroup;
using MaintenanceManagement.Application.Power.FeederGroup.Command.UpdateFeederGroup;
using MaintenanceManagement.Application.Power.FeederGroup.Queries.GetFeederGroup;
using MaintenanceManagement.Application.Power.FeederGroup.Queries.GetFeederGroupAutoComplete;
using MaintenanceManagement.Application.Power.FeederGroup.Queries.GetFeederGroupById;
using FluentValidation;
using MaintenanceManagement.Presentation.Validation.Power.FeederGroup;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace MaintenanceManagement.Presentation.Controllers.Power
{
    [Route("api/[controller]")]
    public class FeederGroupController : ApiControllerBase
    {
        private readonly IFeederGroupQueryRepository _feederGroupQueryRepository;
        private readonly IValidator<CreateFeederGroupCommand> _feederGroupCommandRepository;
        private readonly IValidator<UpdateFeederGroupCommand> _updateFeederGroupCommandValidator;
        private readonly IValidator<DeleteFeederGroupCommand> _deletefeederGroupCommandDelegateRepository;


        private readonly ILogger _logger;

        public FeederGroupController(ISender mediator, ILogger<FeederGroupController> logger,
        IFeederGroupQueryRepository feederGroupQueryRepository,
        IValidator<CreateFeederGroupCommand> feederGroupCommandRepository,
        IValidator<UpdateFeederGroupCommand> updateFeederGroupCommandValidator,
        IValidator<DeleteFeederGroupCommand> feederGroupCommandDelegateRepository) : base(mediator)
        {
            _logger = logger;
            _feederGroupQueryRepository = feederGroupQueryRepository;
            _feederGroupCommandRepository = feederGroupCommandRepository;
            _updateFeederGroupCommandValidator = updateFeederGroupCommandValidator;
            _deletefeederGroupCommandDelegateRepository = feederGroupCommandDelegateRepository;

        }

        [HttpGet]
        public async Task<IActionResult> GetAllFeederGroupAsync([FromQuery] int PageNumber, [FromQuery] int PageSize, [FromQuery] string? SearchTerm = null)
        {
            var FeederGroup = await Mediator.Send(
             new GetFeederGroupQuery
             {
                 PageNumber = PageNumber,
                 PageSize = PageSize,
                 SearchTerm = SearchTerm
             });
            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                data = FeederGroup.Data,
                TotalCount = FeederGroup.TotalCount,
                PageNumber = FeederGroup.PageNumber,
                PageSize = FeederGroup.PageSize
            });
        }     

                [HttpGet("{id}")]
       
        public async Task<IActionResult> GetFeederById(int id)
        {
            var result = await Mediator.Send(new GetFeederGroupByIdQuery { Id = id });

                return Ok(new
                {
                    StatusCode = StatusCodes.Status200OK,
                    data = result,
                    message = result
                });
           
        }

        [HttpPost("create")]
        public async Task<ActionResult<ApiResponseDTO<int>>> Create([FromBody] CreateFeederGroupCommand command)
        {


            var response = await Mediator.Send(command);

                return CreatedAtAction(nameof(GetFeederById), new { id = response }, new
                {
                    StatusCode = StatusCodes.Status201Created,
                    message = response,
                    errors = "",
                    data = response
                });
            
        }


        [HttpPut]
        public async Task<IActionResult> UpdateAsync(UpdateFeederGroupCommand command)
        {
          
            var result = await Mediator.Send(command);
           
                return Ok(new
                {
                    StatusCode = StatusCodes.Status200OK,
                    message = result,
                    asset = result
                });
           

        }


        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            
            var updatefeederGroup = await Mediator.Send(new DeleteFeederGroupCommand { Id = id });

          
            return Ok(new { StatusCode = StatusCodes.Status200OK, message = updatefeederGroup, errors = "" });

        }

         [HttpGet("by-name")]
        public async Task<IActionResult> GetFeederGroup([FromQuery] string? name)
        {
          
            var feederGroup = await Mediator.Send(new GetFeederGroupAutoCompleteQuery {SearchPattern = name});
            
            return Ok( new { StatusCode=StatusCodes.Status200OK, data = feederGroup });
            
        }
            



      
    }
}