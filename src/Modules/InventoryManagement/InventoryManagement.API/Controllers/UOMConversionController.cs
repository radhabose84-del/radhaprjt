using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using InventoryManagement.Application.UOMConversion.Command.CreateUOMConversion;
using InventoryManagement.Application.UOMConversion.Command.DeleteUOMConversion;
using InventoryManagement.Application.UOMConversion.Command.UpdateUOMConversion;
using InventoryManagement.Application.UOMConversion.Queries.GetAllUOMConversion;
using InventoryManagement.Application.UOMConversion.Queries.GetConvertedValue;
using InventoryManagement.Application.UOMConversion.Queries.GetUOMConversionById;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace InventoryManagement.API.Controllers
{
    [Route("api/[controller]")]
    public class UOMConversionController : ApiControllerBase
    {
        private readonly ILogger<UOMConversionController> _logger;

        public UOMConversionController(ISender mediator) : base(mediator)

        {
        }

        [HttpGet]

        public async Task<IActionResult> GetAllUOMConversionsAsync([FromQuery] int PageNumber, [FromQuery] int PageSize, [FromQuery] string? SearchTerm = null)
        {
            var conversions = await Mediator.Send(
                new GetAllUOMConversionsQuery
                {
                    PageNumber = PageNumber,
                    PageSize = PageSize,
                    SearchTerm = SearchTerm
                });

            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                Data = conversions.Data,
                TotalCount = conversions.TotalCount,
                PageNumber = conversions.PageNumber,
                PageSize = conversions.PageSize
            });
        }

        [HttpGet("{id}")]
        [ActionName(nameof(GetByIdAsync))]
        public async Task<IActionResult> GetByIdAsync(int id)
        {
            var uomConversion = await Mediator.Send(new GetUOMConversionByIdQuery { Id = id });

            if (uomConversion == null)
            {
                return NotFound(new
                {
                    StatusCode = StatusCodes.Status404NotFound,
                    Message = $"UOM Conversion record not found for Id = {id}"
                });
            }

            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                Data = uomConversion,
                Message = "UOM Conversion record fetched successfully"
            });
        }

        [HttpPost]
        public async Task<IActionResult> CreateAsync(CreateUOMConversionCommand command)
        {


            var response = await Mediator.Send(command);

            return Ok(new
            {
                StatusCode = StatusCodes.Status201Created,
                message = "Created Successfully",
                errors = "",
                data = response
            });

        }


        [HttpPut]
        public async Task<IActionResult> Update(UpdateUOMConversionCommand command)
        {

            await Mediator.Send(command);

            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                Message = "Updated Successfully",
                Errors = ""
            });


        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {


            await Mediator.Send(new DeleteUOMConversionCommand { Id = id });

            return Ok(new { StatusCode = StatusCodes.Status200OK, message = "Deleted successfully.", errors = "" });


        }
        
         [HttpGet("convert")]
        public async Task<IActionResult> ConvertUOM([FromQuery] int fromUOMId, [FromQuery] int toUOMId, [FromQuery] decimal quantity)
        {
            var result = await Mediator.Send(new GetConvertedValueQuery
            {
                FromUOMId = fromUOMId,
                ToUOMId = toUOMId,
                Quantity = quantity
            });

            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                Message = result.Message,
                ConvertedValue = result.Data
            });
        }



       
    }
}