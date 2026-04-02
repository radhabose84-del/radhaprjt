namespace InventoryManagement.Application.Common.Interfaces.IMiscMaster
{
    public interface IMiscMasterQueryRepository
    {
        Task<(List<InventoryManagement.Domain.Entities.MiscMaster>, int)> GetAllMiscMasterAsync(int PageNumber, int PageSize, string? SearchTerm);
        Task<InventoryManagement.Domain.Entities.MiscMaster> GetByIdAsync(int id);
        Task<List<InventoryManagement.Domain.Entities.MiscMaster>> GetMiscMaster(string searchPattern, string? miscTypeCode,string? miscTypeDesc);
        Task<InventoryManagement.Domain.Entities.MiscMaster?> GetByMiscMasterCodeAsync(string name, int? id = null);

        // Task<bool> AlreadyExistsAsync(string code,int? id = null);
        Task<bool> AlreadyExistsAsync(string code, int miscTypeId, int? id = null);
        Task<bool> NotFoundAsync(int id);
        Task<bool> FKColumnValidation(int ShiftMasterId);
        Task<InventoryManagement.Domain.Entities.MiscMaster> GetMiscMasterByName(string miscTypeCode, string miscTypeName);
        Task<bool> SoftDeleteValidation(int id);
        Task<bool> IsMiscMasterLinkedAsync(int id);
    }
}