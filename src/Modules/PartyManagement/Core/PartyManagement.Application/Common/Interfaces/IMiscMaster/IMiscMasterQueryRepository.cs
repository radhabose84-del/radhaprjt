namespace PartyManagement.Application.Common.Interfaces.IMiscMaster
{
    public interface IMiscMasterQueryRepository
    {
        Task<(List<PartyManagement.Domain.Entities.MiscMaster>, int)> GetAllMiscMasterAsync(int PageNumber, int PageSize, string? SearchTerm);
        Task<PartyManagement.Domain.Entities.MiscMaster> GetByIdAsync(int id);
        Task<List<PartyManagement.Domain.Entities.MiscMaster>> GetMiscMaster(string searchPattern, string miscTypeCode);
        Task<PartyManagement.Domain.Entities.MiscMaster?> GetByMiscMasterCodeAsync(string name, int? id = null);

        // Task<bool> AlreadyExistsAsync(string code,int? id = null);
        Task<bool> AlreadyExistsAsync(string code, int miscTypeId, int? id = null);
        Task<bool> NotFoundAsync(int id);
        Task<bool> FKColumnValidation(int ShiftMasterId);
        Task<PartyManagement.Domain.Entities.MiscMaster> GetMiscMasterByName(string miscTypeCode, string miscTypeName);
        Task<bool> SoftDeleteValidationAsync(int id);
        Task<bool> IsMiscMasterLinkedAsync(int id);
    }
}