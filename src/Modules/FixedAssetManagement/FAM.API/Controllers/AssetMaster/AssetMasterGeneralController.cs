using FAM.Application.AssetMaster.AssetMasterGeneral.Commands.CreateAssetMasterGeneral;
using FAM.Application.AssetMaster.AssetMasterGeneral.Commands.DeleteAssetMasterGeneral;
using FAM.Application.AssetMaster.AssetMasterGeneral.Commands.DeleteDocumentAssetMasterGeneral;
using FAM.Application.AssetMaster.AssetMasterGeneral.Commands.DeleteFileAssetMasterGeneral;
using FAM.Application.AssetMaster.AssetMasterGeneral.Commands.SaveAssetDocument;
using FAM.Application.AssetMaster.AssetMasterGeneral.Commands.UpdateAssetMasterGeneral;
using FAM.Application.AssetMaster.AssetMasterGeneral.Commands.UploadAssetMasterGeneral;
using FAM.Application.AssetMaster.AssetMasterGeneral.Commands.UploadDocumentAssetMaster;
using FAM.Application.AssetMaster.AssetMasterGeneral.Queries.GetAssetCodePattern;
using FAM.Application.AssetMaster.AssetMasterGeneral.Queries.GetAssetMasterByIdSplit;
using FAM.Application.AssetMaster.AssetMasterGeneral.Queries.GetAssetMasterGeneral;
using FAM.Application.AssetMaster.AssetMasterGeneral.Queries.GetAssetMasterGeneralAutoComplete;
using FAM.Application.AssetMaster.AssetMasterGeneral.Queries.GetAssetMasterGeneralById;
using FAM.Application.AssetMaster.AssetMasterGeneral.Queries.GetAssetParentMaster;
using FAM.Application.Common.Exceptions;
using FAM.Application.Common.HttpResponse;
using FAM.Application.DepreciationGroup.Queries.GetAssetTypeQuery;
using FAM.Application.DepreciationGroup.Queries.GetWorkingStatusQuery;
using FAM.Application.ExcelImport;
using FAM.Application.ExcelImport.PhysicalStockVerification;
using FluentValidation;
using MassTransit;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
namespace FAM.API.Controllers.AssetMaster
{
    [ApiController]
    [Route("api/[controller]")]
    public class AssetMasterGeneralController : ApiControllerBase
    {
        

        public AssetMasterGeneralController(
            ISender mediator
        )
        : base(mediator)
        {
           
        }

        [HttpGet]
        public async Task<IActionResult> GetAllAssetMasterGeneralAsync([FromQuery] int PageNumber, [FromQuery] int PageSize, [FromQuery] string? SearchTerm = null)

