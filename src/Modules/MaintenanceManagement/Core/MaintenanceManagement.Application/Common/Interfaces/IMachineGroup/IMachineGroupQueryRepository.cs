using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MaintenanceManagement.Application.Common.Interfaces.IMachineGroup
{
    public interface IMachineGroupQueryRepository
    {

       

        Task<(List<MaintenanceManagement.Domain.Entities.MachineGroup>,int)> GetAllMachineGroupsAsync(int PageNumber, int PageSize, string? SearchTerm);
        Task<MaintenanceManagement.Domain.Entities.MachineGroup> GetByIdAsync(int id);
        Task<bool> GetByMachineGroupCodeAsync(string GroupName ,int Id);


        Task<bool> GetByMachineGroupnameAsync(string GroupName);

         Task<List<MaintenanceManagement.Domain.Entities.MachineGroup>> GetMachineGroupAutoComplete(string searchPattern);
          Task<bool> NotFoundAsync(int id );

        Task<bool> FKColumnExistValidation(int machineGroupId);
    //  Task<bool> GetByMachineGroupCodeAsync(string groupName); 

       // Task<bool> ExistsByMachineGroupNameAsync(string? GroupName);
        Task<bool> IsMachineGroupLinkedAsync(int id);  
        
    }

    
}