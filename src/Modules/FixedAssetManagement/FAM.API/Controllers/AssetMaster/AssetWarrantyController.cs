using FAM.Application.AssetMaster.AssetWarranty.Commands.CreateAssetWarranty;
using FAM.Application.AssetMaster.AssetWarranty.Commands.DeleteAssetWarranty;
using FAM.Application.AssetMaster.AssetWarranty.Commands.DeleteFileAssetWarranty;
using FAM.Application.AssetMaster.AssetWarranty.Commands.UpdateAssetWarranty;
using FAM.Application.AssetMaster.AssetWarranty.Commands.UploadAssetWarranty;
using FAM.Application.AssetMaster.AssetWarranty.Queries.GetAssetWarranty;
using FAM.Application.AssetMaster.AssetWarranty.Queries.GetAssetWarrantyAutoComplete;
using FAM.Application.AssetMaster.AssetWarranty.Queries.GetAssetWarrantyById;
using FAM.Application.AssetMaster.AssetWarranty.Queries.GetWarrantyClaimStatus;
using FAM.Application.AssetMaster.AssetWarranty.Queries.GetWarrantyType;
using FAM.API.Validation.AssetMaster.AssetWarranty;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace FAM.API.Controllers.AssetMaster
{
    public class AssetWarrantyController : ApiControllerBase
    {
        
    public AssetWarrantyController(ISender mediator
                             ) 
        : base(mediator)
        {        
        }
        [HttpGet]                
        public async Task<IActionResult> GetAllAssetWarrantyAsync([FromQuery] int PageNumber,[FromQuery] int PageSize,[FromQuery] string? SearchTerm = null)
        {            
            var WarrantyMaster = await Mediator.Send(
            new GetAssetWarrantyQuery
            {
                PageNumber = PageNumber, 
                PageSize = PageSize, 
                SearchTerm = SearchTerm
            });
            return Ok(new 
            { 
                StatusCode=StatusCodes.Status200OK, 
                message = WarrantyMaster.Message,
                data = WarrantyMaster.Data.ToList(),
                TotalCount = WarrantyMaster.TotalCount,
                PageNumber = WarrantyMaster.PageNumber,
                PageSize = WarrantyMaster.PageSize
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
                    StatusCode=StatusCodes.Status400BadRequest,
                    message = "Invalid WarrantyMaster Id" 
                });
            }
            var result = await Mediator.Send(new GetAssetWarrantyByIdQuery { Id = id });            
        
            return Ok(new 
            {
                StatusCode=StatusCodes.Status200OK,
                data = result
            });   
        }

        [HttpPost]               
        public async Task<IActionResult> CreateAsync(CreateAssetWarrantyCommand  command)
        { 
                  
            var result = await Mediator.Send(command);
          
                return Ok(new 
                { 
                    StatusCode=StatusCodes.Status201Created,
                    message = "AssetWarranty Created Successfully", 
                    data = result
                });
           
        }
        [HttpPut]        
        public async Task<IActionResult> UpdateAsync(UpdateAssetWarrantyCommand command)
        {         
                    
            var result = await Mediator.Send(command);
           
                return Ok(new 
                {   StatusCode=StatusCodes.Status200OK,
                    message = "AssetWarranty Updated Successfully", 
                    asset = result
                });
          
                
        }
        [HttpDelete("{id}")]        
        public async Task<IActionResult> DeleteAsync(int id)
        {   
            
                 
            if (id <= 0)
            {
                return BadRequest(new
                {
                    StatusCode = StatusCodes.Status400BadRequest,
                    message = "Invalid Asset ID"
                });
            }            
             await Mediator.Send(new DeleteAssetWarrantyCommand { Id = id });                 
           
            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                data =$"Asset Warranty ID {id} Deleted" ,
                message = "Asset Warranty Deleted Successfully"
            });
        }
            
        [HttpGet("by-name")]  
        public async Task<IActionResult> GetAssetWarranty([FromQuery] string? name)
        {          
            var result = await Mediator.Send(new GetAssetWarrantyAutoCompleteQuery {SearchPattern = name}); // Pass `searchPattern` to the constructor
        
            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                message = result,
                data = result
            });
        } 
        [HttpPost("upload-logo")]
        public async Task<IActionResult> UploadLogo(UploadFileAssetWarrantyCommand uploadFileCommand)
        {
            
            var file = await Mediator.Send(uploadFileCommand);
           

           return Ok(new 
            { 
                StatusCode=StatusCodes.Status200OK, 
                message = "Logo uploaded successfully", 
                data = file,
                errors = "" 
            });
              

        }
        [HttpDelete("delete-logo")]
        public async Task<IActionResult> DeleteLogo(DeleteFileAssetWarrantyCommand deleteFileCommand)
        {
             await Mediator.Send(deleteFileCommand);
           
            return Ok(new 
            { 
                StatusCode=StatusCodes.Status200OK, 
                message = "Logo deleted successfully", 
                errors = "" 
            });
        }    
        [HttpGet("WarrantyType")]
        public async Task<IActionResult> GetManufactureTypes()
        {
            var result = await Mediator.Send(new GetWarrantyTypeQuery());

            if (result == null || result.Data == null || result.Data.Count == 0)
            {
                return NotFound(new
                {
                    StatusCode = StatusCodes.Status404NotFound,
                    message = "No Warranty Type found."
                });
            }
            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                message = "Warranty Type fetched successfully.",
                data = result.Data
            });
        }    
        [HttpGet("WarrantyClaimStatus")]
        public async Task<IActionResult> GetWarrantyClaimStatus()
        {
            var result = await Mediator.Send(new GetWarrantyClaimStatusQuery());

            if (result == null || result.Data == null || result.Data.Count == 0)
            {
                return NotFound(new
                {
                    StatusCode = StatusCodes.Status404NotFound,
                    message = "No Warranty Claim Status found."
                });
            }
            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                message = "Warranty Claim Status fetched successfully.",
                data = result.Data
            });
        }        
    }    
}