        {
            var assetMaster = await Mediator.Send(
                new GetAssetMasterGeneralQuery
                {
                    PageNumber = PageNumber,
                    PageSize = PageSize,
                    SearchTerm = SearchTerm
                });

            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                message = assetMaster.Message,
                data = assetMaster.Data.ToList(),
                TotalCount = assetMaster.TotalCount,
                PageNumber = assetMaster.PageNumber,
                PageSize = assetMaster.PageSize
            });
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetByIdAsync(int id)
        {
            if (id <= 0)
            {

                return BadRequest(new
                {
                    StatusCode = StatusCodes.Status400BadRequest,

                    message = "Invalid Asset ID"
                });
            }

            var result = await Mediator.Send(new GetAssetMasterGeneralByIdQuery { Id = id });
          
            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                data = result

            });
        }
        [HttpGet("{id}/split")]
        public async Task<IActionResult> GetByIdSplitAsync(int id)
        {
            if (id <= 0)
            {
                return BadRequest(new
                {
                    StatusCode = StatusCodes.Status400BadRequest,

                    message = "Invalid Asset ID"
                });
            }

            var result = await Mediator.Send(new GetAssetMasterByIdSplitQuery { Id = id });
         
            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                data = result

            });
        }
        [HttpPost]
        public async Task<IActionResult> CreateAsync(CreateAssetMasterGeneralCommand command)
        {

            var result = await Mediator.Send(command);
            return StatusCode(StatusCodes.Status201Created, new ApiResponseDTO<AssetMasterDto>
            {
                StatusCode = StatusCodes.Status201Created,
                IsSuccess = true,
                Message = "AssetMasterGeneral created successfully.",
                Data = result
            });
        }
        [HttpPut]
        public async Task<IActionResult> UpdateAsync(UpdateAssetMasterGeneralCommand command)

        {
           
            var result = await Mediator.Send(command);
         
                return Ok(new
                {
                    StatusCode = StatusCodes.Status200OK,

                    message = "AssetMasterGeneral updated successfully.",
                    asset = result
                });
           
        }
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAsync(int id)
        {
            var command = new DeleteAssetMasterGeneralCommand { Id = id };
          
             await Mediator.Send(command);
            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                message = $"AssetMasterGeneral ID {id} deleted successfully."
            });
        }

        // GET: api/AssetMasterGeneral/by-name?name=...

        [HttpGet("by-name")]
        public async Task<IActionResult> GetAssetName([FromQuery] string? name)

        {
            var result = await Mediator.Send(new GetAssetMasterGeneralAutoCompleteQuery { SearchPattern = name });
           
            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                message = result,
                data = result
            });
        }

        // GET: api/AssetMasterGeneral/AssetType
        [HttpGet("AssetType")]
        public async Task<IActionResult> GetAssetTypes()
        {
            var result = await Mediator.Send(new GetAssetTypeQuery());
            if (result == null || result.Data == null || result.Data.Count == 0)
            {
                return NotFound(new
                {
                    StatusCode = StatusCodes.Status404NotFound,
                    message = "No Asset Types found."
                });
            }
            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                message = "Asset Types fetched successfully.",
                data = result.Data
            });
        }
        // GET: api/AssetMasterGeneral/WorkingStatus
        [HttpGet("ParentAsset")]
        public async Task<IActionResult> GetParentAsset([FromQuery] string assetType)
        {
            var result = await Mediator.Send(new GetAssetParentMasterQuery { AssetType = assetType });
          
            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                message = result,
                data = result
            });
        }
        // GET: api/AssetMasterGeneral/WorkingStatus
        [HttpGet("WorkingStatus")]
        public async Task<IActionResult> GetWorkingStatus()
        {
            var result = await Mediator.Send(new GetWorkingStatusQuery());
            if (result == null || result.Data == null || result.Data.Count == 0)
            {
                return NotFound(new
                {
                    StatusCode = StatusCodes.Status404NotFound,
                    message = "No Working Status found."
                });
            }
            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                message = "Working Status fetched successfully.",
                data = result.Data
            });
        }
        // GET: api/AssetMasterGeneral/AssetType
        [HttpGet("AssetCodePattern")]
        public async Task<IActionResult> GetAssetCodePattern()
        {
            var result = await Mediator.Send(new GetAssetCodePatternQuery());
            if (result == null || result.Data == null || result.Data.Count == 0)
            {
                return NotFound(new
                {
                    StatusCode = StatusCodes.Status404NotFound,
                    message = "No Asset Pattern found."
                });
            }
            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                message = "Asset Pattern fetched successfully.",
                data = result.Data
            });
        }

        // POST: api/AssetMasterGeneral/upload-logo
        [HttpPost("upload-logo")]
        public async Task<IActionResult> UploadLogo(UploadFileAssetMasterGeneralCommand uploadFileCommand)
        {
          
            var file = await Mediator.Send(uploadFileCommand);
         

            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                message = "File uploaded successfully.",
                data = file,
                errors = ""
            });
        }
        // DELETE: api/AssetMasterGeneral/delete-logo
        [HttpDelete("delete-logo")]

        public async Task<IActionResult> DeleteLogo([FromBody] DeleteFileAssetMasterGeneralCommand deleteFileCommand)
        {
           

            var file = await Mediator.Send(deleteFileCommand);
           

            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                message = file,
                errors = ""
            });
        }
        //Excel Import
        [HttpPost("import")]
        public async Task<IActionResult> Import([FromForm] ImportAssetDto dto)
        {
            if (dto.File == null || dto.File.Length == 0)
                return BadRequest("Please upload a valid Excel file.");

            var result = await Mediator.Send(new ImportAssetCommand(dto));

            if (result.IsSuccess && result.Data)
                return Ok(new
                {
                    StatusCode = StatusCodes.Status200OK,
                    message = result.Message,
                    errors = ""
                });

            return BadRequest(new
            {
                StatusCode = StatusCodes.Status400BadRequest,
                message = result.Message,
                errors = ""
            });
        }
        [HttpPost("upload-document")]
        public async Task<IActionResult> UploadDocument(UploadDocumentAssetMasterGeneralCommand uploadFileCommand)
        {
           
            var file = await Mediator.Send(uploadFileCommand);
          
            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                message = file,
                data = file,
                errors = ""
            });
        }
        // DELETE: api/AssetMasterGeneral/delete-logo
        [HttpDelete("delete-document")]
        public async Task<IActionResult> DeleteDocument([FromBody] DeleteDocumentAssetMasterGeneralCommand deleteFileCommand)
        {
            if (deleteFileCommand == null || string.IsNullOrWhiteSpace(deleteFileCommand.assetPath))
            {
                return BadRequest(new
                {
                    StatusCode = StatusCodes.Status400BadRequest,
                    message = "Invalid request. 'assetPath' cannot be null or empty.",
                    errors = ""
                });
            }
            var file = await Mediator.Send(deleteFileCommand);
          
            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                message = file,
                errors = ""
            });
        }
        [HttpPost("saveDocument")]
        public async Task<IActionResult> UpdateDocument([FromBody] SaveAssetDocumentCommand saveCommand)
        {
            if (saveCommand == null || string.IsNullOrWhiteSpace(saveCommand.assetPath))
            {
                return BadRequest(new
                {
                    StatusCode = StatusCodes.Status400BadRequest,
                    message = "Invalid request. 'assetPath' cannot be null or empty.",
                    errors = ""
                });
            }
            var saveDoc = await Mediator.Send(saveCommand);
           
            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                message = saveDoc,
                errors = ""
            });
        }
        [HttpPost("PhysicalAuditUpload")]
        [RequestSizeLimit(2 * 1024 * 1024)] // 2 MB
        public async Task<IActionResult> UploadExcel([FromForm] ImportAssetAuditDto dto)
        {
            if (dto == null || dto.File == null || dto.File.Length == 0)
            {
                return BadRequest(new ApiResponseDTO<bool>
                {
                    IsSuccess = false,
                    Message = "File is missing or empty.",
                    Data = false
                });
            }

            var command = new ImportAssetAuditCommand(dto);
            var result = await Mediator.Send(command);

            if (!result.IsSuccess)
            {
                return BadRequest(new
                {
                    StatusCode = StatusCodes.Status400BadRequest,
                    message = result.Message,
                    errors = ""
                });
            }

            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                message = result.Message,
                errors = ""
            });
        }

        [HttpPost("ScanAsset")]
        public async Task<IActionResult> ScanAsset([FromBody] ScanAssetAuditCommand command)
        {
            if (string.IsNullOrWhiteSpace(command.AssetCode))
                  return BadRequest(new ApiResponseDTO<bool>
                {
                    IsSuccess = false,
                    Message = "AssetCode is required.",
                    Data = false
                });

            var result = await Mediator.Send(command);

            if (result.IsSuccess)
            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                message = result.Message,
                errors = ""
            });

             return BadRequest(new
                {
                    StatusCode = StatusCodes.Status400BadRequest,
                    message = result.Message,
                    errors = ""
                });
        }


    }
}
