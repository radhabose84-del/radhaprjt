namespace UserManagement.Application.Common.Interfaces.IIconMaster
{
    public interface IIconMasterCommandRepository
    {
        Task<int> CreateAsync(UserManagement.Domain.Entities.IconMaster iconMaster);
        Task<bool> ExistsByKeywordAsync(string keyword);
        Task<int> UpdateAsync(int id, UserManagement.Domain.Entities.IconMaster iconMaster);
        Task<int> DeleteIconMasterAsync(int id, UserManagement.Domain.Entities.IconMaster iconMaster);
    }
}
