namespace PurchaseManagement.Application.Common.Interfaces.IMiscMaster
{
    public interface IMiscMasterQueryRepository
    {
        Task<(List<PurchaseManagement.Domain.Entities.MiscMaster>, int)> GetAllMiscMasterAsync(int PageNumber, int PageSize, string? SearchTerm);
        Task<PurchaseManagement.Domain.Entities.MiscMaster> GetByIdAsync(int id);
        Task<List<PurchaseManagement.Domain.Entities.MiscMaster>> GetMiscMaster(string searchPattern, string miscTypeCode);
        Task<PurchaseManagement.Domain.Entities.MiscMaster?> GetByMiscMasterCodeAsync(string name, int? id = null);

        // Task<bool> AlreadyExistsAsync(string code,int? id = null);
        Task<bool> AlreadyExistsAsync(string code, int miscTypeId, int? id = null);
        Task<bool> NotFoundAsync(int id);
        Task<bool> FKColumnValidation(int ShiftMasterId);
        Task<PurchaseManagement.Domain.Entities.MiscMaster> GetMiscMasterByName(string miscTypeCode, string miscTypeName);

        /// <summary>Returns the Description of the MiscType row (Purchase.MiscTypeMaster) for the given MiscTypeCode.</summary>
        Task<string?> GetMiscTypeDescriptionAsync(string miscTypeCode);

         Task<(int LocalId, int ImportId)> GetPoMethodIdsAsync(CancellationToken ct);
        Task<bool> IsValidPoMethodIdAsync(int id, CancellationToken ct);
        Task<bool> SoftDeleteValidation(int id);
        Task<bool> IsMiscMasterLinkedAsync(int id);
    }
}