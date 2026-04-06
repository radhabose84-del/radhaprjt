namespace ProjectManagement.Application.Common.Interfaces.IMiscTypeMaster
{
    public interface IMiscTypeMasterQueryRepository
    {
        Task<(List<ProjectManagement.Domain.Entities.MiscTypeMaster>, int)> GetAllMiscTypeMasterAsync(int PageNumber, int PageSize, string? SearchTerm);
        Task<ProjectManagement.Domain.Entities.MiscTypeMaster> GetByIdAsync(int id);

        Task<List<ProjectManagement.Domain.Entities.MiscTypeMaster>> GetMiscTypeMaster(string searchPattern);

        Task<ProjectManagement.Domain.Entities.MiscTypeMaster?> GetByMiscTypeMasterCodeAsync(string name, int? id = null);

        Task<bool> AlreadyExistsAsync(string miscTypeCode, int? id = null);

        Task<bool> NotFoundAsync(int Id);

        Task<bool> SoftDeleteValidationAsync(int id);

        Task<bool> IsMiscTypeMasterLinkedAsync(int id);
    }
}