using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using PurchaseManagement.Application.PaymentTermMaster.Command.CreatePaymentTermMaster;
using PurchaseManagement.Application.PaymentTermMaster.Command.DeletePaymentTermMaster;
using PurchaseManagement.Application.PaymentTermMaster.Command.UpdatePaymentTermMaster;
using PurchaseManagement.Application.PaymentTermMaster.Queries.GetAllPaymentTermMaster;
using PurchaseManagement.Application.PaymentTermMaster.Queries.GetPaymentTermAutoComplete;
using PurchaseManagement.Application.PaymentTermMaster.Queries.GetPaymentTermMasterById;
using MassTransit.Mediator;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;

namespace PurchaseManagement.API.Controllers
{
    [Route("api/[controller]")]

    public class PaymentTermMasterController : ApiControllerBase
    {

        public PaymentTermMasterController(ISender mediator) : base(mediator)
        {



        }

        [HttpGet]
        public async Task<IActionResult> GetAllMiscMasterAsync([FromQuery] int PageNumber, [FromQuery] int PageSize, [FromQuery] string? SearchTerm = null)
        {
            var paymentterm = await Mediator.Send(
            new GetAllPaymentTermMasterQuery
            {
                PageNumber = PageNumber,
                PageSize = PageSize,
                SearchTerm = SearchTerm
            });
            // var activecompanies = companies.Data.ToList(); 

            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                data = paymentterm.Data,
                TotalCount = paymentterm.TotalCount,
                PageNumber = paymentterm.PageNumber,
                PageSize = paymentterm.PageSize
            });
        }


        [HttpGet("{id}")]
        [ActionName(nameof(GetByIdAsync))]
        public async Task<IActionResult> GetByIdAsync(int id)
        {

            var paymentTermMaster = await Mediator.Send(new GetPaymentTermMasterByIdQuery() { Id = id });

            return Ok(new { StatusCode = StatusCodes.Status200OK, data = paymentTermMaster, message = "PaymentTermMaster fetched successfully." });


        }


        [HttpPost]
        public async Task<IActionResult> CreateAsync(CreatePaymentTermMasterCommand command)
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
        public async Task<IActionResult> UpdateAsync(UpdatePaymentTermMasterCommand command)
        {

            var response = await Mediator.Send(command);
            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                message = "Updated Successfully",
                errors = "",
                data = response
            });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            await Mediator.Send(new DeletePaymentTermMasterCommand { Id = id });

            return Ok(new { StatusCode = StatusCodes.Status200OK, message = "Deleted successfully.", errors = "" });

        }  
        
        [HttpGet("by-name")]
        public async Task<IActionResult> GetPaymentTermMaster([FromQuery] string? searchPattern ,[FromQuery] string? paymentTermCode)
        {
          
            var paymentTermMaster = await Mediator.Send(new GetPaymentTermAutoCompleteQuery {SearchPattern = searchPattern, PaymentTermCode=paymentTermCode});
            
            return Ok( new { StatusCode=StatusCodes.Status200OK, data = paymentTermMaster });
            
        }
    }
}