namespace UserManagement.Application.Common.Interfaces.IMiscMaster
{
    public interface IMiscMasterQueryRepository
    {
        Task<(List<UserManagement.Domain.Entities.MiscMaster>, int)> GetAllMiscMasterAsync(int PageNumber, int PageSize, string? SearchTerm);
        Task<UserManagement.Domain.Entities.MiscMaster> GetByIdAsync(int id);
        Task<List<UserManagement.Domain.Entities.MiscMaster>> GetMiscMaster(string searchPattern, string miscTypeCode);
        Task<UserManagement.Domain.Entities.MiscMaster?> GetByMiscMasterCodeAsync(string name, int? id = null);

        // Task<bool> AlreadyExistsAsync(string code,int? id = null);
        Task<bool> AlreadyExistsAsync(string code, int miscTypeId, int? id = null);
        Task<bool> NotFoundAsync(int id);
        Task<bool> FKColumnValidation(int ShiftMasterId);
        Task<UserManagement.Domain.Entities.MiscMaster> GetMiscMasterByName(string miscTypeCode, string miscTypeName);

        /// <summary>Returns true if any non-deleted record references this MiscMaster (delete guard).</summary>
        Task<bool> SoftDeleteValidationAsync(int id);

        /// <summary>Returns true if any active record references this MiscMaster (inactivate guard).</summary>
        Task<bool> IsMiscMasterLinkedAsync(int id);
    }
}