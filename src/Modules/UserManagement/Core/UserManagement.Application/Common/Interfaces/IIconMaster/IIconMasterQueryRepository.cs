namespace UserManagement.Application.Common.Interfaces.IIconMaster
{
    public interface IIconMasterQueryRepository
    {
        Task<(List<UserManagement.Domain.Entities.IconMaster>, int)> GetAllIconMasterAsync(int pageNumber, int pageSize, string? searchTerm);
        Task<UserManagement.Domain.Entities.IconMaster?> GetByIdAsync(int id);
        Task<List<UserManagement.Domain.Entities.IconMaster>> GetByKeywordAsync(string searchPattern);
    }
}
