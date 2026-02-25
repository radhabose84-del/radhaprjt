using MaintenanceManagement.Domain.Entities;

namespace MaintenanceManagement.Application.Common.Interfaces.IShiftMasterDetail
{
    public interface IShiftMasterDetailQuery
    {
         Task<(IEnumerable<dynamic>,int)> GetAllShiftMasterDetailAsync(int PageNumber, int PageSize, string? SearchTerm);
        Task<ShiftMasterDetail> GetByIdAsync(int ShiftMasterId);
        Task<List<ShiftMasterDetail>> GetShiftMasterDetail(string searchPattern);
        Task<bool> SoftDeleteValidation(int Id); 
        Task<bool> AlreadyExistsAsync(int ShiftMasterId);
        Task<bool> NotFoundAsync(int Id );
        Task<bool> FKColumnValidation(int ShiftMasterId );
    }
}