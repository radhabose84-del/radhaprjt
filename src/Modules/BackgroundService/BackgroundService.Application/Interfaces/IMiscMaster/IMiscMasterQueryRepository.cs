
namespace BackgroundService.Application.Interfaces.IMiscMaster
{
    public interface IMiscMasterQueryRepository
    {
        Task<(List<Domain.Entities.Notification.MiscMaster>, int)> GetAllMiscMasterAsync(int PageNumber, int PageSize, string? SearchTerm);
        Task<Domain.Entities.Notification.MiscMaster> GetByIdAsync(int id);
        Task<List<Domain.Entities.Notification.MiscMaster>> GetMiscMaster(string searchPattern, string miscTypeCode);
        Task<Domain.Entities.Notification.MiscMaster?> GetByMiscMasterCodeAsync(string name, int? id = null);

        // Task<bool> AlreadyExistsAsync(string code,int? id = null);
        Task<bool> AlreadyExistsAsync(string code, int miscTypeId, int? id = null);
        Task<bool> NotFoundAsync(int id);
        Task<bool> FKColumnValidation(int ShiftMasterId);
        Task<Domain.Entities.Notification.MiscMaster> GetMiscMasterByName(string miscTypeCode, string miscTypeName); 
    }
}