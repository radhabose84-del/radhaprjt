using FAM.Application.Common.Exceptions;
using FAM.Application.Common.HttpResponse;
using FAM.Application.DepreciationGroup.Commands.CreateDepreciationGroup;
using FAM.Application.DepreciationGroup.Commands.DeleteDepreciationGroup;
using FAM.Application.DepreciationGroup.Commands.UpdateDepreciationGroup;
using FAM.Application.DepreciationGroup.Queries.GetBookTypeQuery;
using FAM.Application.DepreciationGroup.Queries.GetDepreciationGroup;
using FAM.Application.DepreciationGroup.Queries.GetDepreciationGroupAutoComplete;
using FAM.Application.DepreciationGroup.Queries.GetDepreciationGroupById;
using FAM.Application.DepreciationGroup.Queries.GetDepreciationMethodQuery;
using FAM.Application.MiscMaster.Queries.GetMiscMaster;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace FAM.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DepreciationGroupController  : ApiControllerBase
    {
        private readonly IValidator<CreateDepreciationGroupCommand> _createDepreciationGroupCommandValidator;
        private readonly IValidator<UpdateDepreciationGroupCommand> _updateDepreciationGroupCommandValidator;
        private readonly IValidator<DeleteDepreciationGroupCommand> _deleteDepreciationGroupCommandValidator;
         
         
       public DepreciationGroupController(ISender mediator, 
                             IValidator<CreateDepreciationGroupCommand> createDepreciationGroupCommandValidator, 
                             IValidator<UpdateDepreciationGroupCommand> updateDepreciationGroupCommandValidator,
                             IValidator<DeleteDepreciationGroupCommand> deleteDepreciationGroupCommandValidator) 
        : base(mediator)
        {        
            _createDepreciationGroupCommandValidator = createDepreciationGroupCommandValidator;    
            _updateDepreciationGroupCommandValidator = updateDepreciationGroupCommandValidator;                 
            _deleteDepreciationGroupCommandValidator = deleteDepreciationGroupCommandValidator;     
        }
        [HttpGet]                
        public async Task<IActionResult> GetAllDepreciationGroupsAsync([FromQuery] int PageNumber,[FromQuery] int PageSize,[FromQuery] string? SearchTerm = null)
        {            
            var (data, totalCount) = await Mediator.Send(new GetDepreciationGroupQuery
            {
                PageNumber = PageNumber,
                PageSize = PageSize,
                SearchTerm = SearchTerm
            });

            var response = new ApiResponseDTO<List<DepreciationGroupDTO>>
            {
                IsSuccess = true,
                Message = "Depreciation groups retrieved successfully.",
                Data = data,
                TotalCount = totalCount,
                PageNumber = PageNumber,
                PageSize = PageSize,
                StatusCode = StatusCodes.Status200OK
            };

            return Ok(response);
        }

        [HttpGet("{id}")]  
        [ActionName(nameof(GetByIdAsync))]        
        public async Task<IActionResult> GetByIdAsync(int id)
        {
             if (id <= 0)
                throw new ExceptionRules("Invalid DepreciationGroup ID. ID must be greater than 0.");

            var result = await Mediator.Send(new GetDepreciationGroupByIdQuery { Id = id });

            if (result is null)
                throw new EntityNotFoundException("DepreciationGroup", id);

            return Ok(new ApiResponseDTO<DepreciationGroupDTO>
            {
                IsSuccess = true,
                Message = "DepreciationGroup retrieved successfully.",
                Data = result,
                StatusCode = StatusCodes.Status200OK
            });
        }

        [HttpPost]               
        public async Task<IActionResult> CreateAsync(CreateDepreciationGroupCommand  command)
        { 
            var validationResult = await _createDepreciationGroupCommandValidator.ValidateAsync(command);
            if (!validationResult.IsValid)
            {
                return BadRequest(new  ApiResponseDTO<object>
                {
                    IsSuccess = false,
                    Message = "Validation failed",
                    Errors = validationResult.Errors.Select(e => e.ErrorMessage).ToList()
                });
            }        
            var result = await Mediator.Send(command);          
            return Ok(new ApiResponseDTO<string>
            {
                StatusCode = StatusCodes.Status200OK,
                IsSuccess = true,
                Message = "DepreciationGroup created successfully."                
            });
        }
        [HttpPut]        
        public async Task<IActionResult> UpdateAsync(UpdateDepreciationGroupCommand command)
        {         
            var validationResult = await _updateDepreciationGroupCommandValidator.ValidateAsync(command);
            if (!validationResult.IsValid)
            {
               return BadRequest(new  ApiResponseDTO<object>
                {
                    IsSuccess = false,
                    Message = "Validation failed",
                    Errors = validationResult.Errors.Select(e => e.ErrorMessage).ToList()
                });
            }     
            var result = await Mediator.Send(command);
            return Ok(new ApiResponseDTO<string>
            {
                StatusCode = StatusCodes.Status200OK,
                IsSuccess = true,
                Message = "DepreciationGroup updated successfully.",
                Data = $"DepreciationGroup ID {command.Id} updated."
            });                
        }
        [HttpDelete("{id}")]        
        public async Task<IActionResult> DeleteAsync(int id)
        {      
            var command = new DeleteDepreciationGroupCommand { Id = id };
            var validationResult = await  _deleteDepreciationGroupCommandValidator.ValidateAsync(command);
            if (!validationResult.IsValid)
            {
                var errors = validationResult.Errors.Select(e => e.ErrorMessage).ToList();
                throw new ExceptionRules(string.Join(" | ", errors)); 
            }
            if (id <= 0)
            {
                throw new ExceptionRules("Invalid ID. ID must be greater than 0."); 
            }            
            var result = await Mediator.Send(command);                            
            return Ok(new ApiResponseDTO<string>
            {
                StatusCode = StatusCodes.Status200OK,
                IsSuccess = true,
                Message = "DepreciationGroup deleted successfully.",
                Data = $"DepreciationGroup ID {id} Deleted"
            });
        }
             
        [HttpGet("by-name")]  
        public async Task<IActionResult> GetDepreciationGroup([FromQuery] string? name)
        {                  
            var result = await Mediator.Send(new GetDepreciationGroupAutoCompleteQuery { SearchPattern = name });

            return Ok(new ApiResponseDTO<List<DepreciationGroupAutoCompleteDTO>>
            {
                IsSuccess = true,
                Message = "Fetched depreciation group.",
                Data = result,
                StatusCode = StatusCodes.Status200OK
            });
        }
       [HttpGet("bookType")]
        public async Task<IActionResult> GetBookTypes()
        {
            var result = await Mediator.Send(new GetBookTypeQuery());
            if (result == null ||  result.Count == 0)
            {
                throw new EntityNotFoundException("BookType", "All");
            }

            return Ok(new ApiResponseDTO<List<GetMiscMasterDto>>
            {
                StatusCode = StatusCodes.Status200OK,
                IsSuccess = true,
                Message = "Book Types fetched successfully.",
                Data = result
            });               
        }
        [HttpGet("DepMethod")]
        public async Task<IActionResult> GetDepreciationMethods()
        {
            var result = await Mediator.Send(new GetDepreciationMethodQuery());
            if (result == null || result.Count == 0)
            {
                throw new EntityNotFoundException("Depreciation Method", "All");
            }
            return Ok(new ApiResponseDTO<List<GetMiscMasterDto>>
            {
                StatusCode = StatusCodes.Status200OK,
                IsSuccess = true,
                Message = "Book Types fetched successfully.",
                Data = result
            });
        }
    }
}