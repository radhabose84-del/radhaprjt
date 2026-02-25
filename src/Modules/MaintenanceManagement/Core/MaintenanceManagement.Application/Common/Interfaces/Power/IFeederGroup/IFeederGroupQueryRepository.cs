using MaintenanceManagement.Domain.Entities.Power;

namespace MaintenanceManagement.Application.Common.Interfaces.Power.IFeederGroup
{
    public interface IFeederGroupQueryRepository
    {
        Task<(List<FeederGroup>, int)> GetAllFeederGroupAsync(int PageNumber, int PageSize, string? SearchTerm);

        //   Task<List<FeederGroup>> GetFeederGroupByIdAsync(int id);

        Task<FeederGroup> GetFeederGroupByIdAsync(int id);

        Task<bool> AlreadyExistsAsync(string feederGroupCode, int? id = null);

        Task<bool> NotFoundAsync(int id);

        Task<List<FeederGroup>> GetFeederGroupAutoComplete(string searchPattern);
        
         Task<bool> SoftDeleteValidation(int id); 
         Task<bool> IsFeederGroupLinkedAsync(int id);
         
   
    }
}