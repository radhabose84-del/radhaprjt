
namespace BackgroundService.Application.Common.Interfaces.IMiscTypeMaster
{
    public interface IMiscTypeMasterQueryRepository
    {
         Task<(List<Domain.Entities.Notification.MiscTypeMaster>,int)> GetAllMiscTypeMasterAsync(int PageNumber, int PageSize, string? SearchTerm);
        Task<Domain.Entities.Notification.MiscTypeMaster> GetByIdAsync(int id);

        Task<List<Domain.Entities.Notification.MiscTypeMaster>> GetMiscTypeMaster(string searchPattern);

        Task<Domain.Entities.Notification.MiscTypeMaster?> GetByMiscTypeMasterCodeAsync(string name,int? id = null);

        Task<bool> AlreadyExistsAsync(string miscTypeCode,int? id = null);

        Task<bool> NotFoundAsync(int Id );

        Task<bool> SoftDeleteValidation(int Id); 
    }
}