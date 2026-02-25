using FAM.Application.AssetMaster.AssetInsurance.Commands.CreateAssetInsurance;
using FAM.Application.AssetMaster.AssetInsurance.Commands.DeleteAssetInsurance;
using FAM.Application.AssetMaster.AssetInsurance.Commands.UpdateAssetInsurance;
using FAM.Application.AssetMaster.AssetInsurance.Queries.GetAssetInsurance;
using FAM.Application.AssetMaster.AssetInsurance.Queries.GetAssetInsuranceById;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace FAM.Presentation.Controllers.AssetMaster
{
    [Route("api/[controller]")]
    public class AssetInsuranceController : ApiControllerBase
    {            
       

        public AssetInsuranceController(ISender mediator
        ) : base(mediator)
        {
            
           
        }

        [HttpGet("{id}")]         
        public async Task<IActionResult> GetByAssetIdAsync(int id)
        {
             var assetInsurance = await Mediator.Send(new GetAssetInsuranceByIdQuery() { Id = id});
       
                
              return Ok(new { StatusCode=StatusCodes.Status200OK, data = assetInsurance,message = assetInsurance });
          
           
        }
        [HttpPost]
        public async Task<IActionResult> CreateAsync(CreateAssetInsuranceCommand command)
        {
           
            var response = await Mediator.Send(command);

                return StatusCode(StatusCodes.Status201Created, new 
                { 
                    StatusCode = StatusCodes.Status201Created,
                    Message = "AssetInsurance Created Successfully",
                    Data = response
                });
          
        }

        [HttpPut]
            public async Task<IActionResult> Update(UpdateAssetInsuranceCommand command)
            {
        
                 await Mediator.Send(command);
                
                    return Ok(new 
                    { 
                        StatusCode = StatusCodes.Status200OK, 
                        Message = "AssetInsurance Updated Successfully", 
                        Errors = "" 
                    });
               
            }

         [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
              if (id <= 0)
            {
                return BadRequest(new
                {
                    StatusCode = StatusCodes.Status400BadRequest,
                    message = "Invalid Asset ID"
                });
            }            
             await Mediator.Send(new DeleteAssetInsuranceCommand { Id = id });                 
            
            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                data =$"Asset Insurance ID {id} Deleted" ,
                message = "Asset Insurance Deleted Successfully"
            });
        }   

        [HttpGet]
        public async Task<IActionResult> GetAllAssetInsuranceAsync([FromQuery] int PageNumber, [FromQuery] int PageSize, [FromQuery] string? SearchTerm = null)
        {
            var assetInsurances = await Mediator.Send(
                new GetAssetInsuranceQuery
                {
                    PageNumber = PageNumber,
                    PageSize = PageSize,
                    SearchTerm = SearchTerm
                });

            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                Data = assetInsurances.Data,
                TotalCount = assetInsurances.TotalCount,
                PageNumber = assetInsurances.PageNumber,
                PageSize = assetInsurances.PageSize
            });
        }

    }
}