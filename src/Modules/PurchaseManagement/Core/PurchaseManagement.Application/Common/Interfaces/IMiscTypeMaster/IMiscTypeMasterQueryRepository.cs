namespace PurchaseManagement.Application.Common.Interfaces.IMiscTypeMaster
{
    public interface IMiscTypeMasterQueryRepository
    {
        Task<(List<PurchaseManagement.Domain.Entities.MiscTypeMaster>,int)> GetAllMiscTypeMasterAsync(int PageNumber, int PageSize, string? SearchTerm);
        Task<PurchaseManagement.Domain.Entities.MiscTypeMaster> GetByIdAsync(int id);

        Task<List<PurchaseManagement.Domain.Entities.MiscTypeMaster>> GetMiscTypeMaster(string searchPattern);

        Task<PurchaseManagement.Domain.Entities.MiscTypeMaster?> GetByMiscTypeMasterCodeAsync(string name,int? id = null);

        Task<bool> AlreadyExistsAsync(string miscTypeCode,int? id = null);

        Task<bool> NotFoundAsync(int Id );

        Task<bool> SoftDeleteValidation(int Id);
        Task<bool> IsMiscTypeMasterLinkedAsync(int id);
    }
}