using MaintenanceManagement.Application.MachineMaster.Command.CreateMachineMaster;
using MaintenanceManagement.Application.MachineMaster.Command.DeleteMachineMaster;
using MaintenanceManagement.Application.MachineMaster.Command.UpdateMachineMaster;
using MaintenanceManagement.Application.MachineMaster.Queries.GetAssetSpecificationById;
using MaintenanceManagement.Application.MachineMaster.Queries.GetMachineDepartmentbyId;
using MaintenanceManagement.Application.MachineMaster.Queries.GetMachineLineNo;
using MaintenanceManagement.Application.MachineMaster.Queries.GetMachineMaster;
using MaintenanceManagement.Application.MachineMaster.Queries.GetMachineMasterAutoComplete;
using MaintenanceManagement.Application.MachineMaster.Queries.GetMachineMasterById;
using MaintenanceManagement.Application.MachineMaster.Queries.GetMachineNoDepartmentbyId;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace MaintenanceManagement.Presentation.Controllers
{
    [Route("api/[controller]")]
    public class MachineController : ApiControllerBase
    {
        
        private readonly IMediator _mediator;


        public MachineController( IMediator mediator)
        : base(mediator)
        {
            
            _mediator = mediator;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllMachineMasterAsync([FromQuery] string? SearchTerm = null)
        {
            var MachineMaster = await Mediator.Send(
             new GetMachineMasterQuery
             {
               
                 SearchTerm = SearchTerm
             });
            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                data = MachineMaster.Data,
                TotalCount = MachineMaster.TotalCount,
              
            });
        }

        [HttpGet("by-name")]
        public async Task<IActionResult> GetMachineMasterAsync([FromQuery] string? Typename)
        {
            var MachineMaster = await Mediator.Send(new GetMachineMasterAutoCompleteQuery
            {
                SearchPattern = Typename ?? string.Empty
            });

            return Ok(new { StatusCode = StatusCodes.Status200OK, data = MachineMaster });
        }

        [HttpGet("{id}")]
        [ActionName(nameof(GetByIdAsync))]
        public async Task<IActionResult> GetByIdAsync(int id)
        {
            var machine = await Mediator.Send(new GetMachineMasterByIdQuery() { Id = id });

           
                return Ok(new { StatusCode = StatusCodes.Status200OK, data = machine, message = machine });
            
        }

        [HttpPost]
        public async Task<IActionResult> CreateAsync(CreateMachineMasterCommand createMachineMasterCommand)
        {


            // Process the command
            var CreateMachineId = await _mediator.Send(createMachineMasterCommand);

                return Ok(new
                {
                    StatusCode = StatusCodes.Status201Created,
                    message = "Created successfully.",
                    data = CreateMachineId
                });
            

        }
        [HttpPut]
        public async Task<IActionResult> UpdateAsync(UpdateMachineMasterCommand updateMachineMasterCommand)
        {

             await _mediator.Send(updateMachineMasterCommand);

                return Ok(new
                {
                    message = "Updated successfully.",
                    statusCode = StatusCodes.Status200OK
                });
            
        }

        [HttpDelete]
        public async Task<IActionResult> DeleteMachineMasterAsync(int id)
        {

            // Process the delete command
             await _mediator.Send(new DeleteMachineMasterCommand { Id = id });

                return Ok(new
                {
                    message = "Deleted successfully.",
                    statusCode = StatusCodes.Status200OK
                });

        }

        [HttpGet("MachineLineNo")]
        public async Task<IActionResult> GetMachineLineNo()
        {
            var result = await Mediator.Send(new GetMachineLinenoQuery());

            if (result == null || result.Data == null || result.Data.Count == 0)
            {
                return NotFound(new
                {
                    StatusCode = StatusCodes.Status404NotFound,
                    message = "No MachineLineNo found."
                });
            }
            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                message = "MachineLineNo fetched successfully.",
                data = result.Data
            });
        }


        [HttpGet("MachineGroup/{MachineGroupId}")]
        [ActionName(nameof(GetMachineDepartmentByIdAsync))]
        public async Task<IActionResult> GetMachineDepartmentByIdAsync(int MachineGroupId)
        {
            var machine = await Mediator.Send(new GetMachineDepartmentbyIdQuery() { MachineGroupId = MachineGroupId });


                return Ok(new { StatusCode = StatusCodes.Status200OK, data = machine, message = machine });
            
        }
        [HttpGet("GetByAssetId/{assetId}")]
        public async Task<IActionResult> GetByAssetId(int assetId)
        {
            var result = await _mediator.Send(new GetAssetSpecificationByIdQuery { AssetId = assetId });
            return Ok(new { StatusCode = StatusCodes.Status200OK, data = result, message = "Asset fetched successfully." });
        }

        [HttpGet("GetMachinesByDepartmentId/{departmentId}")]
        public async Task<IActionResult> GetMachinesByDepartmentId(int departmentId)
        {
            var result = await _mediator.Send(new GetMachineNoDepartmentbyIdQuery { DepartmentId = departmentId });
            return Ok(new { StatusCode = StatusCodes.Status200OK, data = result, message = "Machines fetched successfully." });
        }

        
    }
}