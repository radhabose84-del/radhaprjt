using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MaintenanceManagement.Application.ActivityMaster.Queries.GetActivityByMachinGroupId;
using MaintenanceManagement.Application.ActivityMaster.Queries.GetAllActivityMaster;
using MaintenanceManagement.Application.ActivityMaster.Queries.GetMachineGroupById;
using MaintenanceManagement.Application.MachineGroup.Queries.GetMachineGroupById;

namespace MaintenanceManagement.Application.Common.Interfaces.IActivityMaster
{
    public interface IActivityMasterQueryRepository
    {

        //Task<(List<Core.Domain.Entities.ActivityMaster>,int)> GetAllActivityMasterAsync(int PageNumber, int PageSize, string? SearchTerm);

        //  Task<(List<GetAllActivityMasterDto>, int)> GetAllActivityMasterAsync(int PageNumber, int PageSize ,string? SearchTerm );

        Task<(List<GetAllActivityMasterDto>, int)> GetAllActivityMasterAsync(int PageNumber, int PageSize, string? SearchTerm);

        //  Task<Core.Domain.Entities.ActivityMaster> GetByIdAsync(int id);

        Task<GetActivityMasterByIdDto> GetByIdAsync(int id);

        Task<List<GetMachineGroupNameByIdDto>> GetMachineGroupById(int activityId);

        // Task<List<Core.Domain.Entities.ActivityMaster>> GetActivityMasterAutoComplete(string searchPattern);
        Task<List<MaintenanceManagement.Domain.Entities.ActivityMaster>> GetActivityMasterAutoComplete(string searchPattern , string machineCode = null);


        //Task<bool> GetByActivityNameAsync(string activityname );
        Task<bool> GetByActivityNameAsync(string activityName, int? activityId = null);
        Task<bool> FKColumnExistValidation(int activityId);

        Task<List<MaintenanceManagement.Domain.Entities.MiscMaster>> GetActivityTypeAsync();

        Task<List<GetActivityByMachineGroupDto>> GetActivityByMachinGroupId(int MachineGroupId);
        Task<bool> IsActivityMasterLinkedAsync(int id);
           
        
    }
}