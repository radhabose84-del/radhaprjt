using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using MaintenanceManagement.Application.Common.HttpResponse;
using MaintenanceManagement.Application.Common.Interfaces.Power.IFeeder;
using MaintenanceManagement.Application.Power.Feeder.Command.CreateFeeder;
using MaintenanceManagement.Application.Power.Feeder.Command.DeleteFeeder;
using MaintenanceManagement.Application.Power.Feeder.Command.UpdateFeeder;
using MaintenanceManagement.Application.Power.Feeder.Queries.GetFeeder;
using MaintenanceManagement.Application.Power.Feeder.Queries.GetFeederAutoComplete;
using MaintenanceManagement.Application.Power.Feeder.Queries.GetFeederById;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace MaintenanceManagement.API.Controllers.Power
{
    [ApiController]
    [Route("api/[controller]")]
    public class FeederController : ApiControllerBase
    {
        private readonly ILogger<FeederController> _logger;
        private readonly IFeederQueryRepository _feederQueryRepository;
        private readonly IValidator<CreateFeederCommand> _feederCommandRepository;
        private readonly IValidator<UpdateFeederCommand> _updatefeederCommandRepository;
        private readonly IValidator<DeleteFeederCommand> _DeletefeederCommandRepository;

        public FeederController(ISender mediator, ILogger<FeederController> logger, IFeederQueryRepository feederQueryRepository, IValidator<CreateFeederCommand> feederCommandRepository, IValidator<UpdateFeederCommand> updatefeederCommandRepository, IValidator<DeleteFeederCommand> DeletefeederCommandRepository) : base(mediator)
        {
            _logger = logger;
            _feederQueryRepository = feederQueryRepository;
            _feederCommandRepository = feederCommandRepository;
            _updatefeederCommandRepository = updatefeederCommandRepository;
            _DeletefeederCommandRepository = DeletefeederCommandRepository;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllFeederAsync([FromQuery] int PageNumber, [FromQuery] int PageSize, [FromQuery] string? SearchTerm = null)
        {
            var Feeder = await Mediator.Send(
             new GetFeederQuery
             {
                 PageNumber = PageNumber,
                 PageSize = PageSize,
                 SearchTerm = SearchTerm
             });
            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                data = Feeder.Data,
                TotalCount = Feeder.TotalCount,
                PageNumber = Feeder.PageNumber,
                PageSize = Feeder.PageSize
            });
        }

        [HttpGet("{id}")]

        public async Task<IActionResult> GetById(int id)
        {
            var result = await Mediator.Send(new GetFeederByIdQuery { Id = id });

                return Ok(new
                {
                    StatusCode = StatusCodes.Status200OK,
                    data = result,
                    message = result
                });
           
        }

        [HttpPost("create")]
        public async Task<ActionResult<ApiResponseDTO<int>>> Create([FromBody] CreateFeederCommand command)
        {
            

            var response = await Mediator.Send(command);

           
                return CreatedAtAction(nameof(GetById), new { id = response }, new
                {
                    StatusCode = StatusCodes.Status201Created,
                    message = " Feeder created successfully",
                    errors = "",
                    data = response
                });
           
        }

        [HttpPut]
        public async Task<IActionResult> UpdateAsync(UpdateFeederCommand command)
        {
            
            var result = await Mediator.Send(command);
           
                return Ok(new
                {
                    StatusCode = StatusCodes.Status200OK,
                    message = "Feeder updated successfully",
                    asset = result
                });
            

        }
         
           [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
           

            var updatefeeder = await Mediator.Send(new DeleteFeederCommand { Id = id });

           
            return Ok(new { StatusCode = StatusCodes.Status200OK, message = updatefeeder, errors = "" });

        }


           [HttpGet("by-name")]
        public async Task<IActionResult> GetFeeder([FromQuery] string? name)
        {
          
            var feeder = await Mediator.Send(new GetFeederAutoCompleteQuery {SearchPattern = name});
            return Ok( new { StatusCode=StatusCodes.Status200OK, data = feeder });
            
        }
    }
}