
using MaintenanceManagement.Application.MachineGroupUser.Queries.GetMachineGroupUser;
using MaintenanceManagement.Application.MachineGroupUser.Queries.GetMachineGroupUserAutoComplete;

namespace MaintenanceManagement.Application.Common.Interfaces.IMachineGroupUser
{
    public interface IMachineGroupUserQueryRepository
    {
        Task<(List<MachineGroupUserDto>,int)> GetAllMachineGroupUserAsync(int PageNumber, int PageSize, string? SearchTerm);
        Task<MachineGroupUserDto> GetByIdAsync(int id);     
        Task<List<MachineGroupUserAutoCompleteDto>> GetMachineGroupUserByName(string searchPattern);   
        /* Task<bool> SoftDeleteValidation(int Id);  */
        Task<bool> AlreadyExistsAsync(int machineGroupId,int departmentId,int UserId,int? id = null);
        Task<bool> NotFoundAsync(int id );
    }
}